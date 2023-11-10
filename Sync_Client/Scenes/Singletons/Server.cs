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

	private bool startedGame = false;


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
		GetTree().ChangeSceneToFile("res://Scenes/CharacterSelect/CharacterSelect.tscn");
	}

	private void OnConnectionFailed()
	{
		GD.Print("Connection failed");
		GetNode<Label>("/root/Menu/Label").Text = "Connection failed";
	}

	public void SelectCharacter(string character) {
		GD.Print($"Request select character {character}");
		GetTree().ChangeSceneToFile("res://Scenes/World/World.tscn");
		startedGame = true;
		RpcId(0, nameof(RequestSelectCharacter), character);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void RequestSelectCharacter(string character) {}

	[Rpc(MultiplayerApi.RpcMode.Authority)]
	private void InstancePlayer(long id, string name, string character, Vector2 position, Vector2 velocity, float direction) {
		GD.Print($"Instance player {id}: {name} {character} {position} {velocity} {direction}");
		if (id == peer.GetUniqueId()) {
			Player player = Global.InstancePlayer<Player>(playerScene, id, character, position, velocity, direction);
			player.Character = character;
        	GetNode<Node2D>("/root/World").AddChild(player);
		} else {
			OtherPlayer player = Global.InstancePlayer<OtherPlayer>(otherPlayerScene, id, character, position, velocity, direction);
			player.Character = character;
			GetNode<Node2D>("/root/World").AddChild(player);
		}

	}

	[Rpc(MultiplayerApi.RpcMode.Authority)]
	private void RemovePlayer(long id) {
		GD.Print($"Remove player {id}");
		CharacterBody2D player = GetNode<Node2D>("/root/World").GetNodeOrNull<CharacterBody2D>(id.ToString());
		if (player == null) {
			GD.PrintErr($"RemovePlayer: Player {id} not found");
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
	private void UpdatePosition(long id, Vector2 position, Vector2 velocity, bool flipH) {
		if (!startedGame) return;
		// GD.Print($"Update position {id} {position} {velocity} {flipH}");
		if (id == peer.GetUniqueId()) {
			// TODO: handle within client-side prediction
			return;
		} else {
			OtherPlayer player = GetNode<Node2D>("/root/World").GetNodeOrNull<OtherPlayer>(id.ToString());
			if (player == null) {
				GD.PrintErr($"UpdatePosition: Player {id} not found");
				return;
			}
			// GD.Print($"Update player {id} [{player.Character}]: {position} {velocity} {flipH}");
			Global.UpdatePlayer(player, position, velocity, flipH);
		}
	}
}
