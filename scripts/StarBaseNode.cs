using Godot;
using System.Collections.Generic;

public class StarBaseNode : Node2D {
    [Signal]
    public delegate void LevelUpgraded();

    public StarBase starBase;

    private static PackedScene _scene = null;
    public static StarBaseNode New(StarBase starBase) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/StarBaseNode.tscn");
        }
        var o = (StarBaseNode)_scene.Instance();
        o.starBase = starBase;
        return o;
    }

    public override void _Ready() {
    }

    public void SetColor(Team t) {
        GetNode<Sprite>("Sprite").Frame = (int)t;
    }

    public virtual void ProcessDay() {
        var pointsGained = 1f;
        if (starBase.mineralsStock >= 200) {
            pointsGained++;
            if (RpgGameState.rng.Randf() < 0.5) {
                starBase.mineralsStock--;
            }
        }
        if (starBase.organicStock >= 100) {
            pointsGained++;
            if (RpgGameState.rng.Randf() < 0.5) {
                starBase.organicStock--;
            }
        }
        if (starBase.powerStock >= 150) {
            pointsGained++;
            if (RpgGameState.rng.Randf() < 0.5) {
                starBase.powerStock--;
            }
        }

        starBase.levelProgression += pointsGained;
        if (starBase.level < StarBase.maxBaseLevel) {
            var upgradeCost = starBase.LevelUpgradeCost();
            if (starBase.levelProgression >= upgradeCost) {
                starBase.levelProgression = 0;
                starBase.level++;
                EmitSignal(nameof(LevelUpgraded));
            }
        }

        // 35% consume mineral
        // 10% consume organic
        // 20% consume power
        // 25% produce mineral
        // 10% produce power
        var resourcesRoll = RpgGameState.rng.Randf();
        if (resourcesRoll < 0.35f) {
            starBase.mineralsStock = QMath.ClampMin(starBase.mineralsStock - 1, 0);
        } else if (resourcesRoll < 0.45f) {
            starBase.organicStock = QMath.ClampMin(starBase.organicStock - 1, 0);
        } else if (resourcesRoll < 0.65f) {
            starBase.powerStock = QMath.ClampMin(starBase.powerStock - 1, 0);
        } else if (resourcesRoll < 0.9f) {
            starBase.mineralsStock++;
        } else {
            starBase.powerStock++;
        }
    }

    protected Vessel ProcessProduction() {
        if (starBase.productionQueue.Count == 0) {
            return null;
        }
        if (starBase.mineralsStock == 0 || starBase.powerStock == 0) {
            return null;
        }
        
        Vessel vessel = null;
        var vesselDesign = starBase.productionQueue.Peek();
        if ((starBase.productionProgress + 1) >= vesselDesign.productionTime) {
            if (starBase.garrison.Count >= StarBase.maxGarrisonSize) {
                // The vessel is ready, but can't be produced
                // until something is moved outside of a garrison.
                return null;
            }
            starBase.productionProgress = 0;
            starBase.productionQueue.Dequeue();
            vessel = NewVessel(vesselDesign);
            starBase.garrison.Add(vessel);
        } else {
            starBase.productionProgress++;
        }

        starBase.mineralsStock--;
        if (RpgGameState.rng.Randf() < 0.5) {
            starBase.powerStock--;
        }

        return vessel;
    }

    private Vessel NewVessel(VesselDesign design) {        
        var v = new Vessel {
            isBot = true,
            player = starBase.owner,
            name = PilotNames.UniqHumanName(RpgGameState.usedNames),
            design = design,
        };
        v.hp = v.design.maxHp;
        return v;
    }
}
