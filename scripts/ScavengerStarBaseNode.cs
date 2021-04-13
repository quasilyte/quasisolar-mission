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
            VesselFactory.Init(vesselProduced, vesselProduced.design);
        }

        ProcessResources();

        if (RpgGameState.day > 30) {
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
            starBase.productionQueue.Enqueue(VesselDesign.Find("Scavenger", "Marauder"));
            return;
        }
        var roll = QRandom.Float();
        if (roll < 0.6) {
            starBase.productionQueue.Enqueue(VesselDesign.Find("Scavenger", "Raider"));
        } else {
            starBase.productionQueue.Enqueue(VesselDesign.Find("Scavenger", "Marauder"));
        }
    }

    private void MaybeCreateSpaceUnit() {
        if (starBase.units.Count >= MaxUnits()) {
            return;
        }
        if (starBase.garrison.Count < 10) {
            return;
        }

        var spaceUnit = new SpaceUnit {
            owner = RpgGameState.scavengerPlayer,
            pos = starBase.system.pos,
        };

        var groupSize = QRandom.IntRange(1, 2);
        for (int i = 0; i < groupSize; i++) {
            spaceUnit.fleet.Add(starBase.PopVessel());
        }

        RpgGameState.spaceUnits.Add(spaceUnit);
        starBase.units.Add(spaceUnit);

        var unitNode = ScavengerSpaceUnitNode.New(spaceUnit);
        EmitSignal(nameof(SpaceUnitCreated), new object[] { unitNode });
    }

    private int MaxUnits() {
        return 3;
    }
}
