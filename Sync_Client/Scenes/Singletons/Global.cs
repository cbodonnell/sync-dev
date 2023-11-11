using Godot;
using Shared;
using System;

public partial class Global : Node
{
    public static Player InstancePlayer(PackedScene scene, string id, PlayerUpdate playerUpdate) {
        Player player = scene.Instantiate<Player>();
        player.Name = id;
        player.GlobalPosition = playerUpdate.P;
        player.Velocity = playerUpdate.V;
        player.Character = playerUpdate.C;
        player.FlipH = playerUpdate.F;
        return player;
    }

    public static OtherPlayer InstanceOtherPlayer(PackedScene scene, string id, PlayerUpdate playerUpdate) {
        OtherPlayer player = scene.Instantiate<OtherPlayer>();
        player.Name = id;
        player.GlobalPosition = playerUpdate.P;
        player.Velocity = playerUpdate.V;
        player.Character = playerUpdate.C;
        player.FlipH = playerUpdate.F;
        return player;
    }
    
    public static void UpdateOtherPlayer(OtherPlayer player, PlayerUpdate playerUpdate)
    {
			player.CreateTween().TweenProperty(player, "position", playerUpdate.P, 0.05f);
			player.Velocity = playerUpdate.V;
            player.FlipH = playerUpdate.F;
    }
}
