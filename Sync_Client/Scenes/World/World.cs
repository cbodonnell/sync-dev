using Godot;
using Shared;
using System;
using System.Collections.Generic;

public partial class World : Node2D
{
	
	private Server server;
	private ulong lastGameStateUpdate = 0;

	private PackedScene playerScene = (PackedScene)GD.Load("res://Scenes/Player/Player.tscn");
	private PackedScene otherPlayerScene = (PackedScene)GD.Load("res://Scenes/OtherPlayer/OtherPlayer.tscn");

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		server = GetNode<Server>("/root/Server");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
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

	public void RemovePlayer(string id)
	{
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

		foreach (KeyValuePair<string, PlayerUpdate> entry in gameState.P)
		{
			string id = entry.Key;
			PlayerUpdate playerUpdate = entry.Value;
			if (id == server.GetUniqueId()) {
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
					AddChild(player);
				}
			}
		}
	}
}
