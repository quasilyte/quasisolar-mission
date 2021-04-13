using Godot;
using System;

public class ScavengerSpaceUnitNode : SpaceUnitNode {
    // FIXME: bots lose their field values when scene is switched.

    private StarSystem _currentSystem;
    private bool _canBeDetected = false;

    private static PackedScene _scene = null;
    public static new ScavengerSpaceUnitNode New(SpaceUnit unit) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/ScavengerSpaceUnitNode.tscn");
        }
        var o = (ScavengerSpaceUnitNode)_scene.Instance();
        o.unit = unit;
        o.speed = 40;
        o._spriteColor = MapNodeColor.Purple;
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

    // public override void _Process(float delta) {
    //     base._Process(delta);
    // }

    private void UpdateVisibility() {
        Visible = _canBeDetected && RpgGameState.humanUnit.pos.DistanceTo(GlobalPosition) <= RpgGameState.RadarRange();
    }

    public override void ProcessDay() {
        base.ProcessDay();

        UpdateVisibility();        

        if (unit.waypoint != Vector2.Zero) {
            return;
        }

        if (unit.botSystemLeaveDelay == 0) {
            var destinationOptions = RpgGameState.starSystemConnections[_currentSystem];
            var nextSystem = QRandom.Element(destinationOptions);
            _currentSystem = null;
            unit.waypoint = nextSystem.pos;
            _canBeDetected = true;
        }

        if (_currentSystem != null) {
            if (_currentSystem.starBase != null) {
                ProcessStarBaseDay();
            } else {
                MaybeCollectResources();
            }
        }

        unit.botSystemLeaveDelay = QMath.ClampMin(unit.botSystemLeaveDelay - 1, 0);
    }

    private void OnDestinationReached() {
        _currentSystem = RpgGameState.starSystemByPos[unit.waypoint];

        var starBase = _currentSystem.starBase;
        if (starBase == null || starBase.owner == RpgGameState.scavengerPlayer) {
            unit.botSystemLeaveDelay = QRandom.IntRange(8, 32);
            _canBeDetected = false;
            return;
        }
    }

    private void MaybeCollectResources() {
        if (RpgGameState.humanUnit.pos == unit.pos) {
            return;
        }

        var cargoFree = unit.CargoFree();
        if (cargoFree == 0) {
            return;
        }

        Func<int, int> collectAmount = (int resources) => {
            var roll = QRandom.IntRange(2, 10);
            var collected = QMath.ClampMax(QMath.ClampMin(roll, cargoFree), resources);
            cargoFree -= collected;
            return collected;
        };
        foreach (var p in _currentSystem.resourcePlanets) {
            // Destroy the drone.
            p.hasMine = false;

            if (cargoFree == 0) {
                break;
            }

            if (p.powerCollected > 0) {
                var collected = collectAmount(p.powerCollected);
                unit.cargo.power += collected;
                p.powerCollected -= collected;
            }
            if (p.organicCollected > 0) {
                var collected = collectAmount(p.organicCollected);
                unit.cargo.organic += collected;
                p.organicCollected -= collected;
            }
            if (p.mineralsCollected > 0) {
                var collected = collectAmount(p.mineralsCollected);
                unit.cargo.minerals += collected;
                p.mineralsCollected -= collected;
            }
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
        if (starBase.owner == RpgGameState.scavengerPlayer) {
            if (unit.CargoSize() != 0) {
                OffloadResourcesTo(starBase);
                GD.Print("scavenger offloads resources");
            }
        }
    }
}