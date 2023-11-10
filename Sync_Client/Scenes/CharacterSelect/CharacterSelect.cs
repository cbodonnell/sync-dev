using Godot;
using System;

public partial class CharacterSelect : Node2D
{
	private Server server;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		server = GetNode<Server>("/root/Server");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void _on_ninja_frog_pressed()
	{
		server.SelectCharacter("NinjaFrog");
	}

	private void _on_mask_dude_pressed()
	{
		server.SelectCharacter("MaskDude");
	}
}
