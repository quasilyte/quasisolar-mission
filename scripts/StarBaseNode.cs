using Godot;
using System.Collections.Generic;

public class StarBaseNode : Node2D {
    [Signal]
    public delegate void LevelUpgraded();

    public StarBase starBase;

    protected RpgGameState _gameState;

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
        _gameState = RpgGameState.instance;
    }

    public void SetColor(MapNodeColor color) {
        GetNode<Sprite>("Sprite").Frame = (int)color;
    }

    public virtual void ProcessDay() {
        foreach (var vref in starBase.garrison) {
            var v = vref.Get();
            v.hp = QMath.ClampMax(v.hp + 5, v.MaxHp());
            v.energy = v.MaxBackupEnergy();
        }

        starBase.botPatrolDelay = QMath.ClampMin(starBase.botPatrolDelay - 1, 0);
        starBase.botReinforcementsDelay = QMath.ClampMin(starBase.botReinforcementsDelay - 1, 0);

        // 35% consume mineral
        // 10% consume organic
        // 20% consume power
        var resourcesRoll = QRandom.Float();
        if (resourcesRoll < 0.35f) {
            starBase.mineralsStock = QMath.ClampMin(starBase.mineralsStock - 1, 0);
        } else if (resourcesRoll < 0.45f) {
            starBase.organicStock = QMath.ClampMin(starBase.organicStock - 1, 0);
        } else if (resourcesRoll < 0.65f) {
            starBase.powerStock = QMath.ClampMin(starBase.powerStock - 1, 0);
        }

        GatherResources();
    }

    protected virtual void GatherResources() {
        // 25% produce mineral
        // 10% produce power
        var resourcesRoll = QRandom.Float();
        if (resourcesRoll < 0.25f) {
            starBase.mineralsStock++;
        } else if (resourcesRoll < 0.35f) {
            starBase.powerStock++;
        }
    }

    protected Vessel ProcessProduction(double productionRate = 1) {
        if (starBase.productionQueue.Count == 0) {
            return null;
        }
        if (starBase.mineralsStock == 0 || starBase.powerStock == 0) {
            return null;
        }

        var productionDelta = productionRate;
        
        Vessel vessel = null;
        var vesselDesignName = starBase.productionQueue.Peek();
        var vesselDesign = VesselDesign.Find(vesselDesignName);
        if ((starBase.productionProgress + productionDelta) >= vesselDesign.productionTime) {
            if (starBase.garrison.Count >= StarBase.maxGarrisonSize) {
                // The vessel is ready, but can't be produced
                // until something is moved outside of a garrison.
                return null;
            }
            starBase.productionProgress = 0;
            starBase.productionQueue.Dequeue();
            vessel = _gameState.NewVessel(starBase.owner, vesselDesign);
            vessel.rank = starBase.VesselRank(QRandom.Float());
            starBase.garrison.Add(vessel.GetRef());
        } else {
            starBase.productionProgress += productionDelta;
        }

        starBase.mineralsStock--;
        if (QRandom.Float() < 0.5) {
            starBase.powerStock--;
        }

        return vessel;
    }

    public virtual float InfluenceRadius() { return 0; }
}
