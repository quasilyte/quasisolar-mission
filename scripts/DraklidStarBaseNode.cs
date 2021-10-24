using Godot;
using System;
using System.Collections.Generic;

public class DraklidStarBaseNode : StarBaseNode {
    [Signal]
    public delegate void SpaceUnitCreated(SpaceUnitNode unit);

    private static PackedScene _scene = null;
    public static new DraklidStarBaseNode New(StarBase starBase) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/DraklidStarBaseNode.tscn");
        }
        var o = (DraklidStarBaseNode)_scene.Instance();
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

        if (_gameState.day > 400 && starBase.garrison.Count > 5 && starBase.level >= ItemInfo.MinStarBaseLevel(VesselDesign.Find("Plunderer"))) {
            var numPlunderers = 0;
            foreach (var vessel in starBase.garrison) {
                if (vessel.Get().designName == "Plunderer") {
                    numPlunderers++;
                }
            }
            if (numPlunderers < 2) {
                starBase.productionQueue.Enqueue("Plunderer");
                return;
            }
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
        spaceUnit.owner = Faction.Draklid;
        spaceUnit.pos = starBase.system.Get().pos;

        var groupSize = QRandom.IntRange(1, 2);
        for (int i = 0; i < groupSize; i++) {
            spaceUnit.fleet.Add(starBase.PopVessel());
        }

        if (QRandom.Float() < 0.5) {
            spaceUnit.cargo.minerals = QRandom.IntRange(5, 40) * groupSize;
        }
        if (QRandom.Float() < 0.3) {
            spaceUnit.cargo.organic = QRandom.IntRange(10, 25) * groupSize;
        }
        if (QRandom.Float() < 0.4) {
            spaceUnit.cargo.organic = QRandom.IntRange(5, 35) * groupSize;
        }

        starBase.units.Add(spaceUnit.GetRef());

        var unitNode = DraklidSpaceUnitNode.New(spaceUnit);
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
