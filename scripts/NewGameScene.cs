using Godot;
using System;
using System.Collections.Generic;

public class NewGameScene : Node2D {
    class Option {
        public string text;
        public string value;
        public int score;
        public bool selected;
    }

    private Dictionary<string, Option[]> options = new Dictionary<string, Option[]> {
        {
            "StartingSkill",
            new Option[]{
                new Option{text = "Salvaging", value = "Salvaging", score = 0},
                new Option{text = "Fighter", value = "Fighter", score = 0},
                new Option{text = "Siege Mastery I", value = "Siege Mastery I", score = 0},
                new Option{text = "Luck", value = "Luck", score = 0, selected = true},
                new Option{text = "No bonus skill", value = "", score = 10},
            }
        },

        {
            "StartingFleet",
            new Option[]{
                new Option{text = "4 vessels", value = "3", score = -10},
                new Option{text = "3 vessels", value = "2", score = -3},
                new Option{text = "2 vessels", value = "1", score = 0, selected = true},
                new Option{text = "Only flagship", value = "0", score = 10},
            }
        },

        {
            "FlagshipDesign",
            new Option[]{
                new Option{text = "Fighter (level 3)", value = "Fighter", score = -15},
                new Option{text = "Freighter (level 3)", value = "Freighter", score = -10},
                new Option{text = "Explorer (level 2)", value = "Explorer", score = 0, selected = true},
                new Option{text = "Scout (level 1)", value = "Scout", score = 5},
            }
        },

        {
            "StartingCredits",
            new Option[]{
                new Option{text = "12500", score = -25},
                new Option{text = "4000", score = -10},
                new Option{text = "1000", score = 0, selected = true},
                new Option{text = "0", score = 5},
            }
        },

        {
            "Artifacts",
            new Option[]{
                new Option{text = "Excessive", value = "20", score = -40},
                new Option{text = "Normal", value = "15", score = 0, selected = true},
                new Option{text = "Rare", value = "12", score = 10},
                new Option{text = "Barely Enough", value = "10", score = 30},
            }
        },

        {
            "PlanetResources",
            new Option[]{
                new Option{text = "Rich", value = "2", score = -45},
                new Option{text = "Normal", value = "1", score = 0, selected = true},
                new Option{text = "Poor", value = "0", score = 15},
            }
        },

        {
            "KrigiaPresence",
            new Option[]{
                new Option{text = "Minimal", value = "-3", score = -20},
                new Option{text = "Normal", value = "0", score = 0, selected = true},
                new Option{text = "High", value = "3", score = 15},
            }
        },

        {
            "ScavengersPresence",
            new Option[]{
                new Option{text = "Minimal", score = -15},
                new Option{text = "Normal", score = 0, selected = true},
                new Option{text = "High", score = 10},
            }
        },

        {
            "MissionDeadline",
            new Option[]{
                new Option{text = "8000 days", score = -20},
                new Option{text = "4000 days", score = 0, selected = true},
                new Option{text = "2000 days", score = 10},
                new Option{text = "1000 days", score = 30},
            }
        },

        {
            "RandomEvents",
            new Option[]{
                new Option{text = "Very Rare", score = 0},
                new Option{text = "Occasional", score = 0, selected = true},
                new Option{text = "Usual", score = 0},
            }
        },

        {
            "Asteroids",
            new Option[]{
                new Option{text = "None", score = 0},
                new Option{text = "Few", score = 0, selected = true},
                new Option{text = "Average", score = 0},
                new Option{text = "Many", score = 0},
            }
        },
    };

    private int _scoreMultiplier;
    private Label _scoreMultiplierLabel;

    public override void _Ready() {
        _scoreMultiplierLabel = GetNode<Label>("ScoreMultiplier");

        foreach (var kv in options) {
            var id = kv.Key;
            var button = GetNode<OptionButton>(id);
            var selected = 1;
            for (int i = 0; i < kv.Value.Length; i++) {
                if (kv.Value[i].selected) {
                    selected = i;
                }
                if (String.IsNullOrEmpty(kv.Value[i].value)) {
                    kv.Value[i].value = kv.Value[i].text;
                }
                button.AddItem(kv.Value[i].text, i);
            }
            var args = new Godot.Collections.Array { id };
            button.Connect("item_selected", this, nameof(OnItemSelected), args);
            button.Select(selected);
        }

        GetNode<Button>("Start").Connect("pressed", this, nameof(OnStartButtonPressed));

        GetNode<LineEdit>("GameSeed").Text = RandomGameSeed();

        UpdateScoreMiltiplier();
    }

    private int CalculateScoreMultiplier() {
        var score = 200;
        foreach (var kv in options) {
            var button = GetNode<OptionButton>(kv.Key);
            score += options[kv.Key][button.Selected].score;
        }
        return score;
    }

    private void UpdateScoreMiltiplier() {
        _scoreMultiplierLabel.Text = CalculateScoreMultiplier() + "%";
    }

    private void OnItemSelected(int itemId, string nodeName) {
        UpdateScoreMiltiplier();
    }

    private string OptionValue(string name) {
        return options[name][GetNode<OptionButton>(name).Selected].value;
    }

    private int OptionIntValue(string name) {
        return (int)Int64.Parse(OptionValue(name));
    }

    public void OnStartButtonPressed() {
        var seed = GameSeed();
        GD.Print($"game seed = {seed}");

        RpgGameState.Reset(seed);
        QRandom.SetRandomNumberGenerator(RpgGameState.rng);

        InitBaseValues();
        GenerateWorld();
        AddScavengers();

        RpgGameState.transition = RpgGameState.MapTransition.NewGame;
        GetTree().ChangeScene("res://scenes/MapView.tscn");
    }

    private ulong GameSeed() {
        var s = GetNode<LineEdit>("GameSeed").Text;
        if (string.IsNullOrEmpty(s)) {
            s = RandomGameSeed();
        }
        return (ulong)s.GetHashCode();
    }

    private string RandomGameSeed() {
        var alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var stringChars = new char[10];
        var random = new Random();

        for (int i = 0; i < stringChars.Length; i++) {
            stringChars[i] = alphabet[random.Next(alphabet.Length)];
        }

        return new string(stringChars);
    }

    private StarSystem.Color RandomStarSystemColor() {
        var colorValues = Enum.GetValues(typeof(StarSystem.Color));
        var colorRoll = QRandom.IntRange(0, colorValues.Length - 1);
        return (StarSystem.Color)colorValues.GetValue(colorRoll);
    }

    private void AddScavengers() {
        // foreach (var sys in RpgGameState.starSystems) {
        //     if (sys.starBase == null || sys.starBase.owner != RpgGameState.scavengerPlayer) {
        //         continue;
        //     }

        //     var v = new Vessel {
        //         isBot = true,
        //         player = RpgGameState.scavengerPlayer,
        //         name = "Foobar",
        //     };
        //     VesselFactory.Init(v, "Scavenger Raider");

        //     var spaceUnit = new SpaceUnit{
        //         owner = RpgGameState.scavengerPlayer,
        //         kind = SpaceUnit.Kind.Scavenger,
        //         pos = RpgGameState.StartingSystem().pos + new Vector2(100, 100),
        //         waypoint = RpgGameState.StartingSystem().pos,
        //         fleet = {v},
        //     };
        //     RpgGameState.spaceUnits.Add(spaceUnit);

        //     sys.starBase.units.Add(spaceUnit);
        // }
    }

    private void InitBaseValues() {
        RpgGameState.limits = new RpgGameState.GameLimits {
            maxFuel = 500,
            droneCapacity = 500,
        };

        RpgGameState.dronePrice = 1200;
        RpgGameState.credits = OptionIntValue("StartingCredits");
        RpgGameState.fuelPrice = 3;
        RpgGameState.repairPrice = 7;
        RpgGameState.fuel = RpgGameState.MaxFuel() - 100;

        RpgGameState.travelSpeed = 60;
        RpgGameState.randomEventCooldown = 20;

        RpgGameState.humanPlayer = new Player {
            PlayerName = GetNode<LineEdit>("PlayerName").Text,
            Alliance = 1,
        };

        RpgGameState.scavengerPlayer = new Player{
            PlayerName = "Scavenger",
            Alliance = 6,
        };;

        RpgGameState.krigiaPlayer = new Player {
            PlayerName = "Krigia",
            Alliance = 2,
        };

        RpgGameState.zythPlayer = new Player {
            PlayerName = "Zyth",
            Alliance = 3,
        };

        RpgGameState.wertuPlayer = new Player {
            PlayerName = "Wertu",
            Alliance = 4,
        };
    }

    private static Vector2 RandomizedLocation(Vector2 loc, float size) {
        var halfSize = size / 2;
        float x = loc.x + QRandom.FloatRange(-halfSize, halfSize);
        float y = loc.y + QRandom.FloatRange(-halfSize, halfSize);
        return new Vector2(x, y);
    }

    private Vector2 RandomStarSystemPosition(Rect2 rect, Sector sector) {
        var attempts = 0;
        var result = Vector2.Zero;
        while (true) {
            attempts++;
            var dist = QRandom.FloatRange(175, 500);
            var toBeConnected = QRandom.Element(sector.systems);
            var candidate = RandomizedLocation(toBeConnected.pos, dist);
            if (!rect.HasPoint(candidate)) {
                continue;
            }
            var retry = false;
            foreach (var sys in sector.systems) {
                if (sys.pos.DistanceTo(candidate) < 170) {
                    retry = true;
                    break;
                }
            }
            if (retry) {
                continue;
            }
            result = candidate;
            break;
        }
        if (attempts > 10) {
            GD.Print($"used {attempts} attempts to find a star system spot");
        }
        return result;
    }

    private ResourcePlanet NewResourcePlanet(float budget) {
        int minerals = 0;
        int organic = 0;
        int power = 0;

        const float mineralCost = 0.08f;
        const float organicCost = 0.15f;
        const float powerCost = 0.21f;

        while (budget >= mineralCost) {
            var resourceType = QRandom.IntRange(0, 2);
            if (resourceType == 2 && budget >= powerCost) {
                budget -= powerCost;
                power++;
            } else if (resourceType == 1 && budget >= organicCost) {
                budget -= organicCost;
                organic++;
            } else {
                budget -= mineralCost;
                minerals++;
            }
        }

        return new ResourcePlanet(minerals, organic, power);
    }

    class VesselTemplate {
        public VesselDesign design;
        public float roll;
    }

    class Sector {
        public List<StarSystem> systems = new List<StarSystem>();

        public int NumArtifacts() {
            var n = 0;
            foreach (var sys in systems) {
                if (sys.artifact != null) {
                    n++;
                }
            }
            return n;
        }
    }

    const int numMapCols = 4;
    const int numMapRows = 2;

    private VesselTemplate[] _scavengerTemplates = new VesselTemplate[]{
        new VesselTemplate{design = VesselDesign.Find("Scavenger", "Raider"), roll = 0},
        new VesselTemplate{design = VesselDesign.Find("Scavenger", "Marauder"), roll = 0.65f},
    };

    private VesselTemplate[] _krigiaTemplates = new VesselTemplate[]{
        new VesselTemplate{design = VesselDesign.Find("Krigia", "Talons"), roll = 0},
        new VesselTemplate{design = VesselDesign.Find("Krigia", "Claws"), roll = 0.3f},
        new VesselTemplate{design = VesselDesign.Find("Krigia", "Fangs"), roll = 0.55f},
        new VesselTemplate{design = VesselDesign.Find("Krigia", "Tusks"), roll = 0.75f},
        new VesselTemplate{design = VesselDesign.Find("Krigia", "Horns"), roll = 0.90f},
    };

    private VesselTemplate[] _wertuTemplates = new VesselTemplate[]{
        new VesselTemplate{design = VesselDesign.Find("Wertu", "Probe"), roll = 0},
        new VesselTemplate{design = VesselDesign.Find("Wertu", "Guardian"), roll = 0.3f},
        new VesselTemplate{design = VesselDesign.Find("Wertu", "Angel"), roll = 0.70f},
        new VesselTemplate{design = VesselDesign.Find("Wertu", "Dominator"), roll = 0.85f},
    };

    private void InitFleet(StarBase starBase, VesselTemplate[] templates, float budget) {
        var fleet = new List<Vessel> { };
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
                var v = new Vessel {
                    isBot = true,
                    pilotName = $"{starBase.owner.PlayerName}", // FIXME
                    player = starBase.owner,
                };
                VesselFactory.Init(v, _template.design);
                fleet.Add(v);
                budget -= cost;
                break;
            }
        }

        starBase.garrison = fleet;

        GD.Print("fleet = " + fleet.Count);
    }

    private void InitKrigiaFleet(StarBase starBase, float budget) {
        InitFleet(starBase, _krigiaTemplates, budget);
    }

    private void InitWertuFleet(StarBase starBase, float budget) {
        InitFleet(starBase, _wertuTemplates, budget);
    }

    private void InitScavengerFleet(StarBase starBase, float budget) {
        InitFleet(starBase, _scavengerTemplates, budget);
    }

    private void DeployBases(Player player, int numBases, Sector[] sectors, VesselTemplate[] templates) {
        while (numBases > 0) {
            var col = QRandom.IntRange(1, numMapCols - 1);
            var row = QRandom.IntRange(0, 1);
            var i = row * numMapCols + col;
            var sector = sectors[i];
            var j = QRandom.IntRange(0, sector.systems.Count - 1);
            if (sector.systems[j].starBase == null) {
                var fleetRollBonus = (float)col * 20;
                var fleetRoll = QRandom.FloatRange(40, 80) + fleetRollBonus;
                var starBase = new StarBase(sector.systems[j], player);
                InitFleet(starBase, templates, fleetRoll);
                sector.systems[j].starBase = starBase;
                numBases--;
            }
        }
    }

    private StarSystem NewStarSystem(HashSet<string> starSystenNames, Vector2 pos) {
        var sys = new StarSystem {
            name = StarSystemNames.UniqStarSystemName(starSystenNames),
            color = RandomStarSystemColor(),
            pos = pos,
        };

        var planetsRollBonus = (float)OptionIntValue("PlanetResources") * 0.20f;
        var planetsBudget = QRandom.FloatRange(0, 0.6f) + planetsRollBonus;
        if (planetsBudget < 0.1) {
            sys.resourcePlanets = new List<ResourcePlanet>{
                new ResourcePlanet(1, 0, 0),
            };
        } else {
            while (planetsBudget >= 0.1) {
                if (sys.resourcePlanets.Count == 2) {
                    sys.resourcePlanets.Add(NewResourcePlanet(planetsBudget));
                    break;
                }
                var toSpend = QRandom.FloatRange(0.1f, planetsBudget);
                if (toSpend > 0.6) {
                    var change = toSpend - 0.6f;
                    planetsBudget += change;
                    toSpend = 0.6f;
                }
                planetsBudget -= toSpend;
                sys.resourcePlanets.Add(NewResourcePlanet(toSpend));
            }
        }

        return sys;
    }

    private void GenerateWorld() {
        // Player always starts in the left part of the map.
        var startingCol = 0;
        var startingRow = QRandom.IntRange(0, 1);
        var startingSector = startingCol + startingRow * numMapCols;

        var sectors = new Sector[8];

        var starSystenNames = new HashSet<string>();
        for (int row = 0; row < 2; row++) {
            for (int col = 0; col < numMapCols; col++) {
                var i = row * numMapCols + col;
                var sector = new Sector();
                sectors[i] = sector;
                var rect = new Rect2(new Vector2(col * 685 + 550, row * 450 + 150), 650, 400);
                var middle = rect.Position + rect.Size / 2;

                var color = RandomStarSystemColor();
                sector.systems.Add(NewStarSystem(starSystenNames, RandomizedLocation(middle, 120)));

                for (int j = 0; j < QRandom.IntRange(2, 4); j++) {
                    sector.systems.Add(NewStarSystem(starSystenNames, RandomStarSystemPosition(rect, sector)));
                }
            }
        }

        var startingSystem = sectors[startingSector].systems[0];
        startingSystem.name = "Quasisol";
        startingSystem.color = StarSystem.Color.Yellow;
        startingSystem.starBase = new StarBase(startingSystem, RpgGameState.humanPlayer);
        startingSystem.resourcePlanets = new List<ResourcePlanet>{
            new ResourcePlanet(1, 0, 0),
        };

        var numKrigiaBases = QRandom.IntRange(7, 10) + OptionIntValue("KrigiaPresence");
        var numWertuBases = QRandom.IntRange(3, 4);
        var numZythBases = QRandom.IntRange(2, 3);
        GD.Print($"deployed {numKrigiaBases} Krigia bases");
        GD.Print($"deployed {numWertuBases} Wertu bases");
        GD.Print($"deployed {numZythBases} Zyth bases");

        var startingSkill = OptionValue("StartingSkill");
        if (!startingSkill.Empty()) {
            RpgGameState.skillsLearned.Add(startingSkill);
        }

        // First step: deploy using the predetermined rules.
        {
            var sector = sectors[startingSector];
            sector.systems[1].starBase = new StarBase(sector.systems[1], RpgGameState.krigiaPlayer);
            InitKrigiaFleet(sector.systems[1].starBase, 25);
            numKrigiaBases--;
        }
        {
            var secondSector = startingRow == 0 ? numMapCols : 0;
            var sector = sectors[secondSector];
            var roll = QRandom.FloatRange(35, 55);
            sector.systems[0].starBase = new StarBase(sector.systems[0], RpgGameState.krigiaPlayer);
            InitKrigiaFleet(sector.systems[0].starBase, roll);
            sector.systems[1].starBase = new StarBase(sector.systems[1], RpgGameState.scavengerPlayer);
            InitScavengerFleet(sector.systems[1].starBase, roll);
            numKrigiaBases--;
        }

        // Second step: fill everything else.
        DeployBases(RpgGameState.krigiaPlayer, numKrigiaBases, sectors, _krigiaTemplates);
        DeployBases(RpgGameState.wertuPlayer, numWertuBases, sectors, _wertuTemplates);

        foreach (var sector in sectors) {
            foreach (var sys in sector.systems) {
                var id = RpgGameState.starSystems.Count;
                if (sys == startingSystem) {
                    RpgGameState.startingSystemID = id;
                }
                sys.id = id;
                RpgGameState.starSystems.Add(sys);
                RpgGameState.starSystemByPos[sys.pos] = sys;
            }
        }
        GD.Print($"generated {RpgGameState.starSystems.Count} star systems");

        // TODO: do it more efficiently than O(n^2)?
        var graph = RpgGameState.starSystemConnections;
        Func<StarSystem, StarSystem, bool> addToGraph = (StarSystem sys, StarSystem connected) => {
            if (!graph.ContainsKey(sys)) {
                graph.Add(sys, new List<StarSystem>());
            }
            var list = graph[sys];
            list.Add(connected);
            return true;
        };
        for (int i = 0; i < RpgGameState.starSystems.Count; i++) {
            var sys = RpgGameState.starSystems[i];
            for (int j = 0; j < RpgGameState.starSystems.Count; j++) {
                if (i == j) {
                    continue;
                }
                var other = RpgGameState.starSystems[j];
                if (sys.pos.DistanceTo(other.pos) > 600) {
                    continue;
                }
                addToGraph(sys, other);
                addToGraph(other, sys);
            }
        }
        foreach (var sys in RpgGameState.starSystems) {
            if (!graph.ContainsKey(sys)) {
                throw new Exception("found a system that is not included into the graph");
            }
            var connections = graph[sys];
            if (connections.Count == 0) {
                throw new Exception("found a system with 0 connections");
            }
        }

        // var artifactsNeeded = 10;
        // var artifacts
        foreach (var art in ArtifactDesign.list) {
            while (true) {
                var sys = QRandom.Element(RpgGameState.starSystems);
                if (sys.artifact != null) {
                    continue;
                }
                if (sys == startingSystem) {
                    continue;
                }
                sys.artifact = art;
                sys.artifactRecoveryDelay = QRandom.IntRange(10, 40);
                GD.Print("placed " + art.name + " in " + sys.name + " with " + sys.artifactRecoveryDelay + " recovery delay");
                break;
            }   
        }

        RpgGameState.humanBases.Add(startingSystem.starBase);

        var vesselDesign = VesselDesign.Find("Earthling", OptionValue("FlagshipDesign"));
        var fleet = new List<Vessel>();
        var humanVessel = new Vessel {
            isGamepad = true,
            player = RpgGameState.humanPlayer,
            pilotName = PilotNames.UniqHumanName(RpgGameState.usedNames),
            design = vesselDesign,
            energySource = EnergySource.Find("Power Generator"),
            artifacts = new List<ArtifactDesign>{
                EmptyArtifact.Design,
                EmptyArtifact.Design,
                EmptyArtifact.Design,
                EmptyArtifact.Design,
                EmptyArtifact.Design,
            },
            weapons = new List<WeaponDesign>{
                IonCannonWeapon.Design,
                EmptyWeapon.Design,
            },
            specialWeapon = EmptyWeapon.Design,
        };
        VesselFactory.InitStats(humanVessel);
        fleet.Add(humanVessel);

        for (int i = 0; i < OptionIntValue("StartingFleet"); i++) {
            var v = new Vessel {
                isBot = true,
                player = RpgGameState.humanPlayer,
                pilotName = PilotNames.UniqHumanName(RpgGameState.usedNames),
            };
            VesselFactory.Init(v, "Earthling Scout");
            fleet.Add(v);
        }

        RpgGameState.humanUnit.owner = RpgGameState.humanPlayer;
        RpgGameState.humanUnit.fleet = fleet;
        RpgGameState.humanUnit.pos = startingSystem.pos;
    }
}