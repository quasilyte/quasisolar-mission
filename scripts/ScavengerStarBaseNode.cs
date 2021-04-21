using Godot;
using System;
using System.Collections.Generic;

public class ScavengerStarBaseNode : StarBaseNode {
    [Signal]
    public delegate void SpaceUnitCreated(SpaceUnitNode unit);

    private static PackedScene _scene = null;
    public static new ScavengerStarBaseNode New(StarBase starBase) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/ScavengerStarBaseNode.tscn");
        }
        var o = (ScavengerStarBaseNode)_scene.Instance();
        o.starBase = starBase;
        return o;
    }

    public override void _Ready() {
        base._Ready();
    }

    public override void ProcessDay() {
        base.ProcessDay();

        var vesselProduced = ProcessProduction();
        if (vesselProduced != null) {
            VesselFactory.Init(vesselProduced, vesselProduced.Design());
        }

        ProcessResources();

        if (_gameState.day > 30) {
            MaybeEnqueueVessel();
            MaybeCreateSpaceUnit();
        }
    }

    private void ProcessResources() {
        var resourceRoll = QRandom.Float();
        if (resourceRoll < 0.2) {
            starBase.mineralsStock++;
        } else if (resourceRoll < 0.3) {
            starBase.powerStock++;
        }

        if (starBase.mineralsStock < 50 && starBase.organicStock != 0) {
            starBase.mineralsStock += 2;
            starBase.organicStock--;
        } else if (starBase.powerStock < 50 && starBase.organicStock != 0) {
            starBase.powerStock++;
            starBase.organicStock--;
            return;
        }
    }

    private void MaybeEnqueueVessel() {
        if (starBase.productionQueue.Count != 0) {
            return;
        }

        if (starBase.mineralsStock > 100 && starBase.powerStock > 50) {
            starBase.productionQueue.Enqueue("Marauder");
            return;
        }
        var roll = QRandom.Float();
        if (roll < 0.6) {
            starBase.productionQueue.Enqueue("Raider");
        } else {
            starBase.productionQueue.Enqueue("Marauder");
        }
    }

    private void MaybeCreateSpaceUnit() {
        if (starBase.units.Count >= MaxUnits()) {
            return;
        }
        if (starBase.garrison.Count < 10) {
            return;
        }

        var spaceUnit = _gameState.spaceUnits.New();
        spaceUnit.owner = Faction.Scavenger;
        spaceUnit.pos = starBase.system.Get().pos;

        var groupSize = QRandom.IntRange(1, 2);
        for (int i = 0; i < groupSize; i++) {
            spaceUnit.fleet.Add(starBase.PopVessel());
        }

        starBase.units.Add(spaceUnit.GetRef());

        var unitNode = ScavengerSpaceUnitNode.New(spaceUnit);
        EmitSignal(nameof(SpaceUnitCreated), new object[] { unitNode });
    }

    private int MaxUnits() {
        if (_gameState.day < 200) {
            return 1;
        }
        if (_gameState.day < 400) {
            return 2;
        }
        return 3;
    }
}
