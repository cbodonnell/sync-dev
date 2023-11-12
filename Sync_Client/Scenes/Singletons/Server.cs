using Godot;
using Newtonsoft.Json;
using Shared;
using System;
using System.Collections.Generic;

public partial class Server : Node
{

	[Export]
	// private string address = "127.0.0.1";
	private string address = "10.8.0.1";

	[Export]
	private int port = 9999;

	private ENetMultiplayerPeer peer = new ENetMultiplayerPeer();

	private ENetConnection.CompressionMode compressionMode = ENetConnection.CompressionMode.RangeCoder;

	public double ClientClock = Time.GetUnixTimeFromSystem();
	private float decimalCollector = 0;
	private List<double> latencies = new List<double>();
	private double latency = 0;
	private double deltaLatency = 0;
	
	private bool startedGame = false;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	public override void _PhysicsProcess(double delta)
	{
		ClientClock += delta + deltaLatency;
		deltaLatency = 0;
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
		RpcId(1, nameof(FetchServerTime), Time.GetUnixTimeFromSystem());
		Timer timer = new Timer(){
			WaitTime = 0.5,
			Autostart = true,
		};
		timer.Timeout += DetermineLatency;
		AddChild(timer);
		GetTree().ChangeSceneToFile("res://Scenes/CharacterSelect/CharacterSelect.tscn");
	}

	private void OnConnectionFailed()
	{
		GD.Print("Connection failed");
		GetNode<Label>("/root/Menu/Label").Text = "Connection failed";
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void FetchServerTime(double clientTime) {}

	[Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void SetServerTime(double serverTime, double clientTime) {
		GD.Print($"Set server time {serverTime}");
		latency = (Time.GetUnixTimeFromSystem() - clientTime) / 2;
		ClientClock = serverTime + latency;
	}

	private void DetermineLatency() {
		RpcId(1, nameof(CheckLatency), Time.GetUnixTimeFromSystem());
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void CheckLatency(double clientTime) {}

	[Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void SetLatency(double clientTime) {
		double newLatency = (Time.GetUnixTimeFromSystem() - clientTime) / 2;
		latencies.Add(newLatency);
		if (latencies.Count == 9) {
			double totalLatency = 0;
			latencies.Sort();
			double medianLatency = latencies[4];
			// iterate over latencies in reverse
			for (int i = latencies.Count - 1; i >= 0; i--) {
				double latency = latencies[i];
				if (latency > medianLatency * 2 && latency > 0.020) {
					latencies.RemoveAt(i);
				} else {
					totalLatency += latency;
				}
			}
			deltaLatency = totalLatency / latencies.Count - latency;
			latency = totalLatency / latencies.Count;
			// GD.Print($"Latency: {latency}");
			latencies.Clear();
		}
	}

	public void SelectCharacter(string character) {
		GD.Print($"Request select character {character}");
		GetTree().ChangeSceneToFile("res://Scenes/World/World.tscn");
		startedGame = true;
		RpcId(0, nameof(RequestSelectCharacter), character);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void RequestSelectCharacter(string character) {}

	[Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void InstancePlayer(string id, string data) {
		if (!startedGame) return;
		GD.Print($"Instance player {id}: {data}");
		PlayerUpdate playerUpdate = JsonConvert.DeserializeObject<PlayerUpdate>(data);
		GetNode<World>("/root/World").InstancePlayer(id, playerUpdate);
	}

	[Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void RemovePlayer(string id) {
		if (!startedGame) return;
		GD.Print($"Remove player {id}");
		GetNode<World>("/root/World").RemovePlayer(id);
	}

	public void SendPlayerUpdate(PlayerUpdate playerUpdate) {
		// GD.Print($"Update position {position} {velocity} {flipH}");
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
		GetNode<World>("/root/World").UpdateGameState(gameState);
		GD.Print($"UpdateGameState: {gameState.T} && ClientClock: {ClientClock}");
	}

	public string GetUniqueId() {
		return peer.GetUniqueId().ToString();
	}
}
