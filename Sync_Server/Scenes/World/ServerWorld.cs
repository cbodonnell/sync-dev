using Godot;
using System;
using Shared;

public partial class ServerWorld : Node2D
{
	private PackedScene playerScene = (PackedScene)GD.Load("res://Scenes/Player/ServerPlayer.tscn");

	public ServerPlayer InstanceServerPlayer(string id, PlayerUpdate playerUpdate)
	{
		ServerPlayer player = playerScene.Instantiate<ServerPlayer>();
        player.Name = id;
        player.GlobalPosition = playerUpdate.P;
        player.Velocity = playerUpdate.V;
        player.Character = playerUpdate.C;
        player.FlipH = playerUpdate.F;
		AddChild(player);
		return player;
	}

	public void RemoveServerPlayer(string id)
	{
		ServerPlayer player = GetNodeOrNull<ServerPlayer>(id);
		if (player == null) {
			GD.PrintErr($"RemovePlayer: Player {id} not found");
			return;
		}
		player.QueueFree();
	}

	public void UpdateServerPlayer(string id, PlayerUpdate playerUpdate)
    {
			ServerPlayer existingPlayer = GetNodeOrNull<ServerPlayer>(id);
			if (existingPlayer == null) {
				GD.PrintErr($"UpdatePlayer: Player {id} not found");
				return;
			}
			existingPlayer.GlobalPosition = playerUpdate.P;
			existingPlayer.Velocity = playerUpdate.V;
			existingPlayer.FlipH = playerUpdate.F;
    }
}
