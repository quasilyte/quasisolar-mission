using Godot;
using System.Collections.Generic;

public abstract class SpaceUnitNode : Node2D {
    public SpaceUnit unit;

    protected StarSystem _currentSystem;

    protected RpgGameState _gameState;

    protected MapNodeColor _spriteColor;

    public float speed;

    [Signal]
    public delegate void MovePhaaStarBase();

    [Signal]
    public delegate void SearchForStarBase();

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
        if (unit.owner == Faction.Earthling) {
            GetNode<Sprite>("Sprite").Frame = (int)MapNodeColor.Cyan;
            return;
        }
        GetNode<Sprite>("Sprite").Frame = (int)_spriteColor;
    }

    public override void _Ready() {
        _gameState = RpgGameState.instance;
        UpdateColor();

        if (RpgGameState.starSystemByPos.ContainsKey(unit.pos)) {
            _currentSystem = RpgGameState.starSystemByPos[unit.pos];
        }

        GetNode<Area2D>("Area2D").Connect("mouse_entered", this, nameof(OnMouseEnter));
        GetNode<Area2D>("Area2D").Connect("mouse_exited", this, nameof(OnMouseExited));
    }

    public virtual void ProcessTick(float delta) {
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
            if (RpgGameState.starSystemByPos.ContainsKey(unit.waypoint)) {
                _currentSystem = RpgGameState.starSystemByPos[unit.waypoint];
            }
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

    private void OnMouseEnter() {
        if (!Visible) {
            return;
        }
        if (RpgGameState.instance.humanUnit.id == unit.id) {
            return;
        }
        MapItemInfoNode.instance.Pin(this, new Vector2(0, -48), GetKnownInfo());
    }

    private void OnMouseExited() {
        MapItemInfoNode.instance.Unpin(this);
    }

    private List<string> GetKnownInfo() {
        var lines = new List<string>();
        lines.Add(unit.owner.ToString() + " Unit");
        
        lines.Add("--");
        foreach (var x in unit.fleet) {
            var v = x.Get();
            var rank = new string('*', v.rank);
            var level = v.Design().level;
            lines.Add($"{v.designName} [{level}] [{rank}]");
        }

        return lines;
    }
}
