using Godot;
using Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class World : Node2D
{
	private Server server;
	
	private PackedScene playerScene = (PackedScene)GD.Load("res://Scenes/Player/Player.tscn");
	private PackedScene otherPlayerScene = (PackedScene)GD.Load("res://Scenes/OtherPlayer/OtherPlayer.tscn");

	private double lastGameStateUpdate = 0;
	private List<GameState> gameStateBuffer = new List<GameState>();
	
	// Good rule of thumb seems to be server tickrate * 2
	// to ensure we're mostly interpolating between two game states.
	// Client could potentially request more frequent updates
	// but this is a good starting point
	private const double INTERPOLATION_OFFSET = 0.100; // 100ms


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		server = GetNode<Server>("/root/Server");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if (gameStateBuffer.Count <= 1) {
			return;
		}

		double renderTime = server.ServerTime - INTERPOLATION_OFFSET;
		while (gameStateBuffer.Count > 2 && gameStateBuffer[2].T < renderTime) {
			gameStateBuffer.RemoveAt(0);
		}

		if (gameStateBuffer.Count > 2) {
			// GD.Print("Interpolating");
			// we have a future game state, interpolate between the previous and the future game state
			float interpolationFactor = (float)(renderTime - gameStateBuffer[1].T) / (float)(gameStateBuffer[2].T - gameStateBuffer[1].T);
			foreach (KeyValuePair<string, PlayerUpdate> entry in gameStateBuffer[2].P)
			{
				string id = entry.Key;
				if (id == server.GetUniqueId()) {
					// TODO: make any corrections if necessary
					continue;
				}
				if (!gameStateBuffer[1].P.ContainsKey(id)) {
					continue;
				}

				PlayerUpdate previousPlayerUpdate = gameStateBuffer[1].P[id];
				PlayerUpdate newPlayerUpdate = entry.Value;

				OtherPlayer existingPlayer = GetNodeOrNull<OtherPlayer>(id);
				if (existingPlayer != null) {
					// GD.Print($"{server.GetUniqueId()} is updating other player {id}");
					Global.InterpolateOtherPlayer(existingPlayer, previousPlayerUpdate, newPlayerUpdate, interpolationFactor);
				} else {
					GD.Print($"UpdateGameState: Player {id} not found. Instancing...");
					OtherPlayer player = Global.InstanceOtherPlayer(otherPlayerScene, id, newPlayerUpdate);
					AddChild(player);
				}
			}
		} else if (renderTime > gameStateBuffer[1].T) {
			// GD.Print("Extrapolating");
			// we have no future game state, extrapolate from the previous game state
			float extrapolationFactor = (float)(renderTime - gameStateBuffer[0].T) / (float)(gameStateBuffer[1].T - gameStateBuffer[0].T) - 1.0f;
			foreach (KeyValuePair<string, PlayerUpdate> entry in gameStateBuffer[1].P)
			{
				string id = entry.Key;
				if (id == server.GetUniqueId()) {
					// TODO: make any corrections if necessary
					continue;
				}
				if (!gameStateBuffer[0].P.ContainsKey(id)) {
					continue;
				}

				PlayerUpdate previousPreviousPlayerUpdate = gameStateBuffer[0].P[id];
				PlayerUpdate previousPlayerUpdate = entry.Value;

				OtherPlayer existingPlayer = GetNodeOrNull<OtherPlayer>(id);
				if (existingPlayer != null) {
					// GD.Print($"{server.GetUniqueId()} is updating other player {id}");
					Global.ExtrapolateOtherPlayer(existingPlayer, previousPreviousPlayerUpdate, previousPlayerUpdate, extrapolationFactor);
				} else {
					GD.Print($"UpdateGameState: Player {id} not found. Not instancing since we're extrapolating...");
				}
			}
		}

	}

	public void InstancePlayer(string id, PlayerUpdate playerUpdate)
	{
		if (id == server.GetUniqueId()) {
			Player player = Global.InstancePlayer(playerScene, id, playerUpdate);
        	AddChild(player);
		} else {
			OtherPlayer player = Global.InstanceOtherPlayer(otherPlayerScene, id, playerUpdate);
			AddChild(player);
		}
	}

	public async void RemovePlayer(string id)
	{
		// TODO: graceful logout
		await Task.Delay(200);
		CharacterBody2D player = GetNodeOrNull<CharacterBody2D>(id);
		if (player == null) {
			GD.PrintErr($"RemovePlayer: Player {id} not found");
			return;
		}
		player.QueueFree();
	}

	public void UpdateGameState(GameState gameState)
	{
		if (gameState.T < lastGameStateUpdate) {
			GD.PrintErr($"UpdateGameState: Received old game state {gameState.T} < {lastGameStateUpdate}");
			return;
		}
		lastGameStateUpdate = gameState.T;
		gameStateBuffer.Add(gameState);
	}
}
