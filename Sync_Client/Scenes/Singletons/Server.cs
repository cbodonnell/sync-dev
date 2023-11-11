using Godot;
using Newtonsoft.Json;
using Shared;
using System;
using System.Collections.Generic;

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
	private ulong lastGameStateUpdate = 0;


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
	private void InstancePlayer(string id, string data) {
		if (!startedGame) return;
		GD.Print($"Instance player {id}: {data}");
		PlayerUpdate playerUpdate = JsonConvert.DeserializeObject<PlayerUpdate>(data);
		if (id == peer.GetUniqueId().ToString()) {
			Player player = Global.InstancePlayer(playerScene, id, playerUpdate);
        	GetNode<Node2D>("/root/World").AddChild(player);
		} else {
			OtherPlayer player = Global.InstanceOtherPlayer(otherPlayerScene, id, playerUpdate);
			GetNode<Node2D>("/root/World").AddChild(player);
		}

	}

	[Rpc(MultiplayerApi.RpcMode.Authority)]
	private void RemovePlayer(string id) {
		GD.Print($"Remove player {id}");
		CharacterBody2D player = GetNode<Node2D>("/root/World").GetNodeOrNull<CharacterBody2D>(id);
		if (player == null) {
			GD.PrintErr($"RemovePlayer: Player {id} not found");
			return;
		}
		player.QueueFree();
	}

	public void SendPlayerUpdate(PlayerUpdate playerUpdate) {
		// GD.Print($"Update position {position} {velocity} {flipH}");
		// TODO: send direction as well
		string data = JsonConvert.SerializeObject(playerUpdate);
		RpcId(0, nameof(RequestUpdatePlayer), data);
	}
	
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.UnreliableOrdered)]
	private void RequestUpdatePlayer(string data) {}

	[Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.UnreliableOrdered)]
	private void UpdateGameState(string data) {
		if (!startedGame) return;
		// GD.Print($"Update UpdateGameState {data}");
		GameState gameState = JsonConvert.DeserializeObject<GameState>(data);

		if (gameState.T < lastGameStateUpdate) {
			GD.PrintErr($"UpdateGameState: Received old game state {gameState.T} < {lastGameStateUpdate}");
			return;
		}
		lastGameStateUpdate = gameState.T;

		foreach (KeyValuePair<string, PlayerUpdate> entry in gameState.P)
		{
			string id = entry.Key;
			PlayerUpdate playerUpdate = entry.Value;
			if (id == peer.GetUniqueId().ToString()) {
				// TODO: handle within client-side prediction
			} else {
				OtherPlayer existingPlayer = GetNode<Node2D>("/root/World").GetNodeOrNull<OtherPlayer>(id);
				if (existingPlayer != null) {
					// GD.Print($"Update player {id}: {playerUpdate}");
					Global.UpdateOtherPlayer(existingPlayer, playerUpdate);
				} else {
					GD.Print($"UpdateGameState: Player {id} not found. Instancing...");
					// TODO: DRY this out
					OtherPlayer player = Global.InstanceOtherPlayer(otherPlayerScene, id, playerUpdate);
					GetNode<Node2D>("/root/World").AddChild(player);
				}
			}
		}
	}
}
