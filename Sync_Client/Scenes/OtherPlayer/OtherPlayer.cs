using Godot;
using System;
using System.Collections.Generic;

public partial class OtherPlayer : CharacterBody2D
{

	private AnimatedSprite2D animatedSprite2D;
	private AnimationPlayer animationPlayer;

	public string Character = "MaskDude";
	public bool FlipH = false;

	private Dictionary<string, PackedScene> characterScenes = new Dictionary<string, PackedScene>()
	{
		{ "NinjaFrog", (PackedScene)GD.Load("res://Scenes/Characters/NinjaFrog.tscn") },
		{ "MaskDude", (PackedScene)GD.Load("res://Scenes/Characters/MaskDude.tscn") },
	};



	public override void _Ready()
	{
		animatedSprite2D = characterScenes[Character].Instantiate<AnimatedSprite2D>();
		animatedSprite2D.FlipH = FlipH;
		AddChild(animatedSprite2D);

		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		animationPlayer.RootNode = $"../{animatedSprite2D.Name}";
	}

	public override void _PhysicsProcess(double delta)
	{
		// GD.Print($"OtherPlayer: {Velocity}");
		animatedSprite2D.FlipH = FlipH;

		if (Velocity.Y == 0) {
			if (Velocity.X != 0) {
				animationPlayer.Play("Run");
			} else {
				animationPlayer.Play("Idle");
			}
		}

		if (Velocity.Y < 0) {
			animationPlayer.Play("Jump");
		} else if (Velocity.Y > 0) {
			animationPlayer.Play("Fall");
		}
	}
}
