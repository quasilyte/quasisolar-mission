using Godot;

public class FireAction : IPilotAction {
    public Vector2 target;
    public int weaponIndex;

    public string DebugString() {
        return $"FireAction x={target.x} y={target.y} weapon={weaponIndex}";
    }
}
