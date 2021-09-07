using Godot;
using System;
using System.Collections.Generic;

public class NewGameScene : Node2D {
    private Label _scoreMultiplierLabel;
    private NewGameOptions _options;

    public override void _Ready() {
        _scoreMultiplierLabel = GetNode<Label>("ScoreMultiplier");

        _options = new NewGameOptions();

        foreach (var kv in _options.byName) {
            var id = kv.Key;
            var button = GetNode<OptionButton>(id);
            var selected = 1;
            for (int i = 0; i < kv.Value.Length; i++) {
                if (kv.Value[i].selected) {
                    selected = i;
                }
                button.AddItem(kv.Value[i].text, i);
            }
            var args = new Godot.Collections.Array { id };
            button.Connect("item_selected", this, nameof(OnItemSelected), args);
            button.Select(selected);
        }

        GetNode<Button>("Start").Connect("pressed", this, nameof(OnStartButtonPressed));

        GetNode<LineEdit>("GameSeed").Text = NewGameSeed.Generate();

        UpdateScoreMiltiplier();
    }

    private int CalculateScoreMultiplier() {
        var score = 200;
        foreach (var kv in _options.byName) {
            var button = GetNode<OptionButton>(kv.Key);
            score += _options.byName[kv.Key][button.Selected].score;
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
        return (string)_options.byName[name][GetNode<OptionButton>(name).Selected].value;
    }

    private int OptionIntValue(string name) {
        return (int)_options.byName[name][GetNode<OptionButton>(name).Selected].value;;
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

        gameStateInstance.explorationDrones.Add("Curiosity");

        GetTree().ChangeScene("res://scenes/MapView.tscn");
    }

    private ulong GameSeed() {
        var s = GetNode<LineEdit>("GameSeed").Text;
        if (string.IsNullOrEmpty(s)) {
            s = NewGameSeed.Generate();
        }
        return (ulong)s.GetHashCode();
    }

    private RpgGameState.Config NewGameConfig(ulong gameSeed) {
        var limits = new RpgGameState.GameLimits {
            maxFuel = 500,
        };

        var randomEvents = new HashSet<string>();
        foreach (var e in MapEventRegistry.list) {
            randomEvents.Add(e.title);
        }

        var config = new RpgGameState.Config{
            limits = limits,
            exodusPrice = 5000,
            startingCredits = OptionIntValue("StartingCredits"),
            startingFuel = (int)(limits.maxFuel) - 100,
            travelSpeed = 60,
            randomEventCooldown = 20,

            gameSeed = gameSeed,

            randomEvents = randomEvents,

            missionDeadline = OptionIntValue("MissionDeadline"),
        };

        return config;
    }

    const int numMapCols = 3;
    const int numMapRows = 2;
    const int minSectorSystems = 3;
    const int maxSectorSystems = 4;

    private StarBase NewStarBase(RpgGameState.Config config, Faction owner, int level) {
        var starBase = config.starBases.New();
        starBase.level = level;
        starBase.owner = owner;
        starBase.mineralsStock = 70 + QRandom.IntRange(0, 30);
        starBase.organicStock = 10 + QRandom.IntRange(0, 50);
        starBase.powerStock = QRandom.IntRange(0, 50);
        return starBase;
    }

    private void DeployBases(List<WorldTemplate.System> neutralSystems, Faction faction, int numBases) {
        if (neutralSystems.Count < numBases) {
            throw new Exception($"can't deploy {numBases} for {faction}");
        }
        while (numBases > 0) {
            var sys = QRandom.Element(neutralSystems);
            var fleetRollBonus = (float)sys.sector.level * 10;
            var fleetRoll = QRandom.FloatRange(40, 80) + fleetRollBonus;
            var config = sys.sector.world.config;

            var baseLevel = QRandom.IntRange(1, 3);
            if (sys.sector.level == 2) {
                baseLevel++;
            } else if (sys.sector.level == 3) {
                baseLevel += 2;
            }

            var starBase = NewStarBase(config, faction, baseLevel);
            NewGameFleetGen.InitFleet(config, starBase, faction, fleetRoll);
            BindStarBase(starBase, sys.data);
            neutralSystems.Remove(sys);
            numBases--;
        }
    }

    private void BindStarBase(StarBase starBase, StarSystem system) {
        system.starBase = starBase.GetRef();
        starBase.system = system.GetRef();
    }

    private void GenerateWorld(RpgGameState.Config config) {
        var sectors = new List<WorldTemplate.Sector>();
        var world = new WorldTemplate{
            config = config,
            sectors = sectors,
        };

        // Allocate all sectors.
        for (int row = 0; row < numMapRows; row++) {
            for (int col = 0; col < numMapCols; col++) {
                var sector = new WorldTemplate.Sector();
                sector.world = world;
                sector.col = col;
                sector.row = row;
                sector.rect = new Rect2(new Vector2(col * 685 + 550, row * 450 + 150), 650, 400);
                sectors.Add(sector);
            }
        }

        // Fill sectors with star systems.
        {
            var starSystenNames = new HashSet<string>();
            foreach (var sector in sectors) {
                var middle = sector.rect.Position + sector.rect.Size / 2;
                sector.systems.Add(NewGameSysGen.NewStarSystem(sector, QMath.RandomizedLocation(middle, 240)));
                var numSystems = QRandom.IntRange(minSectorSystems, maxSectorSystems);
                for (int j = 0; j < numSystems; j++) {
                    var pos = NewGameSysGen.PickStarSystemPos(sector);
                    sector.systems.Add(NewGameSysGen.NewStarSystem(sector, pos));
                }
            }
        }

        // Assign sector levels.
        {
            // Player always starts in the middle of the map.
            var startingCol = 1;
            var startingRow = QRandom.IntRange(0, 1);
            var i = startingCol + startingRow * numMapCols;
            world.startingSector = sectors[i];
            world.startingSystem = world.startingSector.systems[0];

            // There are two possible sector level layouts:
            //
            // [2][0][2]     [3][1][3]
            // [3][1][3] and [2][0][2]
            //
            // Which one do we get depends on the starting row number.

            world.startingSector.level = 0;
            foreach (var sector in sectors) {
                if (sector == world.startingSector) {
                    continue;
                }
                if (sector.col == world.startingSector.col) {
                    sector.level = 1;
                    continue;
                }
                if (sector.row == world.startingSector.row) {
                    sector.level = 2;
                } else {
                    sector.level = 3;
                }
            }
        }

        // Deploy the purple star system.
        {
            var anomalySystemCol = QRandom.IntRange(0, numMapCols-1);
            var anomalySystemRow = QRandom.IntRange(0, numMapRows-1);
            var anomalySystemIndex = anomalySystemRow * numMapCols + anomalySystemCol;
            var anomalySystem = config.starSystems.New();

            var sector = sectors[anomalySystemIndex];

            anomalySystem.name = "Eth";
            anomalySystem.color = StarColor.Purple;
            anomalySystem.pos = NewGameSysGen.PickStarSystemPos(sector);

            sector.systems.Add(new WorldTemplate.System{
                data = anomalySystem,
                sector = sector,
            });
        }

        GD.Print($"created {config.starSystems.objects.Count} star systems");

        var numDraklidBases = 2;
        if (OptionValue("DraklidPresence") == "high") {
            numDraklidBases++;
        } else if (OptionValue("DraklidPresence") == "minimal") {
            numDraklidBases--;
        }
        var numKrigiaBases = 5;
        if (OptionValue("KrigiaPresence") == "high") {
            numKrigiaBases++;
        } else if (OptionValue("KrigiaPresence") == "minimal") {
            numKrigiaBases--;
        }
        var numWertuBases = 3;
        var numZythBases = 2;

        // Deploy the Earthlings star base. 
        {
            var startingStarBase = NewStarBase(config, Faction.Earthling, 1);
            var startingSystem = world.startingSystem.data;
            startingSystem.name = "Quasisol";
            startingSystem.color = StarColor.Yellow;
            BindStarBase(startingStarBase, startingSystem);
            var solPlanetsHash = new HashSet<string>();
            var solPlanet = PlanetGenerator.NewResourcePlanet(0.05f, 1, solPlanetsHash);
            solPlanet.temperature = QRandom.IntRange(30, 70);
            solPlanet.textureName = PlanetGenerator.PickPlanetSprite("dry", solPlanetsHash);
            startingSystem.resourcePlanets = new List<ResourcePlanet>{
                solPlanet,
                PlanetGenerator.NewResourcePlanet(0.3f, 1, solPlanetsHash),
            };

            var defender = config.vessels.New();
            defender.isBot = true;
            defender.faction = Faction.Earthling;
            defender.pilotName = PilotNames.UniqHumanName(config.usedNames);
            VesselFactory.PadEquipment(defender);
            defender.weapons = new List<string>{
                NeedleGunWeapon.Design.name,
                IonCannonWeapon.Design.name,
            };
            defender.designName = "Fighter";
            defender.energySourceName = "Power Generator";
            VesselFactory.InitStats(defender);
            VesselFactory.RollUpgrades(defender);
            startingStarBase.garrison.Add(defender.GetRef());
        }

        // For a normal game, there is a Krigia base in a starting sector.
        if (OptionValue("KrigiaPresence") != "minimal") {
            var sector = world.startingSector;
            var starBase = NewStarBase(config, Faction.Krigia, 2);
            BindStarBase(starBase, sector.systems[1].data);
            NewGameFleetGen.InitFleet(config, starBase, Faction.Krigia, 25);
            numKrigiaBases--;
        }

        // Populate the star systems with bases.
        {
            var neutralSystems = new List<WorldTemplate.System>();
            foreach (var sector in sectors) {
                foreach (var system in sector.systems) {
                    if (system.data.color == StarColor.Purple) {
                        continue;
                    }
                    if (system.data.starBase.id == 0) {
                        neutralSystems.Add(system);
                    }
                }
            }
            DeployBases(neutralSystems, Faction.Krigia, numKrigiaBases);
            DeployBases(neutralSystems, Faction.Wertu, numWertuBases);
            DeployBases(neutralSystems, Faction.Zyth, numZythBases);
            DeployBases(neutralSystems, Faction.Draklid, numDraklidBases);
            DeployBases(neutralSystems, Faction.Phaa, 1);
            DeployBases(neutralSystems, Faction.Vespion, 1);
        }

        config.startingSystemID = world.startingSystem.data.id;

        // Deploy the artifacts.
        {
            var systems = new List<StarSystem>();
            foreach (var sys in config.starSystems.objects.Values) {
                systems.Add(sys);
            }
            foreach (var art in ArtifactDesign.list) {
                while (true) {
                    var sys = QRandom.Element(systems);
                    if (sys.HasArtifact() || sys.color == StarColor.Purple) {
                        continue;
                    }
                    if (sys == world.startingSystem.data) {
                        continue;
                    }
                    var planet = QRandom.Element(sys.resourcePlanets);
                    planet.artifact = art.name;
                    break;
                }   
            }
        }

        // Create a player-controlled unit.
        {
            var fleet = new List<Vessel.Ref>();
            var humanVessel = config.vessels.New();
            humanVessel.isGamepad = GameControls.preferGamepad;
            humanVessel.faction = Faction.Earthling;
            humanVessel.pilotName = PilotNames.UniqHumanName(config.usedNames);
            humanVessel.designName = "Explorer";
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
            VesselFactory.InitStats(humanVessel);
            VesselFactory.RollUpgrades(humanVessel);
            fleet.Add(humanVessel.GetRef());

            var numScoutsEscort = 1;
            for (int i = 0; i < numScoutsEscort; i++) {
                var v = config.vessels.New();
                v.isBot = true;
                v.faction = Faction.Earthling;
                v.pilotName = PilotNames.UniqHumanName(config.usedNames);
                VesselFactory.Init(v, "Earthling Scout");
                VesselFactory.RollUpgrades(v);
                fleet.Add(v.GetRef());
            }
            var unit = config.spaceUnits.New();
            unit.owner = Faction.Earthling;
            unit.fleet = fleet;
            unit.pos = world.startingSystem.data.pos;
            config.humanUnit = unit.GetRef();
        }

        // Make sure that there is at least 1 gas giant planet.
        {
            var gasGiantsNum = 0;
            var starSystems = new List<StarSystem>(config.starSystems.objects.Values);
            foreach (var sys in starSystems) {
                foreach (var p in sys.resourcePlanets) {
                    if (p.gasGiant) {
                        gasGiantsNum++;
                        GD.Print("gas giant at " + sys.name);
                    }
                }
            }
            if (gasGiantsNum == 0) {
                while (true) {
                    var sys = QRandom.Element(starSystems);
                    if (sys.resourcePlanets.Count == 3) {
                        continue;
                    }
                    if (sys.color == StarColor.Purple) {
                        continue;
                    }
                    var hashSet = new HashSet<string>();
                    var p = PlanetGenerator.NewResourcePlanet(0.05f, 1, hashSet);
                    p.gasGiant = true;
                    p.mineralsPerDay = 0;
                    p.organicPerDay = 0;
                    p.powerPerDay = 1;
                    p.temperature = QRandom.IntRange(-20, 20);
                    p.textureName = PlanetGenerator.PickPlanetSprite("gas", hashSet);
                    sys.resourcePlanets.Add(p);
                    gasGiantsNum = 1;
                    GD.Print("gas giant at " + sys.name);
                    break;
                }
            }
        }
    }
}
