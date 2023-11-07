using Godot;

public class Player {
    private long id;
    private string name;
    private Vector2 position;

    public Player(long id, string name, Vector2 position) {
        this.id = id;
        this.name = name;
        this.position = position;
    }

    public long GetId() {
        return id;
    }

    public string GetName() {
        return name;
    }

    public Vector2 GetPosition() {
        return position;
    }

    public void SetPosition(Vector2 position) {
        this.position = position;
    }
}