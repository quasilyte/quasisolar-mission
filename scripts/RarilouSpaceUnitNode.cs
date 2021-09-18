using Godot;
using System.Collections.Generic;

public class RarilouSpaceUnitNode : SpaceUnitNode {
    private bool _canBeDetected = false;

    private static PackedScene _scene = null;
    public static new RarilouSpaceUnitNode New(SpaceUnit unit) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/RarilouSpaceUnitNode.tscn");
        }
        var o = (RarilouSpaceUnitNode)_scene.Instance();
        o.unit = unit;
        o.speed = 60;
        o._spriteColor = MapNodeColor.Yellow;
        return o;
    }

    public override void _Ready() {
        base._Ready();
        base.Connect("DestinationReached", this, nameof(OnDestinationReached));

        _canBeDetected = unit.waypoint != Vector2.Zero;
        if (RpgGameState.starSystemByPos.ContainsKey(unit.pos)) {
            _currentSystem = RpgGameState.starSystemByPos[unit.pos];
        }

        GlobalPosition = unit.pos;
        UpdateVisibility();
    }

    private void UpdateVisibility() {
        Visible = _canBeDetected && _gameState.humanUnit.Get().pos.DistanceTo(GlobalPosition) <= RpgGameState.RadarRange();
    }

    public void PickNewWaypoint() {
        var candidateSystems = new List<StarSystem>();
        foreach (var sys in _gameState.starSystems.objects.Values) {
            if (sys.starBase.id != 0) {
                continue;
            }
            if (sys.color == StarColor.Purple) {
                continue;
            }
            if (_currentSystem.pos.DistanceTo(sys.pos) < 550) {
                candidateSystems.Add(sys);
            }
        }

        var nextSystem = QRandom.Element(candidateSystems);

        _currentSystem = null;
        unit.waypoint = nextSystem.pos;
        _canBeDetected = true;
    }

    public override void ProcessTick(float delta) {
        base.ProcessTick(delta);
        if (unit.botProgram == SpaceUnit.Program.RarilouFlee) {
            PickNewWaypoint();
            unit.botProgram = SpaceUnit.Program.GenericBehavior;
            return;
        }
    }

    public override void ProcessDay() {
        base.ProcessDay();

        UpdateVisibility();        

        if (unit.waypoint != Vector2.Zero) {
            return;
        }

        if (unit.botSystemLeaveDelay == 0) {
            PickNewWaypoint();
            return;
        }

        unit.botSystemLeaveDelay = QMath.ClampMin(unit.botSystemLeaveDelay - 1, 0);

        foreach (var u in _gameState.spaceUnits.objects.Values) {
            if (u.id == unit.id) {
                continue;
            }
            if (u.pos != unit.pos) {
                continue;
            }
            unit.botSystemLeaveDelay = 0;
            break;
        }
    }

    private void OnDestinationReached() {
        var starBase = _currentSystem.starBase;
        if (starBase.id == 0 && _currentSystem.color != StarColor.Purple) {
            unit.botSystemLeaveDelay = QRandom.IntRange(60, 70);
            _canBeDetected = false;
            return;
        }
    }
}
