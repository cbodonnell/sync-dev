using Godot;
using System;
using System.Collections.Generic;

public partial class OtherPlayer : CharacterBody2D
{

	public AnimatedSprite2D AnimatedSprite2D;
	private AnimationPlayer animationPlayer;

	public string Character = "MaskDude";

	private Dictionary<string, PackedScene> characterScenes = new Dictionary<string, PackedScene>()
	{
		{ "NinjaFrog", (PackedScene)GD.Load("res://Scenes/Characters/NinjaFrog.tscn") },
		{ "MaskDude", (PackedScene)GD.Load("res://Scenes/Characters/MaskDude.tscn") },
	};



	public override void _Ready()
	{
		AnimatedSprite2D = characterScenes[Character].Instantiate<AnimatedSprite2D>();
		AddChild(AnimatedSprite2D);

		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		animationPlayer.RootNode = $"../{AnimatedSprite2D.Name}";
	}

	public override void _PhysicsProcess(double delta)
	{
		// GD.Print($"OtherPlayer: {Velocity}");
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
