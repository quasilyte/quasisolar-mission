using Godot;

public class SpecialAction : IPilotAction {
    public Vector2 target;

    public string DebugString() {
        return $"Special x={target.x} y={target.y}";
    }
}
