using Godot;
using Shared;
using System;
using System.Collections.Generic;

public partial class GameData : Node
{
	public Dictionary<string, PlayerUpdate> PlayerUpdateCollection = new Dictionary<string, PlayerUpdate>();

	public void RemovePlayer (string id) {
		PlayerUpdateCollection.Remove(id);
	}

	public void ReceivePlayerUpdate(string id, PlayerUpdate playerState) {
		if (PlayerUpdateCollection.ContainsKey(id)) {
			if (PlayerUpdateCollection[id].T < playerState.T) {
				PlayerUpdateCollection[id] = playerState;
			}
		} else {
			PlayerUpdateCollection.Add(id, playerState);
		}
	}
}
