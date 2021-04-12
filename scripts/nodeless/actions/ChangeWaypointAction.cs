using Godot;

public class ChangeWaypointAction : IPilotAction {
    public Vector2 pos;

    public string DebugString() {
        return $"ChangeWaypoint x={pos.x} y={pos.y}";
    }
}
