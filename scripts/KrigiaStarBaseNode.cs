using Godot;
using System;
using System.Collections.Generic;

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

        if (_gameState.day > 250) {
            MaybeEnqueueVessel();
        }

        if (_gameState.day > 200) {
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
        var destinationOptions = _gameState.starSystemConnections[starBase.system];
        var destinationSystem = QRandom.Element(destinationOptions);
        if (destinationSystem.pos.DistanceTo(starBase.system.pos) <= InfluenceRadius()) {
            destination = destinationSystem.pos;
        }
        if (destination == Vector2.Zero) {
            return;
        }

        var spaceUnit = new SpaceUnit {
            owner = _gameState.krigiaPlayer,
            pos = starBase.system.pos,
            waypoint = destination,
            botOrigin = starBase,
            botProgram = SpaceUnit.Program.KrigiaPatrol,
        };

        var minGroupSize = 1;
        var maxGroupSize = 1;
        if (_gameState.day > 600) {
            minGroupSize = 2;
            maxGroupSize = 3;
        } else if (_gameState.day > 350) {
            maxGroupSize = 2;
        }
        var groupSize = QRandom.IntRange(minGroupSize, maxGroupSize);
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

        _gameState.spaceUnits.Add(spaceUnit);
        starBase.units.Add(spaceUnit);

        starBase.botPatrolDelay = QRandom.IntRange(100, 200);

        var unitNode = KrigiaSpaceUnitNode.New(spaceUnit);
        EmitSignal(nameof(SpaceUnitCreated), new object[] { unitNode });
    }

    private VesselDesign ChooseVesselToProduce() {
        VesselDesign design = null;
        if (_gameState.day < 1000) {
            while (true) {
                var roll = QRandom.Float();
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
                if (starBase.level >= ItemInfo.MinStarBaseLevel(design)) {
                    return design;
                }
            }
        } else {
            while (true) {
                var roll = QRandom.Float();
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
                if (starBase.level >= ItemInfo.MinStarBaseLevel(design)) {
                    return design;
                }
            }
        }
    }

    private void MaybeEnqueueVessel() {
        if (starBase.productionQueue.Count != 0) {
            return;
        }

        starBase.productionQueue.Enqueue(ChooseVesselToProduce());
    }

    private float InfluenceRadius() {
        return 256;
    }
}
