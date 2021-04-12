using Godot;

public class SpaceUnitNode : Node2D {
    public SpaceUnit unit;

    public float speed;

    [Signal]
    public delegate void DestinationReached();

    [Signal]
    public delegate void PositionChanged();

    private static PackedScene _scene = null;
    public static SpaceUnitNode New(SpaceUnit unit) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/SpaceUnitNode.tscn");
        }
        var o = (SpaceUnitNode)_scene.Instance();
        o.unit = unit;
        return o;
    }

    public virtual void ProcessDay() {}

    public override void _Ready() {
        // GetNode<Sprite>("Sprite").Frame = (int)unit.kind;
    }

    public override void _Process(float delta) {
        if (RpgGameState.mapState.movementEnabled) {
            Move(delta);
        }
    }

    private void Move(float delta) {
        if (unit.waypoint == Vector2.Zero) {
            return;
        }

        var traveled = 0f;
        var dist = GlobalPosition.DistanceTo(unit.waypoint);
        bool destinationReached = false;
        if (GlobalPosition.DistanceTo(unit.waypoint) <= delta * speed) {
            GlobalPosition = unit.waypoint;
            traveled = dist;
            destinationReached = true;
        } else {
            GlobalPosition = GlobalPosition.MoveToward(unit.waypoint, delta * speed);
            traveled = delta * speed;
        }
        unit.pos = GlobalPosition;
        EmitSignal(nameof(PositionChanged), new object[]{traveled});

        if (destinationReached) {
            EmitSignal(nameof(DestinationReached));
            unit.waypoint = Vector2.Zero;
        }
    }

    public void SetDestination(Vector2 dst) {
        unit.waypoint = dst;
    }

    public Vector2 GetDestination() {
        return unit.waypoint;
    }
}
