using Godot;
using System;

public class KrigiaSpaceUnitNode : SpaceUnitNode {
    private StarSystem _currentSystem;
    private bool _canBeDetected = false;

    private static PackedScene _scene = null;
    public static new KrigiaSpaceUnitNode New(SpaceUnit unit) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/KrigiaSpaceUnitNode.tscn");
        }
        var o = (KrigiaSpaceUnitNode)_scene.Instance();
        o.unit = unit;
        o.speed = 50;
        o._spriteColor = MapNodeColor.Red;
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

    public override void ProcessDay() {
        base.ProcessDay();

        UpdateVisibility();        

        if (unit.waypoint != Vector2.Zero) {
            return;
        }

        switch (unit.botProgram) {
        case SpaceUnit.Program.KrigiaPatrol:
            PatrolProcessDay();
            break;
        }

        unit.botSystemLeaveDelay = QMath.ClampMin(unit.botSystemLeaveDelay - 1, 0);
    }

    private void PatrolProcessDay() {
        if (unit.botSystemLeaveDelay == 0) {
            unit.waypoint = unit.botOrigin.system.pos;
            _currentSystem = null;
            _canBeDetected = true;
            return;
        }

        if (_currentSystem != null && _currentSystem.starBase == null) {
            foreach (var p in _currentSystem.resourcePlanets) {
                // Destroy the drone.
                p.hasMine = false;
            }
        }
    }

    private void OnDestinationReached() {
        switch (unit.botProgram) {
        case SpaceUnit.Program.KrigiaPatrol:
            PatrolDestinationReached();
            break;
        }
    }

    private void PatrolDestinationReached() {
        _currentSystem = RpgGameState.starSystemByPos[unit.waypoint];

        var starBase = _currentSystem.starBase;
        if (_currentSystem == unit.botOrigin.system) {
            var vesselsLeft = unit.fleet.FindAll(v => {
                if (starBase.garrison.Count < StarBase.maxGarrisonSize) {
                    starBase.garrison.Add(v);
                    return false;
                }
                return true;
            });
            if (vesselsLeft.Count != 0) {
                // TODO: send remains to another patrol mission?
                // or to another star base?
                GD.Print("WARNING: can't board all the ships after a patrol mission");
            }
            starBase.units.Remove(unit);
            RpgGameState.spaceUnits.Remove(unit);
            EmitSignal(nameof(Removed));
            QueueFree();
            return;
        }

        
        if (starBase != null && starBase.owner == RpgGameState.humanPlayer) {
            if (RpgGameState.humanUnit.pos != _currentSystem.pos) {
                starBase.discoveredByKrigia = true;
            }
        }

        if (starBase == null) {
            unit.botSystemLeaveDelay = QRandom.IntRange(8, 32);
            _canBeDetected = false;
        }
    }

    private void UpdateVisibility() {
        Visible = _canBeDetected && RpgGameState.humanUnit.pos.DistanceTo(GlobalPosition) <= RpgGameState.RadarRange();
    }
}
