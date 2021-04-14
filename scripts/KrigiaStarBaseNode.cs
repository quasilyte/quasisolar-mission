using Godot;
using System;
using System.Collections.Generic;

// TODO: star base level should affect what vessels are being produced
// and how big the production delay is.

public class KrigiaStarBaseNode : StarBaseNode {
    [Signal]
    public delegate void SpaceUnitCreated(SpaceUnitNode unit);

    private static PackedScene _scene = null;
    public static new KrigiaStarBaseNode New(StarBase starBase) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/KrigiaStarBaseNode.tscn");
        }
        var o = (KrigiaStarBaseNode)_scene.Instance();
        o.starBase = starBase;
        return o;
    }

    public override void _Ready() {
        base._Ready();
    }

    public override void _Draw() {
        DrawUtils.DrawDashedCircle(this, InfluenceRadius(), Color.Color8(160, 80, 80));
    }

    public override void ProcessDay() {
        base.ProcessDay();

        var vesselProduced = ProcessProduction();
        if (vesselProduced != null) {
            VesselFactory.Init(vesselProduced, vesselProduced.design);
            starBase.botProductionDelay = QRandom.IntRange(40, 80);
        }

        ProcessResources();

        if (RpgGameState.day > 100) {
            MaybeEnqueueVessel();
        }

        if (RpgGameState.day > 60) {
            MaybeSendPatrol();
        }
    }

    private void ProcessResources() {
        // Krigia has unlimited resources.
        starBase.mineralsStock = 1000;
        starBase.organicStock = 1000;
        starBase.powerStock = 1000;
    }

    private void MaybeSendPatrol() {
        if (starBase.units.Count != 0) {
            return;
        }
        if (starBase.garrison.Count == 0) {
            return;
        }
        if (starBase.botPatrolDelay != 0) {
            return;
        }

        var destination = Vector2.Zero;
        var destinationOptions = RpgGameState.starSystemConnections[starBase.system];
        var destinationSystem = QRandom.Element(destinationOptions);
        if (destinationSystem.pos.DistanceTo(starBase.system.pos) <= InfluenceRadius()) {
            destination = destinationSystem.pos;
        }
        if (destination == Vector2.Zero) {
            return;
        }

        var spaceUnit = new SpaceUnit {
            owner = RpgGameState.krigiaPlayer,
            pos = starBase.system.pos,
            waypoint = destination,
            botOrigin = starBase,
            botProgram = SpaceUnit.Program.KrigiaPatrol,
        };

        var groupSize = QRandom.IntRange(1, 3);
        var keptInGarrison = starBase.garrison.FindAll(v => {
            if (v.design.level > 2) {
                return true;
            }
            if (spaceUnit.fleet.Count == groupSize) {
                return true;
            }
            spaceUnit.fleet.Add(v);
            return false;
        });

        if (spaceUnit.fleet.Count == 0) {
            return;
        }
        starBase.garrison = keptInGarrison;

        RpgGameState.spaceUnits.Add(spaceUnit);
        starBase.units.Add(spaceUnit);

        starBase.botPatrolDelay = QRandom.IntRange(100, 200);

        var unitNode = KrigiaSpaceUnitNode.New(spaceUnit);
        EmitSignal(nameof(SpaceUnitCreated), new object[] { unitNode });
    }

    private void MaybeEnqueueVessel() {
        if (starBase.productionQueue.Count != 0) {
            return;
        }

        var roll = QRandom.Float();
        VesselDesign design = null;
        if (RpgGameState.day < 1000) {
            if (roll < 0.3) {
                design = VesselDesign.Find("Krigia", "Talons");
            } else if (roll < 0.6) {
                design = VesselDesign.Find("Krigia", "Claws");
            } else if (roll < 0.8) {
                design = VesselDesign.Find("Krigia", "Fangs");
            } else if (roll < 0.9) {
                design = VesselDesign.Find("Krigia", "Tusks");
            } else {
                design = VesselDesign.Find("Krigia", "Horns");
            }
        } else {
            if (roll < 0.1) {
                design = VesselDesign.Find("Krigia", "Talons");
            } else if (roll < 0.2) {
                design = VesselDesign.Find("Krigia", "Claws");
            } else if (roll < 0.6) {
                design = VesselDesign.Find("Krigia", "Fangs");
            } else if (roll < 0.8) {
                design = VesselDesign.Find("Krigia", "Tusks");
            } else {
                design = VesselDesign.Find("Krigia", "Horns");
            }
        }

        starBase.productionQueue.Enqueue(design);
    }

    private float InfluenceRadius() {
        return 256;
    }
}
