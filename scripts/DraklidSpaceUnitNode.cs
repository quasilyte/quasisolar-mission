using Godot;
using System;

public class DraklidSpaceUnitNode : SpaceUnitNode {
    // FIXME: bots lose their field values when scene is switched.

    private bool _canBeDetected = false;

    private static PackedScene _scene = null;
    public static new DraklidSpaceUnitNode New(SpaceUnit unit) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/DraklidSpaceUnitNode.tscn");
        }
        var o = (DraklidSpaceUnitNode)_scene.Instance();
        o.unit = unit;
        o.speed = 40;
        o._spriteColor = MapNodeColor.Purple;
        return o;
    }

    public override void _Ready() {
        base._Ready();
        base.Connect("DestinationReached", this, nameof(OnDestinationReached));

        _canBeDetected = unit.waypoint != Vector2.Zero;

        GlobalPosition = unit.pos;
        UpdateVisibility();
    }

    // public override void _Process(float delta) {
    //     base._Process(delta);
    // }

    private void UpdateVisibility() {
        Visible = _canBeDetected && _gameState.humanUnit.Get().pos.DistanceTo(GlobalPosition) <= RpgGameState.RadarRange();
    }

    public void PickNewWaypoint() {
        var destinationOptions = RpgGameState.starSystemConnections[_currentSystem];
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

        if (_currentSystem != null) {
            if (_currentSystem.starBase.id != 0) {
                ProcessStarBaseDay();
            }
        }

        unit.botSystemLeaveDelay = QMath.ClampMin(unit.botSystemLeaveDelay - 1, 0);
    }

    private void OnDestinationReached() {
        var starBase = _currentSystem.starBase;
        if (starBase.id == 0 || starBase.Get().owner == Faction.Draklid) {
            unit.botSystemLeaveDelay = QRandom.IntRange(8, 32);
            _canBeDetected = false;
            return;
        }
    }

    private void OffloadResourcesTo(StarBase dst) {
        dst.mineralsStock += unit.cargo.minerals;
        dst.organicStock += unit.cargo.organic;
        dst.powerStock += unit.cargo.power;
        unit.cargo.minerals = 0;
        unit.cargo.organic = 0;
        unit.cargo.power = 0;
    }

    private void ProcessStarBaseDay() {
        var starBase = _currentSystem.starBase;
        if (starBase.Get().owner == Faction.Draklid) {
            if (unit.CargoSize() != 0) {
                OffloadResourcesTo(starBase.Get());
            }
        }
    }
}
