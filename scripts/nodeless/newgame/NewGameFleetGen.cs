using System.Collections.Generic;
using Godot;

public class NewGameFleetGen {
    class VesselTemplate {
        public VesselDesign design;
        public float roll;
    }

    private static VesselTemplate[] _draklidTemplates = new VesselTemplate[]{
        new VesselTemplate{design = VesselDesign.Find("Raider"), roll = 0},
        new VesselTemplate{design = VesselDesign.Find("Marauder"), roll = 0.6f},
        new VesselTemplate{design = VesselDesign.Find("Plunderer"), roll = 0.85f},
    };

    private static VesselTemplate[] _phaaTemplates = new VesselTemplate[]{
        new VesselTemplate{design = VesselDesign.Find("Mantis"), roll = 0},
    };

    private static VesselTemplate[] _krigiaTemplates = new VesselTemplate[]{
        new VesselTemplate{design = VesselDesign.Find("Talons"), roll = 0},
        new VesselTemplate{design = VesselDesign.Find("Claws"), roll = 0.3f},
        new VesselTemplate{design = VesselDesign.Find("Fangs"), roll = 0.55f},
        new VesselTemplate{design = VesselDesign.Find("Destroyer"), roll = 0.7f},
        new VesselTemplate{design = VesselDesign.Find("Tusks"), roll = 0.75f},
        new VesselTemplate{design = VesselDesign.Find("Horns"), roll = 0.90f},
    };

    private static VesselTemplate[] _wertuTemplates = new VesselTemplate[]{
        new VesselTemplate{design = VesselDesign.Find("Probe"), roll = 0},
        new VesselTemplate{design = VesselDesign.Find("Guardian"), roll = 0.3f},
        new VesselTemplate{design = VesselDesign.Find("Angel"), roll = 0.70f},
        new VesselTemplate{design = VesselDesign.Find("Dominator"), roll = 0.85f},
    };

    private static VesselTemplate[] _zythTemplates = new VesselTemplate[]{
        new VesselTemplate{design = VesselDesign.Find("Hunter"), roll = 0},
        new VesselTemplate{design = VesselDesign.Find("Invader"), roll = 0.65f},
    };

    private static VesselTemplate[] _vespionTemplates = new VesselTemplate[]{
        new VesselTemplate{design = VesselDesign.Find("Larva"), roll = 0},
        new VesselTemplate{design = VesselDesign.Find("Hornet"), roll = 0.5f},
        new VesselTemplate{design = VesselDesign.Find("Queen"), roll = 0.8f},
    };

    private static VesselTemplate[] _lezeonaTemplates = new VesselTemplate[]{
        new VesselTemplate{design = VesselDesign.Find("Pin"), roll = 0},
        new VesselTemplate{design = VesselDesign.Find("Core"), roll = 0.65f},
    };

    public static void InitFleet(RpgGameState.Config config, StarBase starBase, Faction faction, float budget) {
        if (faction == Faction.Draklid) {
            InitFleet(config, starBase, _draklidTemplates, budget);
        } else if (faction == Faction.Phaa) {
            InitFleet(config, starBase, _phaaTemplates, budget);
        } else if (faction == Faction.Wertu) {
            InitFleet(config, starBase, _wertuTemplates, budget);
        } else if (faction == Faction.Krigia) {
            InitFleet(config, starBase, _krigiaTemplates, budget);
        } else if (faction == Faction.Zyth) {
            InitFleet(config, starBase, _zythTemplates, budget);
        } else if (faction == Faction.Vespion) {
            InitFleet(config, starBase, _vespionTemplates, budget);
        } else if (faction == Faction.Lezeona) {
            InitFleet(config, starBase, _lezeonaTemplates, budget);
        } else {
            throw new System.Exception("can't init fleet for " + faction.ToString());
        }
    }

    private static void InitFleet(RpgGameState.Config config, StarBase starBase, VesselTemplate[] templates, float budget) {
        var fleet = new List<Vessel.Ref> { };
        var cheapest = (float)templates[0].design.sellingPrice / 1000;

        while (budget >= cheapest) {
            if (fleet.Count == StarBase.maxGarrisonSize) {
                break; // FIXME: do a better job at spending the budget
            }
            var roll = QRandom.Float();
            for (int i = templates.Length - 1; i >= 0; i--) {
                var _template = templates[i];
                if (roll < _template.roll) {
                    continue;
                }
                var cost = (float)_template.design.sellingPrice / 1000;
                if (budget < cost) {
                    continue;
                }
                var v = config.vessels.New();
                v.isBot = true;
                v.pilotName = starBase.owner.ToString(); // FIXME
                v.faction = starBase.owner;
                v.rank = starBase.VesselRank(QRandom.Float());
                VesselFactory.Init(v, _template.design);
                fleet.Add(v.GetRef());
                budget -= cost;
                break;
            }
        }

        starBase.garrison = fleet;
    }
}
