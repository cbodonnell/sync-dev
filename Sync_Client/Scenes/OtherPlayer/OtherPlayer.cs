using Godot;
using System;

public partial class OtherPlayer : CharacterBody2D
{

	private AnimationPlayer animationPlayer;


	public override void _Ready()
	{
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
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
