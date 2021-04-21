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
            VesselFactory.Init(vesselProduced, vesselProduced.Design());
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
        var destinationOptions = RpgGameState.starSystemConnections[starBase.system.Get()];
        var destinationSystem = QRandom.Element(destinationOptions);
        if (destinationSystem.pos.DistanceTo(starBase.system.Get().pos) <= InfluenceRadius()) {
            destination = destinationSystem.pos;
        }
        if (destination == Vector2.Zero) {
            return;
        }

        var spaceUnit = _gameState.spaceUnits.New();
        spaceUnit.owner = Faction.Krigia;
        spaceUnit.pos = starBase.system.Get().pos;
        spaceUnit.waypoint = destination;
        spaceUnit.botProgram = SpaceUnit.Program.KrigiaPatrol;
        spaceUnit.botOrigin = starBase.GetRef();

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
            if (v.Get().Design().level > 2) {
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

        starBase.units.Add(spaceUnit.GetRef());

        starBase.botPatrolDelay = QRandom.IntRange(100, 200);

        var unitNode = KrigiaSpaceUnitNode.New(spaceUnit);
        EmitSignal(nameof(SpaceUnitCreated), new object[] { unitNode });
    }

    private string ChooseVesselToProduce() {
        string design = "";
        if (_gameState.day < 1000) {
            while (true) {
                var roll = QRandom.Float();
                if (roll < 0.3) {
                    design ="Talons";
                } else if (roll < 0.6) {
                    design ="Claws";
                } else if (roll < 0.8) {
                    design ="Fangs";
                } else if (roll < 0.9) {
                    design ="Tusks";
                } else {
                    design ="Horns";
                }
                if (starBase.level >= ItemInfo.MinStarBaseLevel(VesselDesign.Find(design))) {
                    return design;
                }
            }
        } else {
            while (true) {
                var roll = QRandom.Float();
                if (roll < 0.1) {
                    design ="Talons";
                } else if (roll < 0.2) {
                    design ="Claws";
                } else if (roll < 0.6) {
                    design ="Fangs";
                } else if (roll < 0.8) {
                    design ="Tusks";
                } else {
                    design ="Horns";
                }
                if (starBase.level >= ItemInfo.MinStarBaseLevel(VesselDesign.Find(design))) {
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
