using Godot;
using System;

public partial class Server : Node
{

	[Export]
	private string address = "127.0.0.1";
	// private string address = "10.8.0.1";

	[Export]
	private int port = 9999;

	private PackedScene playerScene = (PackedScene)GD.Load("res://Scenes/Player/Player.tscn");
	private PackedScene otherPlayerScene = (PackedScene)GD.Load("res://Scenes/OtherPlayer/OtherPlayer.tscn");


	private ENetMultiplayerPeer peer = new ENetMultiplayerPeer();

	private ENetConnection.CompressionMode compressionMode = ENetConnection.CompressionMode.RangeCoder;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	public void ConnectToServer()
	{
		GD.Print("Connecting to " + address + ":" + port.ToString());
		var error = peer.CreateClient(address, port);
		if (error != Error.Ok)
		{
			GD.Print("Error creating client: " + error.ToString());
			return;
		}

		peer.Host.Compress(compressionMode);
		Multiplayer.ConnectedToServer += OnConnectedToServer;
		Multiplayer.ConnectionFailed += OnConnectionFailed;
		Multiplayer.MultiplayerPeer = peer;
	}

	private void OnConnectedToServer()
	{
		GD.Print("Connected to server");
		GetTree().ChangeSceneToFile("res://Scenes/World/World.tscn");
	}

	private void OnConnectionFailed()
	{
		GD.Print("Connection failed");
		GetNode<Label>("/root/Menu/Label").Text = "Connection failed";
	}

	[Rpc(MultiplayerApi.RpcMode.Authority)]
	private void InstancePlayer(long id, string name, Vector2 position, Vector2 velocity, float direction) {
		GD.Print($"Instance player {id}: {name} {position} {velocity} {direction}");
		PackedScene scene = id == peer.GetUniqueId() ? playerScene : otherPlayerScene;
		CharacterBody2D player = Global.InstancePlayer(scene, GetNode<Node2D>("/root/World"), position, velocity, direction);
		// this is the node name, not the player's name
		player.Name = id.ToString();
	}

	[Rpc(MultiplayerApi.RpcMode.Authority)]
	private void RemovePlayer(long id) {
		GD.Print($"Remove player {id}");
		CharacterBody2D player = GetNode<Node2D>("/root/World").GetNodeOrNull<CharacterBody2D>(id.ToString());
		if (player == null) {
			GD.PrintErr($"Player {id} not found");
			return;
		}
		player.QueueFree();
	}

	public void UpdatePosition(Vector2 position, Vector2 velocity, bool flipH) {
		// GD.Print($"Update position {position} {velocity} {flipH}");
		// TODO: send direction as well
		RpcId(0, nameof(RequestUpdatePosition), position, velocity, flipH);
	}
	
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.UnreliableOrdered)]
	private void RequestUpdatePosition(Vector2 position, Vector2 velocity, bool flipH) {}

	[Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.UnreliableOrdered)]
	private void UpdatePositionSuccess(long id, Vector2 position, Vector2 velocity, bool flipH) {
		// GD.Print($"Update position success {id} {position} {velocity} {flipH}");
		if (id == peer.GetUniqueId()) {
			// TODO: handle within client-side prediction
			return;
		} else {
			CharacterBody2D player = GetNode<Node2D>("/root/World").GetNodeOrNull<CharacterBody2D>(id.ToString());
			if (player == null) {
				GD.PrintErr($"Player {id} not found");
				return;
			}
			GetTree().CreateTween().TweenProperty(player, "position", position, 0.05f);
			player.Velocity = velocity;
			player.GetNode<AnimatedSprite2D>("AnimatedSprite2D").FlipH = flipH;
		}
	}
}
