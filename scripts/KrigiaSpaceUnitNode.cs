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
        o._spriteColor = MapNodeColor.Red;

        switch (unit.botProgram) {
        case SpaceUnit.Program.KrigiaPatrol:
            o.speed = 40;
            break;
        case SpaceUnit.Program.KrigiaTaskForce:
            o.speed = 20;
            break;
        case SpaceUnit.Program.KrigiaReinforcements:
            o.speed = 25;
            break;
        default:
            throw new Exception("unexpected Krigia space unit program: " + unit.botProgram.ToString());
        }

        return o;
    }

    public override void _Ready() {
        base._Ready();
        base.Connect("DestinationReached", this, nameof(OnDestinationReached));

        _canBeDetected = unit.waypoint != Vector2.Zero || unit.botProgram == SpaceUnit.Program.KrigiaTaskForce;
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
        case SpaceUnit.Program.KrigiaTaskForce:
            TaskForceProcessDay();
            break;
        }

        unit.botSystemLeaveDelay = QMath.ClampMin(unit.botSystemLeaveDelay - 1, 0);
    }

    private void TaskForceProcessDay() {
        // Base is destroyed. Can return home.
        if (_currentSystem.starBase == null) {
            unit.waypoint = unit.botOrigin.system.pos;
            _currentSystem = null;
            _canBeDetected = true;
            return;
        }

        EmitSignal(nameof(AttackStarBase));
    }

    private void PatrolProcessDay() {
        if (unit.botSystemLeaveDelay == 0) {
            unit.waypoint = unit.botOrigin.system.pos;
            _currentSystem = null;
            _canBeDetected = true;
            return;
        }

        if (_gameState.humanUnit.pos == unit.pos) {
            return;
        }

        if (_currentSystem != null && _currentSystem.starBase == null) {
            foreach (var p in _currentSystem.resourcePlanets) {
                if (p.hasMine) {
                    p.hasMine = false;
                    EmitSignal(nameof(DroneDestroyed));
                }
            }
        }
    }

    private void OnDestinationReached() {
        switch (unit.botProgram) {
        case SpaceUnit.Program.KrigiaPatrol:
            PatrolDestinationReached();
            break;
        case SpaceUnit.Program.KrigiaTaskForce:
            TaskForceDestinationReached();
            break;
        case SpaceUnit.Program.KrigiaReinforcements:
            ReinforcementsDestinationReached();
            break;
        }
    }

    private void EnterBase(StarBase starBase) {
        var vesselsLeft = unit.fleet.FindAll(v => {
            if (starBase.garrison.Count < StarBase.maxGarrisonSize) {
                starBase.garrison.Add(v);
                return false;
            }
            return true;
        });
        if (vesselsLeft.Count != 0) {
            // TODO: send remains to a patrol mission?
            // Or maybe to another star base?
            GD.Print("WARNING: can't board all the ships");
        }
        starBase.units.Remove(unit);
        _gameState.spaceUnits.Remove(unit);
        EmitSignal(nameof(Removed));
        QueueFree();
    }

    private void ReinforcementsDestinationReached() {
        _currentSystem = RpgGameState.starSystemByPos[unit.waypoint];
        if (_currentSystem.starBase != null && _currentSystem.starBase.owner == _gameState.krigiaPlayer) {
            EnterBase(_currentSystem.starBase);
        } else {
            unit.waypoint = unit.botOrigin.system.pos;
            _currentSystem = null;
            _canBeDetected = true;
        }
    }

    private void TaskForceDestinationReached() {
        _currentSystem = RpgGameState.starSystemByPos[unit.waypoint];

        var starBase = _currentSystem.starBase;
        if (_currentSystem == unit.botOrigin.system) {
            EnterBase(starBase);
            return;
        }
    }

    private void PatrolDestinationReached() {
        _currentSystem = RpgGameState.starSystemByPos[unit.waypoint];

        var starBase = _currentSystem.starBase;
        if (_currentSystem == unit.botOrigin.system) {
            EnterBase(starBase);
            return;
        }

        if (starBase != null && starBase.owner == _gameState.humanPlayer) {
            if (_gameState.humanUnit.pos != _currentSystem.pos) {
                if (starBase.discoveredByKrigia == 0) {
                    starBase.discoveredByKrigia = _gameState.day;
                    EmitSignal(nameof(BaseDetected));
                }
            }
        }

        if (starBase == null) {
            unit.botSystemLeaveDelay = QRandom.IntRange(8, 32);
            _canBeDetected = false;
        }
    }

    private void UpdateVisibility() {
        Visible = _canBeDetected && _gameState.humanUnit.pos.DistanceTo(GlobalPosition) <= RpgGameState.RadarRange();
    }
}
