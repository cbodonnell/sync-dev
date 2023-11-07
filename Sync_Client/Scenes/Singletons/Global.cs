using Godot;
using System;

public partial class Global : Node
{
    public static Node2D InstanceNode2D(PackedScene packedScene, Node parentNode, Vector2 position) {
        var instance = packedScene.Instantiate<Node2D>();
        instance.GlobalPosition = position;
        parentNode.AddChild(instance);
        return instance;
    }
}
