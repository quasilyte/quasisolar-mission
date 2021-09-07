using Godot;
using System;

public class KrigiaSpaceUnitNode : SpaceUnitNode {
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
        case SpaceUnit.Program.KrigiaFinalAttack:
            o.speed = 30;
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
        case SpaceUnit.Program.KrigiaFinalAttack:
            FinalAttackProcessDay();
            break;
        }

        unit.botSystemLeaveDelay = QMath.ClampMin(unit.botSystemLeaveDelay - 1, 0);
    }

    private StarBase FindClosestHumanBase() {
        StarBase closest = null;
        foreach (var starBase in RpgGameState.humanBases) {
            if (closest == null) {
                closest = starBase;
                continue;
            }
            if (starBase.system.Get().pos.DistanceTo(unit.pos) < closest.system.Get().pos.DistanceTo(unit.pos)) {
                closest = starBase;
            }
        }
        return closest;
    }

    private void FinalAttackProcessDay() {
        // When base is destroyed, find another one.
        if (_currentSystem.starBase.id == 0) {
            var nextTarget = FindClosestHumanBase();
            if (nextTarget != null) {
                unit.waypoint = nextTarget.system.Get().pos;
                _currentSystem = null;
            }
            return;
        }

        EmitSignal(nameof(AttackStarBase));
    }

    private void ReturnToTheBase() {
        unit.waypoint = unit.botOrigin.Get().system.Get().pos;
        _currentSystem = null;
        _canBeDetected = true;
    }

    private void TaskForceProcessDay() {
        // Base is destroyed. Can return home.
        if (_currentSystem.starBase.id == 0) {
            ReturnToTheBase();
            return;
        }

        EmitSignal(nameof(AttackStarBase));
    }

    private void PatrolProcessDay() {
        if (unit.botSystemLeaveDelay == 0) {
            ReturnToTheBase();
            return;
        }

        if (_gameState.humanUnit.Get().pos == unit.pos) {
            return;
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
        case SpaceUnit.Program.KrigiaFinalAttack:
            FinalAttackDestinationReached();
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
        starBase.units.Remove(unit.GetRef());
        EmitSignal(nameof(Removed));
        QueueFree();
    }

    private void ReinforcementsDestinationReached() {
        if (_currentSystem.starBase.id != 0 && _currentSystem.starBase.Get().owner == Faction.Krigia) {
            EnterBase(_currentSystem.starBase.Get());
        } else {
            ReturnToTheBase();
        }
    }

    private void FinalAttackDestinationReached() {
    }

    private void TaskForceDestinationReached() {
        var starBase = _currentSystem.starBase;
        if (_currentSystem == unit.botOrigin.Get().system.Get()) {
            EnterBase(starBase.Get());
            return;
        }
    }

    private void PatrolDestinationReached() {
        if (_currentSystem.starBase.id == 0) {
            unit.botSystemLeaveDelay = QRandom.IntRange(8, 32);
            _canBeDetected = false;
            return;
        }

        var starBase = _currentSystem.starBase.Get();
        if (starBase.id == 0) {
            return;
        }

        if (_currentSystem == unit.botOrigin.Get().system.Get()) {
            EnterBase(starBase);
            return;
        }

        if (starBase.owner == Faction.Earthling) {
            if (_gameState.humanUnit.Get().pos != _currentSystem.pos) {
                if (starBase.discoveredByKrigia == 0) {
                    EmitSignal(nameof(SearchForStarBase));
                }
            }
        }
    }

    private void UpdateVisibility() {
        Visible = _canBeDetected && _gameState.humanUnit.Get().pos.DistanceTo(GlobalPosition) <= RpgGameState.RadarRange();
    }
}
