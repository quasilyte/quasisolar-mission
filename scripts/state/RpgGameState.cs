using Godot;
using System;
using System.Collections.Generic;

public class RpgGameState {
    public static RpgGameState instance;

    public static RandomNumberGenerator rng;

    public class Config {
        public ulong gameSeed;

        public RandomNumberGenerator rng;

        public int dronePrice;
        public int repairPrice;
        public int fuelPrice;
        public int exodusPrice;

        public float travelSpeed;

        public int startingFuel;
        public int startingCredits;
        public int randomEventCooldown;

        public int startingSystemID;

        public HashSet<string> randomEvents;

        public HashSet<string> skills;

        public HashSet<string> usedNames = new HashSet<string>();

        public List<StarSystem> starSystems = new List<StarSystem>();
        public Dictionary<StarSystem, List<StarSystem>> starSystemConnections = new Dictionary<StarSystem, List<StarSystem>>();
        public Dictionary<Vector2, StarSystem> starSystemByPos = new Dictionary<Vector2, StarSystem>();

        public GameLimits limits;

        public HashSet<StarBase> humanBases = new HashSet<StarBase>();

        public SpaceUnit humanUnit;

        public Player humanPlayer;
        public Player scavengerPlayer;
        public Player krigiaPlayer;
        public Player wertuPlayer;
        public Player zythPlayer;
    }

    public class GameLimits {
        // How much resources can drone collect?
        // TODO: make it a per-drone stat?
        public int droneCapacity;

        public float maxFuel;
    }

    public enum MapTransition {
        NewGame,
        ExitStarBase,
        ExitResearchScreen,
        UnitDestroyed,
        BaseAttackSimulation,
        EnemyUnitDestroyed,
        EnemyBaseAttackRepelled,
    }

    public class MapState {
        public bool movementEnabled;
        public UnitMode mode;
    }

    public class KrigiaPlans {
        public int taskForceDelay = 0;
    }

    public RpgGameState(Config c) {
        seed = c.gameSeed;
        rng = c.rng;

        dronePrice = c.dronePrice;
        repairPrice = c.repairPrice;
        fuelPrice = c.fuelPrice;
        exodusPrice = c.exodusPrice;

        starSystems = c.starSystems;
        starSystemByPos = c.starSystemByPos;
        starSystemConnections = c.starSystemConnections;

        fuel = c.startingFuel;
        credits = c.startingCredits;

        usedNames = c.usedNames;

        humanBases = c.humanBases;
        humanUnit = c.humanUnit;

        limits = c.limits;

        startingSystemID = c.startingSystemID;

        travelSpeed = c.travelSpeed;

        randomEventCooldown = c.randomEventCooldown;
        randomEventsAvailable = c.randomEvents;

        skillsLearned = c.skills;

        humanPlayer = c.humanPlayer;
        scavengerPlayer = c.scavengerPlayer;
        krigiaPlayer = c.krigiaPlayer;
        wertuPlayer = c.wertuPlayer;
        zythPlayer = c.zythPlayer;
    }

    public KrigiaPlans krigiaPlans = new KrigiaPlans();

    public MapTransition transition = MapTransition.NewGame;
    public BattleResult lastBattleResult = null;

    public StarBase enteredBase = null;

    public Player humanPlayer = null;
    public Player scavengerPlayer = null;
    public Player krigiaPlayer = null;
    public Player wertuPlayer = null;
    public Player zythPlayer = null;

    public int krigiaReputation = 0;
    public int wertuReputation = 0;
    public int zythReputation = 0;

    public int drones = 0;
    public int dronePrice;
    public int dronwsOwned = 0;

    public ulong seed;

    public AbstractItem[] storage = new AbstractItem[14];

    public int credits = 0;

    public int repairPrice;
    public int fuelPrice;
    public int exodusPrice;

    public int experience = 0;

    public float fuel = 0;

    public int day = 1;

    public int travelSlowPoints = 0;
    public float travelSpeed;

    public MapState mapState = new MapState();

    public GameLimits limits;

    public HashSet<ArtifactDesign> artifactsRecovered = new HashSet<ArtifactDesign>{};

    public HashSet<string> technologiesResearched = new HashSet<string>{};
    public double researchProgress = 0;
    public Research currentResearch = null;
    public int krigiaMaterial = 0;
    public int wertuMaterial = 0;
    public int zythMaterial = 0;
    public int scienceFunds = 0;
    public bool metKrigia = false;
    public bool metWertu = false;
    public bool metZyth = false;
    
    public HashSet<string> skillsLearned;

    public HashSet<string> usedNames;
    public HashSet<ResourcePlanet> planetsWithMines = new HashSet<ResourcePlanet>{};
    public HashSet<StarBase> humanBases;

    public HashSet<SpaceUnit> spaceUnits = new HashSet<SpaceUnit>{};

    public HashSet<string> randomEventsAvailable;
    public int randomEventCooldown;

    public SpaceUnit humanUnit;

    // starSystems is a list of all star systems existing in this session.
    // starSystems[startingSystemID] is a starting star system.
    public List<StarSystem> starSystems;

    public Dictionary<StarSystem, List<StarSystem>> starSystemConnections;
    public Dictionary<Vector2, StarSystem> starSystemByPos;

    public int startingSystemID;

    public int enemyBaseNumAttackers = 0;
    public SpaceUnit enemyAttackerUnit;
    public StarBase garrisonStarBase = null;

    public static StarSystem StartingSystem() {
        return instance.starSystems[instance.startingSystemID];
    }

    public static double ResearchRate() {
        double value = 0.5;
        if (instance.scienceFunds >= 5) {
            var fundsScore = (Math.Log((double)instance.scienceFunds / 3) / 100) * 5;
            value += QMath.ClampMax(fundsScore, 0.5);
        }
        if (instance.currentResearch != null && instance.currentResearch.material != Research.Material.None) {
            // Without needed material, research is performed at the 25% rate.
            if (instance.currentResearch.material == Research.Material.Krigia && instance.krigiaMaterial == 0) {
                value *= 0.25;
            } else if (instance.currentResearch.material == Research.Material.Wertu && instance.wertuMaterial == 0) {
                value *= 0.25;
            } else if (instance.currentResearch.material == Research.Material.Zyth && instance.zythMaterial == 0) {
                value *= 0.25;
            }
            if (instance.technologiesResearched.Contains("Alien Tech Lab")) {
                value = QMath.ClampMax(value + 0.1, 1);
            }
        }
        return value;
    }

    public static int MaxDrones() {
        var value = 5;
        if (instance.skillsLearned.Contains("Drone Control II")) {
            value = 15;
        } else if (instance.skillsLearned.Contains("Drone Control I")) {
            value = 10;
        }
        return value;
    }

    public static void AddFuel(float amount) {
        instance.fuel = QMath.Clamp(instance.fuel + amount, 0, MaxFuel());
    }

    public static float MaxFuel() {
        var value = instance.limits.maxFuel;
        if (instance.technologiesResearched.Contains("Improved Fuel Tanks III")) {
            value *= 1.2f;
        } else if (instance.technologiesResearched.Contains("Improved Fuel Tanks II")) {
            value *= 1.1f;
        } else if (instance.technologiesResearched.Contains("Improved Fuel Tanks")) {
            value *= 1.05f;
        }
        return value;
    }

    public static float RadarRange() {
        var value = 300f;
        if (instance.technologiesResearched.Contains("Jump Tracer Mk3")) {
            value *= 1.25f;
        } else if (instance.technologiesResearched.Contains("Jump Tracer Mk2")) {
            value *= 1.15f;
        }
        return value;
    }

    public static int DebrisSellPrice() { return 12; }
    public static int MineralsSellPrice() { return 14; }
    public static int OrganicSellPrice() { return 20; }
    public static int PowerSellPrice() { return 22; }

    public static void PutItemToStorage(AbstractItem item) {
        for (int i = 0; i < 14; i++) {
            if (instance.storage[i] == null) {
                instance.storage[i] = item;
                return;
            }
        }
    }
}