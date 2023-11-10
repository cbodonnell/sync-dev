using Godot;

public class Player {
    private long id;
    private string name;
    private string character;
    private Vector2 position;
    private Vector2 velocity;
    private bool flipH;


    public Player(long id, string name, string character, Vector2 position, Vector2 velocity, bool flipH) {
        this.id = id;
        this.name = name;
        this.character = character;
        this.position = position;
        this.velocity = velocity;
        this.flipH = flipH;
    }

    public long GetId() {
        return id;
    }

    public string GetName() {
        return name;
    }

    public string GetCharacter() {
        return character;
    }

    public Vector2 GetPosition() {
        return position;
    }

    public void SetPosition(Vector2 position) {
        this.position = position;
    }

    public Vector2 GetVelocity() {
        return velocity;
    }

    public void SetVelocity(Vector2 velocity) {
        this.velocity = velocity;
    }

    public bool GetFlipH() {
        return flipH;
    }

    public void SetFlipH(bool flipH) {
        this.flipH = flipH;
    }

}