using Godot;

public class AddWaypointAction : IPilotAction {
    public Vector2 pos;

    public string DebugString() {
        return $"AddWaypoint x={pos.x} y={pos.y}";
    }
}
