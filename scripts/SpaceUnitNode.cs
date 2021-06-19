using Godot;

public abstract class SpaceUnitNode : Node2D {
    public SpaceUnit unit;

    protected RpgGameState _gameState;

    protected MapNodeColor _spriteColor;

    public float speed;

    [Signal]
    public delegate void SearchForStarBase();

    [Signal]
    public delegate void DroneDestroyed();

    [Signal]
    public delegate void AttackStarBase();

    [Signal]
    public delegate void Removed();

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
        if (unit.owner == Faction.Human) {
            GetNode<Sprite>("Sprite").Frame = (int)MapNodeColor.Cyan;
            return;
        }
        GetNode<Sprite>("Sprite").Frame = (int)_spriteColor;
    }

    public override void _Ready() {
        _gameState = RpgGameState.instance;
        UpdateColor();
    }

    public override void _Process(float delta) {
        if (_gameState.mapState.movementEnabled) {
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
