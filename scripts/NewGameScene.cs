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
                new Option{text = "Siege Mastery", value = "Siege Mastery", score = 0},
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
                new Option{text = "1500", score = 0, selected = true},
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
                new Option{text = "Minimal", value = "minimal", score = -20},
                new Option{text = "Normal", value = "normal", score = 0, selected = true},
                new Option{text = "High", value = "high", score = 25},
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

        var rng = new RandomNumberGenerator();
        rng.Seed = seed;

        var gameConfig = NewGameConfig(seed);
        QRandom.SetRandomNumberGenerator(rng);
        RpgGameState.rng = rng;
        GenerateWorld(gameConfig);
        var gameStateInstance = RpgGameState.New(gameConfig);
        gameStateInstance.InitStaticState(true);
        RpgGameState.instance = gameStateInstance;

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

    private StarColor RandomStarSystemColor() {
        var colorRoll = QRandom.IntRange(0, 5);
        if (colorRoll == 0) {
            return StarColor.Blue;
        } else if (colorRoll == 1) {
            return StarColor.Green;
        } else if (colorRoll == 2) {
            return StarColor.Yellow;
        } else if (colorRoll == 3) {
            return StarColor.Orange;
        } else if (colorRoll == 4) {
            return StarColor.Red;
        } else {
            return StarColor.White;
        }
    }

    private RpgGameState.Config NewGameConfig(ulong gameSeed) {
        var limits = new RpgGameState.GameLimits {
            maxFuel = 500,
            droneCapacity = 500,
        };

        var skills = new HashSet<string>();
        var startingSkill = OptionValue("StartingSkill");
        if (!startingSkill.Empty()) {
            skills.Add(startingSkill);
        }

        var randomEvents = new HashSet<string>();
        foreach (var e in RandomEvent.list) {
            randomEvents.Add(e.title);
        }

        var config = new RpgGameState.Config{
            limits = limits,
            exodusPrice = 5000,
            dronePrice = 1200,
            startingCredits = OptionIntValue("StartingCredits"),
            startingFuel = (int)(limits.maxFuel) - 100,
            fuelPrice = 3,
            repairPrice = 7,
            travelSpeed = 60,
            randomEventCooldown = 20,

            gameSeed = gameSeed,

            skills = skills,
            randomEvents = randomEvents,
        };

        return config;
    }

    private static Vector2 RandomizedLocation(Vector2 loc, float size) {
        var halfSize = size / 2;
        float x = loc.x + QRandom.FloatRange(-halfSize, halfSize);
        float y = loc.y + QRandom.FloatRange(-halfSize, halfSize);
        return new Vector2(x, y);
    }

    private Vector2 RandomStarSystemPosition(Sector sector) {
        var attempts = 0;
        var result = Vector2.Zero;
        while (true) {
            attempts++;
            var dist = QRandom.FloatRange(175, 500);
            var toBeConnected = QRandom.Element(sector.systems);
            var candidate = RandomizedLocation(toBeConnected.pos, dist);
            if (!sector.rect.HasPoint(candidate)) {
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
        public Rect2 rect;

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
        new VesselTemplate{design = VesselDesign.Find("Raider"), roll = 0},
        new VesselTemplate{design = VesselDesign.Find("Marauder"), roll = 0.65f},
    };

    private VesselTemplate[] _krigiaTemplates = new VesselTemplate[]{
        new VesselTemplate{design = VesselDesign.Find("Talons"), roll = 0},
        new VesselTemplate{design = VesselDesign.Find("Claws"), roll = 0.3f},
        new VesselTemplate{design = VesselDesign.Find("Fangs"), roll = 0.55f},
        new VesselTemplate{design = VesselDesign.Find("Tusks"), roll = 0.75f},
        new VesselTemplate{design = VesselDesign.Find("Horns"), roll = 0.90f},
    };

    private VesselTemplate[] _wertuTemplates = new VesselTemplate[]{
        new VesselTemplate{design = VesselDesign.Find("Probe"), roll = 0},
        new VesselTemplate{design = VesselDesign.Find("Guardian"), roll = 0.3f},
        new VesselTemplate{design = VesselDesign.Find("Angel"), roll = 0.70f},
        new VesselTemplate{design = VesselDesign.Find("Dominator"), roll = 0.85f},
    };

    private void InitFleet(RpgGameState.Config config, StarBase starBase, VesselTemplate[] templates, float budget) {
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
                VesselFactory.Init(v, _template.design);
                fleet.Add(v.GetRef());
                budget -= cost;
                break;
            }
        }

        starBase.garrison = fleet;

        GD.Print("fleet = " + fleet.Count);
    }

    private void InitKrigiaFleet(RpgGameState.Config config, StarBase starBase, float budget) {
        InitFleet(config, starBase, _krigiaTemplates, budget);
    }

    private void InitWertuFleet(RpgGameState.Config config, StarBase starBase, float budget) {
        InitFleet(config, starBase, _wertuTemplates, budget);
    }

    private void InitScavengerFleet(RpgGameState.Config config, StarBase starBase, float budget) {
        InitFleet(config, starBase, _scavengerTemplates, budget);
    }

    private StarBase NewStarBase(RpgGameState.Config config, Faction owner, int level) {
        var starBase = config.starBases.New();
        starBase.level = level;
        starBase.owner = owner;
        starBase.mineralsStock = 70 + QRandom.IntRange(0, 30);
        starBase.organicStock = 10 + QRandom.IntRange(0, 50);
        starBase.powerStock = QRandom.IntRange(0, 50);
        return starBase;
    }

    private void DeployBases(RpgGameState.Config config, Faction faction, int numBases, Sector[] sectors, VesselTemplate[] templates) {
        while (numBases > 0) {
            var col = QRandom.IntRange(1, numMapCols - 1);
            var row = QRandom.IntRange(0, 1);
            var i = row * numMapCols + col;
            var sector = sectors[i];
            var j = QRandom.IntRange(0, sector.systems.Count - 1);
            if (sector.systems[j].starBase.id == 0 && sector.systems[j].color != StarColor.Purple) {
                var fleetRollBonus = (float)col * 20;
                var fleetRoll = QRandom.FloatRange(40, 80) + fleetRollBonus;
                var baseLevel = col + QRandom.IntRange(1, 2);
                
                var starBase = NewStarBase(config, faction, baseLevel);
                InitFleet(config, starBase, templates, fleetRoll);
                BindStarBase(starBase, sector.systems[j]);
                numBases--;
            }
        }
    }

    private void BindStarBase(StarBase starBase, StarSystem system) {
        system.starBase = starBase.GetRef();
        starBase.system = system.GetRef();
    }

    private StarSystem NewStarSystem(RpgGameState.Config config, HashSet<string> starSystenNames, Vector2 pos) {
        var sys = config.starSystems.New();
        sys.name = StarSystemNames.UniqStarSystemName(starSystenNames);
        sys.color = RandomStarSystemColor();
        sys.pos = pos;

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

    private void GenerateWorld(RpgGameState.Config config) {
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
                sector.rect = new Rect2(new Vector2(col * 685 + 550, row * 450 + 150), 650, 400);
                sectors[i] = sector;
                var middle = sector.rect.Position + sector.rect.Size / 2;

                var color = RandomStarSystemColor();
                sector.systems.Add(NewStarSystem(config, starSystenNames, RandomizedLocation(middle, 120)));

                var minSystems = 2;
                var maxSystems = 4;
                if (col == startingCol && row == startingRow) {
                    minSystems = 3;
                }
                var numSystems = QRandom.IntRange(minSystems, maxSystems);
                for (int j = 0; j < numSystems; j++) {
                    sector.systems.Add(NewStarSystem(config, starSystenNames, RandomStarSystemPosition(sector)));
                }
            }
        }

        {
            var anomalySystemCol = QRandom.IntRange(0, numMapCols-1);
            var anomalySystemRow = QRandom.IntRange(0, numMapRows-1);
            var anomalySystemIndex = anomalySystemRow * numMapCols + anomalySystemCol;
            var anomalySystem = config.starSystems.New();

            var sector = sectors[anomalySystemIndex];

            anomalySystem.name = "Eth";
            anomalySystem.color = StarColor.Purple;
            anomalySystem.pos = RandomStarSystemPosition(sector);

            sector.systems.Add(anomalySystem);
        }

        var startingStarBase = NewStarBase(config, Faction.Human, 1);

        var startingSystem = sectors[startingSector].systems[0];
        startingSystem.name = "Quasisol";
        startingSystem.color = StarColor.Yellow;
        BindStarBase(startingStarBase, startingSystem);
        startingSystem.resourcePlanets = new List<ResourcePlanet>{
            new ResourcePlanet(1, 0, 0),
        };

        var numKrigiaBases = QRandom.IntRange(7, 10);
        if (OptionValue("KrigiaPresence") == "high") {
            numKrigiaBases += 2;
        } else if (OptionValue("KrigiaPresence") == "minimal") {
            numKrigiaBases -= 2;
        }
        var numWertuBases = QRandom.IntRange(3, 4);
        var numZythBases = QRandom.IntRange(2, 3);
        GD.Print($"deployed {numKrigiaBases} Krigia bases");
        GD.Print($"deployed {numWertuBases} Wertu bases");
        GD.Print($"deployed {numZythBases} Zyth bases");

        // First step: deploy using the predetermined rules.
        if (OptionValue("KrigiaPresence") != "minimal") {
            var sector = sectors[startingSector];
            var starBase = NewStarBase(config, Faction.Krigia, 2);
            BindStarBase(starBase, sector.systems[1]);
            InitKrigiaFleet(config, starBase, 25);
            numKrigiaBases--;
        }
        {
            var secondSector = startingRow == 0 ? numMapCols : 0;
            var sector = sectors[secondSector];
            var roll = QRandom.FloatRange(35, 55);

            var base0 = NewStarBase(config, Faction.Krigia, 3);
            BindStarBase(base0, sector.systems[0]);
            InitKrigiaFleet(config, base0, roll);

            var base1 = NewStarBase(config, Faction.Scavenger, 2);
            BindStarBase(base1, sector.systems[1]);
            InitScavengerFleet(config, base1, roll);
            numKrigiaBases--;
        }

        // Second step: fill everything else.
        DeployBases(config, Faction.Krigia, numKrigiaBases, sectors, _krigiaTemplates);
        DeployBases(config, Faction.Wertu, numWertuBases, sectors, _wertuTemplates);

        config.startingSystemID = startingSystem.id;

        // foreach (var sector in sectors) {
        //     foreach (var sys in sector.systems) {
        //         if (sys == startingSystem) {
        //             config.startingSystemID = sys.id;
        //         }
        //         if (sys.starBase != null && sys.starBase.owner == config.krigiaPlayer) {
        //             sys.starBase.botPatrolDelay = QRandom.IntRange(10, 60);
        //         }
        //     }
        // }

        GD.Print($"generated {config.starSystems.objects.Count} star systems");

        // var artifactsNeeded = 10;
        // var artifacts
        var systems = new List<StarSystem>();
        foreach (var sys in config.starSystems.objects.Values) {
            systems.Add(sys);
        }
        foreach (var art in ArtifactDesign.list) {
            while (true) {
                var sys = QRandom.Element(systems);
                if (sys.artifact != null || sys.color == StarColor.Purple) {
                    continue;
                }
                if (sys == startingSystem) {
                    continue;
                }
                sys.artifact = art.name;
                sys.artifactRecoveryDelay = QRandom.IntRange(10, 40);
                GD.Print("placed " + art.name + " in " + sys.name + " with " + sys.artifactRecoveryDelay + " recovery delay");
                break;
            }   
        }

        var fleet = new List<Vessel.Ref>();
        var humanVessel = config.vessels.New();
        humanVessel.isGamepad = GameControls.preferGamepad;
        humanVessel.faction = Faction.Human;
        humanVessel.pilotName = PilotNames.UniqHumanName(config.usedNames);
        humanVessel.designName = OptionValue("FlagshipDesign");
        humanVessel.energySourceName = "Power Generator";
        humanVessel.artifacts = new List<string>{
            EmptyArtifact.Design.name,
            EmptyArtifact.Design.name,
            EmptyArtifact.Design.name,
            EmptyArtifact.Design.name,
            EmptyArtifact.Design.name,
        };
        humanVessel.weapons = new List<string>{
            IonCannonWeapon.Design.name,
            EmptyWeapon.Design.name,
        };
        humanVessel.specialWeaponName = EmptyWeapon.Design.name;
        VesselFactory.InitStats(humanVessel);
        fleet.Add(humanVessel.GetRef());

        for (int i = 0; i < OptionIntValue("StartingFleet"); i++) {
            var v = config.vessels.New();
            v.isBot = true;
            v.faction = Faction.Human;
            v.pilotName = PilotNames.UniqHumanName(config.usedNames);
            VesselFactory.Init(v, "Earthling Scout");
            fleet.Add(v.GetRef());
        }

        var unit = config.spaceUnits.New();
        unit.owner = Faction.Human;
        unit.fleet = fleet;
        unit.pos = startingSystem.pos;

        config.humanUnit = unit.GetRef();
    }
}
