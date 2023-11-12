using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Shared;
using Newtonsoft.Json;

public partial class Server : Node
{

	[Export]
	// private string address = "127.0.0.1";
	private string address = "0.0.0.0";

	[Export]
	private int port = 9999;

	private int maxClients = 32;

	private ENetMultiplayerPeer peer = new ENetMultiplayerPeer();

	private GameData gameData;
	
	private ENetConnection.CompressionMode compressionMode = ENetConnection.CompressionMode.RangeCoder;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		gameData = GetNode<GameData>("/root/GameData");

		var error = peer.CreateServer(port, maxClients);
		if (error != Error.Ok)
		{
			GD.Print("Error creating server: " + error.ToString());
			return;
		}

		peer.Host.Compress(compressionMode);

		Multiplayer.MultiplayerPeer = peer;
		
		Multiplayer.PeerConnected += OnPeerConnected;
		Multiplayer.PeerDisconnected += OnPeerDisconnected;

		GD.Print("Waiting for connections on port " + port.ToString());
	}

	public override void _PhysicsProcess(double delta)
	{
		if (gameData.PlayerUpdateCollection.Count == 0) {
			// no players connected, no need to run the game loop
			return;
		}
		// GD.Print("Running game loop");
		
		// TODO: validate positions

		// copy game data when using in the game loop
		Dictionary<string, PlayerUpdate> playerUpdatesCopy = gameData.PlayerUpdateCollection.ToDictionary(entry => entry.Key, entry => entry.Value);
		GameState gameState = new GameState(Time.GetUnixTimeFromSystem(), playerUpdatesCopy);

		// TODO: remove unnecessary data from the game state (e.g. client-side timestamps)

		string data = JsonConvert.SerializeObject(gameState);

		RpcId(0, nameof(UpdateGameState), data);
	}
	
	private void OnPeerConnected(long id)
	{
		GD.Print("Peer connected: " + id);
	}

	private void OnPeerDisconnected(long id)
	{
		GD.Print("Peer disconnected: " + id);
		gameData.RemovePlayer(id.ToString());
		RpcId(0, nameof(RemovePlayer), id.ToString());
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void FetchServerTime(double clientTime)
	{
		long id = Multiplayer.GetRemoteSenderId();
		double serverTime = Time.GetUnixTimeFromSystem();
		RpcId(id, nameof(SetServerTime), serverTime, clientTime);
	}

	[Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void SetServerTime(double serverTime, double clientTime) {}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void CheckLatency(double clientTime) {
		long id = Multiplayer.GetRemoteSenderId();
		RpcId(id, nameof(SetLatency), clientTime);
	}

	[Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void SetLatency(double clientTime) {}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void RequestSelectCharacter(string character) {
		int id = Multiplayer.GetRemoteSenderId();
		GD.Print($"RequestSelectCharacter: {id} {character}");
		PlayerUpdate playerUpdate = new PlayerUpdate(){
			P = new Vector2(500, 500),
			V = new Vector2(0, 0),
			F = false,
			C = character
		};
		string data = JsonConvert.SerializeObject(playerUpdate);
		RpcId(0, nameof(InstancePlayer), id.ToString(), data);
	}

	[Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void InstancePlayer(string id, string data) {}

	[Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void RemovePlayer(string id) {}
	
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.UnreliableOrdered)]
	private void RequestUpdatePlayer(string data) {
		int id = Multiplayer.GetRemoteSenderId();
		// GD.Print($"RequestUpdatePosition: {id} {position} {velocity} {flipH}");
		PlayerUpdate player = JsonConvert.DeserializeObject<PlayerUpdate>(data);
		gameData.ReceivePlayerUpdate(id.ToString(), player);
	}

	[Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.UnreliableOrdered)]
	private void UpdateGameState(string data) {}
}
