using Godot;
using System;

public class ModTraderSpaceUnitNode : SpaceUnitNode {
    private bool _canBeDetected = false;

    private static PackedScene _scene = null;
    public static new ModTraderSpaceUnitNode New(SpaceUnit unit) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/units/ModTraderSpaceUnitNode.tscn");
        }
        var o = (ModTraderSpaceUnitNode)_scene.Instance();
        o.unit = unit;
        o.speed = 20;
        o._spriteColor = MapNodeColor.Pink;
        return o;
    }

    public override void _Ready() {
        base._Ready();
        base.Connect("DestinationReached", this, nameof(OnDestinationReached));

        _canBeDetected = unit.waypoint != Vector2.Zero;

        GlobalPosition = unit.pos;
        UpdateVisibility();
    }

    private void UpdateVisibility() {
        Visible = _canBeDetected && _gameState.humanUnit.Get().pos.DistanceTo(GlobalPosition) <= RpgGameState.RadarRange();
    }

    public void PickNewWaypoint() {
        var destinationOptions = RpgGameState.GetSystemConnections(_currentSystem, 450);
        var neutralSystems = destinationOptions.FindAll((sys) => sys.starBase.id == 0);
        StarSystem dst = null;
        if (neutralSystems.Count != 0) {
            dst = QRandom.Element(neutralSystems);
        } else {
            dst = QRandom.Element(destinationOptions);
        }
        var nextSystem = dst;
        _currentSystem = null;
        unit.waypoint = nextSystem.pos;
        _canBeDetected = true;
    }

    public override void ProcessDay() {
        base.ProcessDay();

        UpdateVisibility();        

        if (unit.waypoint != Vector2.Zero) {
            _canBeDetected = true;
            return;
        }

        if (unit.botSystemLeaveDelay == 0) {
            PickNewWaypoint();
            return;
        }

        unit.botSystemLeaveDelay = QMath.ClampMin(unit.botSystemLeaveDelay - 1, 0);
    }

    private void OnDestinationReached() {
        var starBase = _currentSystem.starBase;
        if (starBase.id == 0) {
            unit.botSystemLeaveDelay = QRandom.IntRange(40, 50);
            _canBeDetected = false;
            return;
        }
    }
}
