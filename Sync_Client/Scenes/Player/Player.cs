using Godot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.Json;
using Shared;

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

	private AnimatedSprite2D animatedSprite2D;
	private AnimationPlayer animationPlayer;

	public string Character = "NinjaFrog";
	public bool FlipH = false;

	private Dictionary<string, PackedScene> characterScenes = new Dictionary<string, PackedScene>()
	{
		{ "NinjaFrog", (PackedScene)GD.Load("res://Scenes/Characters/NinjaFrog.tscn") },
		{ "MaskDude", (PackedScene)GD.Load("res://Scenes/Characters/MaskDude.tscn") },
	};


	public override void _Ready()
	{
		server = GetNode<Server>("/root/Server");
		
		animatedSprite2D = characterScenes[Character].Instantiate<AnimatedSprite2D>();
		animatedSprite2D.FlipH = FlipH;
		AddChild(animatedSprite2D);

		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		animationPlayer.RootNode = $"../{animatedSprite2D.Name}";
	}

	public override void _PhysicsProcess(double delta)
	{
		ComputePhysics(delta);
		SendPlayerUpdate();
	}

	private void ComputePhysics(double delta)
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
			FlipH = true;
		} else if (direction == 1) {
			FlipH = false;
		}
		animatedSprite2D.FlipH = FlipH;

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
	}

	private void SendPlayerUpdate()
	{
		PlayerUpdate playerUpdate = new PlayerUpdate()
		{
			T = server.ServerTime,
			P = GlobalPosition,
			V = Velocity,
			F = FlipH,
			C = Character,
		};
		server.SendPlayerUpdate(playerUpdate);
	}
}
