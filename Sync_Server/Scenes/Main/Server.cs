using Godot;
using System;

public partial class Server : Node
{

	[Export]
	private string address = "127.0.0.1";
	// private string address = "0.0.0.0";

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
	
	private void OnPeerConnected(long id)
	{
		GD.Print("Peer connected: " + id);
		Player newPlayer = new Player(id, $"Player {id}", new Vector2(500, 500));
		gameData.AddPlayer(newPlayer);
		RpcId(0, nameof(InstancePlayer), newPlayer.GetId(), newPlayer.GetName(), newPlayer.GetPosition());
		gameData.Players.FindAll(p => p.GetId() != id).ForEach(player => {
			RpcId(id, nameof(InstancePlayer), player.GetId(), player.GetName(), player.GetPosition());
		});
	}

	private void OnPeerDisconnected(long id)
	{
		GD.Print("Peer disconnected: " + id);
		gameData.RemovePlayer(gameData.Players.Find(player => player.GetId() == id));
		RpcId(0, nameof(RemovePlayer), id);
	}

	[Rpc(MultiplayerApi.RpcMode.Authority)]
	private void InstancePlayer(long id, string name, Vector2 position) {}

	[Rpc(MultiplayerApi.RpcMode.Authority)]
	private void RemovePlayer(long id) {}
	
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.UnreliableOrdered)]
	private void RequestUpdatePosition(Vector2 position) {
		int id = Multiplayer.GetRemoteSenderId();
		// TODO: validate position
		Player player = gameData.Players.Find(player => player.GetId() == id);
		if (player == null) {
			GD.PrintErr($"Player {id} not found");
			return;
		}
		player.SetPosition(position);
		RpcId(0, nameof(UpdatePositionSuccess), id, player.GetPosition());
	}

	[Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.UnreliableOrdered)]
	private void UpdatePositionSuccess(long id, Vector2 position) {}
}
