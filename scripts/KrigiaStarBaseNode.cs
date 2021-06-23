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

        // Krigia has unlimited resources, but their
        // production rate heavily depends on the star base level.
        var vesselProduced = ProcessProduction(0.2 * starBase.level);
        if (vesselProduced != null) {
            VesselFactory.Init(vesselProduced, vesselProduced.Design());
            starBase.botProductionDelay = QRandom.IntRange(50, 80);
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


        int minGroupSize;
        int maxGroupSize;
        if (_gameState.day > 1400) {
            minGroupSize = 2;
            maxGroupSize = 4;
        } else if (_gameState.day > 700) {
            minGroupSize = 2;
            maxGroupSize = 3;
        } else if (_gameState.day > 350) {
            minGroupSize = 1;
            maxGroupSize = 2;
        } else {
            minGroupSize = 1;
            maxGroupSize = 1;
        }

        var fleet = new List<Vessel.Ref>();
        var groupSize = QRandom.IntRange(minGroupSize, maxGroupSize);
        var keptInGarrison = starBase.garrison.FindAll(v => {
            var maxVesselLevel = _gameState.day > 1000 ? 4 : 2;
            if (v.Get().Design().level > maxVesselLevel) {
                return true;
            }
            if (fleet.Count == groupSize) {
                return true;
            }
            fleet.Add(v);
            return false;
        });

        if (fleet.Count == 0) {
            return;
        }

        var spaceUnit = _gameState.spaceUnits.New();
        spaceUnit.fleet = fleet;
        spaceUnit.owner = Faction.Krigia;
        spaceUnit.pos = starBase.system.Get().pos;
        spaceUnit.waypoint = destination;
        spaceUnit.botProgram = SpaceUnit.Program.KrigiaPatrol;
        spaceUnit.botOrigin = starBase.GetRef();
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
