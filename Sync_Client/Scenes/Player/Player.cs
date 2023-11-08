using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export]
	public float Speed = 300.0f;

	[Export]
	public float JumpVelocity = -400.0f;

	[Export]
	public float GravityMultiplier = 1.0f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	private Server server;

	private AnimationPlayer animationPlayer;

	private bool flipH = false;


	public override void _Ready()
	{
		server = GetNode<Server>("/root/Server");

		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
			velocity.Y += gravity * GravityMultiplier * (float)delta;

		// Handle Jump.
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
			velocity.Y = JumpVelocity;

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		float direction = Input.GetAxis("ui_left", "ui_right");
		
		if (direction == -1) {
			flipH = true;
		} else if (direction == 1) {
			flipH = false;
		}
		GetNode<AnimatedSprite2D>("AnimatedSprite2D").FlipH = flipH;

		if (direction != 0)
		{
			velocity.X = direction * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
		}

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

		Velocity = velocity;

		MoveAndSlide();
		server.UpdatePosition(GlobalPosition, Velocity, flipH);
	}
}
