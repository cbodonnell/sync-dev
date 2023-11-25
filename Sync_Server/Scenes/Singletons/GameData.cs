using Godot;
using Shared;
using System;
using System.Collections.Generic;

public partial class GameData : Node
{
	public readonly Dictionary<string, PlayerUpdate> PlayerUpdateCollection = new Dictionary<string, PlayerUpdate>();

	public readonly Dictionary<string, List<SpawnAttack>> SpawnAttackCollection = new Dictionary<string, List<SpawnAttack>>();

	public readonly List<GameState> GameStates = new List<GameState>();

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

	public void ReceiveSpawnAttack(string id, SpawnAttack attack) {
		if (SpawnAttackCollection.ContainsKey(id)) {
			SpawnAttackCollection[id].Add(attack);
		} else {
			SpawnAttackCollection.Add(id, new List<SpawnAttack> { attack });
		}
	}

	public void AddGameState(GameState gameState) {
		GameStates.Add(gameState);
	}
}
