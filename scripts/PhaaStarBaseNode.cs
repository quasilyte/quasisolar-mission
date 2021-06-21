using Godot;
using System;
using System.Collections.Generic;

public class PhaaStarBaseNode : StarBaseNode {
    [Signal]
    public delegate void SpaceUnitCreated(SpaceUnitNode unit);

    private static PackedScene _scene = null;
    public static new PhaaStarBaseNode New(StarBase starBase) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/PhaaStarBaseNode.tscn");
        }
        var o = (PhaaStarBaseNode)_scene.Instance();
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

        MaybeEnqueueVessel();
    }

    private void ProcessResources() {
        foreach (var p in starBase.system.Get().resourcePlanets) {
            var roll = QRandom.Float();
            if (roll < 0.2) {
                starBase.mineralsStock += p.mineralsPerDay;
                starBase.organicStock += p.organicPerDay;
                starBase.powerStock += p.powerPerDay;
            }
        }
    }

    private void MaybeEnqueueVessel() {
        if (starBase.productionQueue.Count != 0) {
            return;
        }

        starBase.productionQueue.Enqueue("Mantis");
    }
}
