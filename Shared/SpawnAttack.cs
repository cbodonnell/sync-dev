namespace Shared;
using Godot;

public class SpawnAttack {
    // T is the unix timestamp
    public double? T { get; set; }

    // P is the position
    public Vector2 P { get; set; }

    // V is the velocity
    public Vector2 V { get; set; }

    // F is the flipH
    public bool F { get; set; }

    // C is the character
    public string C { get; set; }

    public SpawnAttack() {
        T = 0;
        P = new Vector2();
        V = new Vector2();
        F = false;
        C = "";
    }

    public SpawnAttack(double timestamp, Vector2 position, Vector2 velocity, bool flipH, string character) {
        T = timestamp;
        P = position;
        V = velocity;
        F = flipH;
        C = character;
    }
}
