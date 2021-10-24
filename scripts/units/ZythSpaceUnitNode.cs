using Godot;
using System;

public class ZythSpaceUnitNode : SpaceUnitNode {
    private bool _canBeDetected = false;

    private static PackedScene _scene = null;
    public static new ZythSpaceUnitNode New(SpaceUnit unit) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/units/ZythSpaceUnitNode.tscn");
        }
        var o = (ZythSpaceUnitNode)_scene.Instance();
        o.unit = unit;
        o.speed = 35;
        o._spriteColor = MapNodeColor.Green;
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
        var destinationOptions = RpgGameState.GetSystemConnections(_currentSystem, 350);
        var nextSystem = QRandom.Element(destinationOptions);
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
            unit.botSystemLeaveDelay = QRandom.IntRange(60, 90);
            _canBeDetected = false;
            return;
        } 
        if (starBase.id != 0 && starBase.Get().owner == Faction.Zyth) {
            unit.botSystemLeaveDelay = QRandom.IntRange(15, 30);
            _canBeDetected = false;
            return;
        }
    }
}
