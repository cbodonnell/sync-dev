using Godot;
using System;

public partial class Global : Node
{
    public static T InstancePlayer<T>(PackedScene packedScene, long id, string character, Vector2 position, Vector2 velocity, float direction) where T : CharacterBody2D
    {
        var instance = packedScene.Instantiate<T>();
        instance.Name = id.ToString();
        instance.GlobalPosition = position;
        instance.Velocity = velocity;

        if (direction == -1) {
            instance.GetNode<AnimatedSprite2D>(character).FlipH = true;
        } else if (direction == 1) {
            instance.GetNode<AnimatedSprite2D>(character).FlipH = false;
        }
        return instance;
    }

    public static void UpdatePlayer(OtherPlayer player, Vector2 position, Vector2 velocity, bool flipH)
    {
			player.CreateTween().TweenProperty(player, "position", position, 0.05f);
			player.Velocity = velocity;
			player.AnimatedSprite2D.FlipH = flipH;
    }
}
