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

        GetNode<LineEdit>("GameSeed").Text = RandomGameSeed();

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

    private static Vector2 RandomizedLocation(Vector2 loc, float size) {
        var halfSize = size / 2;
        float x = loc.x + QRandom.FloatRange(-halfSize, halfSize);
        float y = loc.y + QRandom.FloatRange(-halfSize, halfSize);
        return new Vector2(x, y);
    }

    private Vector2 RandomStarSystemPosition(WorldTemplate.Sector sector) {
        var attempts = 0;
        var result = Vector2.Zero;
        while (true) {
            attempts++;
            var dist = QRandom.FloatRange(175, 500);
            var toBeConnected = QRandom.Element(sector.systems);
            var candidate = RandomizedLocation(toBeConnected.data.pos, dist);
            if (!sector.rect.HasPoint(candidate)) {
                continue;
            }
            var retry = false;
            foreach (var sys in sector.systems) {
                if (sys.data.pos.DistanceTo(candidate) < 170) {
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

    class VesselTemplate {
        public VesselDesign design;
        public float roll;
    }

    const int numMapCols = 3;
    const int numMapRows = 2;

    private StarBase NewStarBase(RpgGameState.Config config, Faction owner, int level) {
        var starBase = config.starBases.New();
        starBase.level = level;
        starBase.owner = owner;
        starBase.mineralsStock = 70 + QRandom.IntRange(0, 30);
        starBase.organicStock = 10 + QRandom.IntRange(0, 50);
        starBase.powerStock = QRandom.IntRange(0, 50);
        return starBase;
    }

    private void DeployBases(WorldTemplate world, Faction faction, int numBases) {
        var config = world.config;
        while (numBases > 0) {
            var col = QRandom.Bool() ? 0 : 2;
            var row = QRandom.IntRange(0, 1);
            var i = row * numMapCols + col;
            var sector = world.sectors[i];
            var j = QRandom.IntRange(0, sector.systems.Count - 1);
            if (sector.systems[j].data.starBase.id == 0 && sector.systems[j].data.color != StarColor.Purple) {
                // var fleetRollBonus = (float)col * 20;
                var fleetRollBonus = 0.0f;
                var fleetRoll = QRandom.FloatRange(40, 80) + fleetRollBonus;
                var baseLevel = QRandom.IntRange(1, 4);
                
                var starBase = NewStarBase(config, faction, baseLevel);
                NewGameFleetGen.InitFleet(config, starBase, faction, fleetRoll);
                BindStarBase(starBase, sector.systems[j].data);
                numBases--;
            }
        }
    }

    private void BindStarBase(StarBase starBase, StarSystem system) {
        system.starBase = starBase.GetRef();
        starBase.system = system.GetRef();
    }

    private WorldTemplate.System NewStarSystem(WorldTemplate.Sector sector, Vector2 pos) {
        var world = sector.world;
        var sys = world.config.starSystems.New();
        sys.name = StarSystemNames.UniqStarSystemName(world.starSystenNames);
        sys.color = RandomStarSystemColor();
        sys.pos = pos;

        var worldSys = new WorldTemplate.System{
            sector = sector,
            data = sys,
        };

        PlanetGenerator.GeneratePlanets(worldSys);

        return worldSys;
    }

    private void GenerateWorld(RpgGameState.Config config) {
        // Player always starts in the middle of the map.
        var startingCol = 1;
        var startingRow = QRandom.IntRange(0, 1);
        var startingSector = startingCol + startingRow * numMapCols;

        var sectors = new List<WorldTemplate.Sector>();
        var world = new WorldTemplate{
            config = config,
            sectors = sectors,
        };
        for (int i = 0; i < 6; i++) {
            sectors.Add(null);
        }

        var starSystenNames = new HashSet<string>();
        for (int col = 0; col < numMapCols; col++) {
            for (int row = 0; row < 2; row++) {
                var i = row * numMapCols + col;
                var sector = new WorldTemplate.Sector();
                sector.world = world;
                sector.level = col; // FIXME
                sector.rect = new Rect2(new Vector2(col * 685 + 550, row * 450 + 150), 650, 400);
                sectors[i] = sector;
                var middle = sector.rect.Position + sector.rect.Size / 2;

                var color = RandomStarSystemColor();
                sector.systems.Add(NewStarSystem(sector, RandomizedLocation(middle, 120)));

                var minSystems = 3;
                var maxSystems = 4;
                var numSystems = QRandom.IntRange(minSystems, maxSystems);
                for (int j = 0; j < numSystems; j++) {
                    sector.systems.Add(NewStarSystem(sector, RandomStarSystemPosition(sector)));
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

            sector.systems.Add(new WorldTemplate.System{
                data = anomalySystem,
                sector = sector,
            });
        }

        var startingStarBase = NewStarBase(config, Faction.Earthling, 1);

        var startingSystem = sectors[startingSector].systems[0].data;
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
        GD.Print($"deployed {numKrigiaBases} Krigia bases");
        GD.Print($"deployed {numWertuBases} Wertu bases");
        GD.Print($"deployed {numZythBases} Zyth bases");

        // First step: deploy using the predetermined rules.
        if (OptionValue("KrigiaPresence") != "minimal") {
            var sector = sectors[startingSector];
            var starBase = NewStarBase(config, Faction.Krigia, 2);
            BindStarBase(starBase, sector.systems[1].data);
            NewGameFleetGen.InitFleet(config, starBase, Faction.Krigia, 25);
            numKrigiaBases--;
        }
        {
            var secondRow = startingRow == 0 ? 1 : 0;
            var secondCol = 1;
            var secondSector = secondCol + secondRow * numMapCols;
            var sector = sectors[secondSector];
            var roll = QRandom.FloatRange(35, 55);

            var base0 = NewStarBase(config, Faction.Krigia, 2);
            BindStarBase(base0, sector.systems[0].data);
            NewGameFleetGen.InitFleet(config, base0, Faction.Krigia, roll);

            var base1 = NewStarBase(config, Faction.Draklid, 2);
            BindStarBase(base1, sector.systems[1].data);
            NewGameFleetGen.InitFleet(config, base1, Faction.Draklid, roll);
            numDraklidBases--;
        }

        // Second step: fill everything else.
        DeployBases(world, Faction.Krigia, numKrigiaBases);
        DeployBases(world, Faction.Wertu, numWertuBases);
        DeployBases(world, Faction.Zyth, numZythBases);
        DeployBases(world, Faction.Draklid, numDraklidBases);
        DeployBases(world, Faction.Phaa, 1);

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
                if (sys.HasArtifact() || sys.color == StarColor.Purple) {
                    continue;
                }
                if (sys == startingSystem) {
                    continue;
                }
                var planet = QRandom.Element(sys.resourcePlanets);
                planet.artifact = art.name;
                GD.Print("placed " + art.name + " in " + sys.name + " at " + planet.name);
                break;
            }   
        }

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
        unit.pos = startingSystem.pos;

        config.humanUnit = unit.GetRef();
    }
}
