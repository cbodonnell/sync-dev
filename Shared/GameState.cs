namespace Shared;

public class GameState {
    public ulong T { get; set; }
    public Dictionary<string, PlayerUpdate> P { get; set; }

    public GameState(ulong tick, Dictionary<string, PlayerUpdate> playerUpdates) {
        T = tick;
        P = playerUpdates;
    }
}
