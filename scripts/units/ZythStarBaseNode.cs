using Godot;
using System;
using System.Collections.Generic;

public class ZythStarBaseNode : StarBaseNode {
    [Signal]
    public delegate void SpaceUnitCreated(SpaceUnitNode unit);

    private static PackedScene _scene = null;
    public static new ZythStarBaseNode New(StarBase starBase) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/units/ZythStarBaseNode.tscn");
        }
        var o = (ZythStarBaseNode)_scene.Instance();
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

        if (_gameState.day > 20) {
            MaybeEnqueueVessel();
            MaybeCreateSpaceUnit();
        }
    }

    private void ProcessResources() {
        if (starBase.garrison.Count < 10 && starBase.mineralsStock < 100) {
            starBase.mineralsStock += 1;
        }
        if (starBase.garrison.Count < 10 && starBase.powerStock < 100) {
            starBase.powerStock += 1;
        }
    }

    private void MaybeEnqueueVessel() {
        if (starBase.productionQueue.Count != 0) {
            return;
        }

        var roll = QRandom.Float();
        if (_gameState.day > 800 && roll < 0.55 && starBase.level >= ItemInfo.MinStarBaseLevel(VesselDesign.Find("Invader"))) {
            starBase.productionQueue.Enqueue("Invader");
        } else {
            starBase.productionQueue.Enqueue("Hunter");
        }
    }

    private void MaybeCreateSpaceUnit() {
        if (starBase.units.Count >= MaxUnits()) {
            return;
        }
        if (starBase.garrison.Count < 6) {
            return;
        }

        var spaceUnit = _gameState.spaceUnits.New();
        spaceUnit.owner = Faction.Zyth;
        spaceUnit.pos = starBase.system.Get().pos;

        var groupSize = 1;
        for (int i = 0; i < groupSize; i++) {
            spaceUnit.fleet.Add(starBase.PopVessel());
        }

        if (QRandom.Float() < 0.7) {
            spaceUnit.cargo.power = QRandom.IntRange(5, 10);
        }

        starBase.units.Add(spaceUnit.GetRef());

        var unitNode = ZythSpaceUnitNode.New(spaceUnit);
        EmitSignal(nameof(SpaceUnitCreated), new object[] { unitNode });
    }

    private int MaxUnits() {
        if (_gameState.day < 900) {
            return 1;
        }
        return 2;
    }
}
