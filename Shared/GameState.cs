namespace Shared;

public class GameState {
    public double T { get; set; }
    public Dictionary<string, PlayerUpdate> P { get; set; }

    public GameState(double timestamp, Dictionary<string, PlayerUpdate> playerUpdates) {
        T = timestamp;
        P = playerUpdates;
    }
}
