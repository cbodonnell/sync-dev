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
    
    public static void InterpolateOtherPlayer(OtherPlayer player, PlayerUpdate previousPlayerUpdate, PlayerUpdate newPlayerUpdate, float interpolationFactor)
    {
			// player.CreateTween().TweenProperty(player, "position", newPlayerUpdate.P, 0.05f);
            player.GlobalPosition = previousPlayerUpdate.P.Lerp(newPlayerUpdate.P, interpolationFactor);
			player.Velocity = newPlayerUpdate.V;
            player.FlipH = newPlayerUpdate.F;
    }
    
    public static void ExtrapolateOtherPlayer(OtherPlayer player, PlayerUpdate previousPreviousPlayerUpdate, PlayerUpdate previousPlayerUpdate, float extrapolationFactor)
    {
            Vector2 positionDelta = previousPlayerUpdate.P - previousPreviousPlayerUpdate.P;
            player.GlobalPosition = previousPlayerUpdate.P + positionDelta * extrapolationFactor;
			player.Velocity = previousPlayerUpdate.V;
            player.FlipH = previousPlayerUpdate.F;
    }
}
