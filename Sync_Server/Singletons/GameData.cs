using Godot;
using System;
using System.Collections.Generic;

public partial class GameData : Node
{
    public readonly List<Player> Players = new List<Player>();

	public void AddPlayer(Player player) {
		Players.Add(player);
	}

	public void RemovePlayer(Player player) {
		Players.Remove(player);
	}
}
