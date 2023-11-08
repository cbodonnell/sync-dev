using Godot;
using System;

public partial class Global : Node
{
    public static CharacterBody2D InstancePlayer(PackedScene packedScene, Node parentNode, Vector2 position, Vector2 velocity, float direction) {
        var instance = packedScene.Instantiate<CharacterBody2D>();
        instance.GlobalPosition = position;
        instance.Velocity = velocity;
        if (direction == -1) {
            instance.GetNode<AnimatedSprite2D>("AnimatedSprite2D").FlipH = true;
        } else if (direction == 1) {
            instance.GetNode<AnimatedSprite2D>("AnimatedSprite2D").FlipH = false;
        }
        parentNode.AddChild(instance);
        return instance;
    }
}
