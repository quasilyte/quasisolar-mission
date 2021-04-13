using Godot;

public abstract class SpaceUnitNode : Node2D {
    public SpaceUnit unit;

    protected int _spriteFrame = 0;

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

    public void UpdateColor() {
        if (unit.owner == RpgGameState.humanPlayer) {
            GetNode<Sprite>("Sprite").Frame = 0;
            return;
        }
        if (RpgGameState.technologiesResearched.Contains("Fleet Identifier")) {
            GetNode<Sprite>("Sprite").Frame = _spriteFrame;
        } else {
            GetNode<Sprite>("Sprite").Frame = 3; // yellow
        }
    }

    public override void _Ready() {
        UpdateColor();
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
