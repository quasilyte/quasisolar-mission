using Godot;
using System;

public class PhaaSpaceUnitNode : SpaceUnitNode {
    private bool _canBeDetected = false;

    private static PackedScene _scene = null;
    public static new PhaaSpaceUnitNode New(SpaceUnit unit) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/PhaaSpaceUnitNode.tscn");
        }
        var o = (PhaaSpaceUnitNode)_scene.Instance();
        o.unit = unit;
        o._spriteColor = MapNodeColor.Lime;
        o.speed = 25;
        return o;
    }

    public override void _Ready() {
        base._Ready();
        base.Connect("DestinationReached", this, nameof(OnDestinationReached));

        _canBeDetected = true;

        GlobalPosition = unit.pos;
        UpdateVisibility();
    }

    public override void ProcessDay() {
        base.ProcessDay();

        UpdateVisibility();        

        if (unit.waypoint != Vector2.Zero) {
            return;
        }

        ReturnToTheBase();
    }

    private void ReturnToTheBase() {
        unit.waypoint = unit.botOrigin.Get().system.Get().pos;
        _currentSystem = null;
    }

    private bool SystemIsFree() {
        foreach (var u in _gameState.spaceUnits.objects.Values) {
            if (u.id == unit.id) {
                continue;
            }
            if (u.pos != unit.pos) {
                continue;
            }
            return false;
        }

        return true;
    }

    private void OnDestinationReached() {
        if (_currentSystem.starBase.id != 0) {
            if (_currentSystem == unit.botOrigin.Get().system.Get()) {
                EnterBase(unit.botOrigin.Get());
                return;
            }
            return;
        }

        if (!SystemIsFree()) {
            return;
        }

        EnterBase(RpgGameState.phaaBase);
        EmitSignal(nameof(MovePhaaStarBase));
    }

    private void EnterBase(StarBase starBase) {
        // FIXME: this code is copied from the Krigia unit.

        var vesselsLeft = unit.fleet.FindAll(v => {
            if (starBase.garrison.Count < StarBase.maxGarrisonSize) {
                starBase.garrison.Add(v);
                return false;
            }
            return true;
        });
        if (vesselsLeft.Count != 0) {
            GD.Print("WARNING: can't board all the ships");
        }
        starBase.units.Remove(unit.GetRef());
        EmitSignal(nameof(Removed));
        QueueFree();
    }

    private void UpdateVisibility() {
        Visible = _canBeDetected && _gameState.humanUnit.Get().pos.DistanceTo(GlobalPosition) <= RpgGameState.RadarRange();
    }
}
