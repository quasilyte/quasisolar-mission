using Godot;
using System;
using System.Collections.Generic;

public class RpgGameState {
    public static RpgGameState instance;

    public static RandomNumberGenerator rng;

    public static Dictionary<StarSystem, List<StarSystem>> starSystemConnections;
    public static Dictionary<Vector2, StarSystem> starSystemByPos;
    public static HashSet<ResourcePlanet> planetsWithMines;
    public static HashSet<StarBase> humanBases;

    public static int enemyBaseNumAttackers = 0;
    public static SpaceUnit enemyAttackerUnit;
    public static StarBase garrisonStarBase = null;
    public static StarBase enteredBase = null;
    public static MapTransition transition = MapTransition.NewGame;
    public static BattleResult lastBattleResult = null;

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

        public GameLimits limits;

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
        LoadGame,
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

    public RpgGameState() {}

    public void InitStaticState(bool newGame) {
        enemyBaseNumAttackers = 0;
        enemyAttackerUnit = null;
        garrisonStarBase = null;
        enteredBase = null;
        transition = newGame ? MapTransition.NewGame : MapTransition.LoadGame;
        lastBattleResult = null;

        humanBases = new HashSet<StarBase>();
        foreach (var sys in starSystems) {
            if (sys.starBase == null) {
                continue;
            }
            if (sys.starBase.owner != humanPlayer) {
                continue;
            }
            humanBases.Add(sys.starBase);
        }

        planetsWithMines = new HashSet<ResourcePlanet>();
        foreach (var sys in starSystems) {
            foreach (var p in sys.resourcePlanets) {
                if (p.hasMine) {
                    planetsWithMines.Add(p);
                }
            }
        }

        starSystemByPos = new Dictionary<Vector2, StarSystem>();
        foreach (var sys in starSystems) {
            starSystemByPos[sys.pos] = sys;
        }

        // TODO: do it more efficiently than O(n^2)?
        starSystemConnections = new Dictionary<StarSystem, List<StarSystem>>();
        var graph = starSystemConnections;
        Func<StarSystem, StarSystem, bool> addToGraph = (StarSystem sys, StarSystem connected) => {
            if (!graph.ContainsKey(sys)) {
                graph.Add(sys, new List<StarSystem>());
            }
            var list = graph[sys];
            list.Add(connected);
            return true;
        };
        for (int i = 0; i < starSystems.Count; i++) {
            var sys = starSystems[i];
            for (int j = 0; j < starSystems.Count; j++) {
                if (i == j) {
                    continue;
                }
                var other = starSystems[j];
                if (sys.pos.DistanceTo(other.pos) > 600) {
                    continue;
                }
                addToGraph(sys, other);
                addToGraph(other, sys);
            }
        }
        foreach (var sys in starSystems) {
            if (!graph.ContainsKey(sys)) {
                throw new Exception("found a system that is not included into the graph");
            }
            var connections = graph[sys];
            if (connections.Count == 0) {
                throw new Exception("found a system with 0 connections");
            }
        }
    }

    public static RpgGameState New(Config c) {
        var o = new RpgGameState();

        o.seed = c.gameSeed;

        o.dronePrice = c.dronePrice;
        o.repairPrice = c.repairPrice;
        o.fuelPrice = c.fuelPrice;
        o.exodusPrice = c.exodusPrice;

        o.starSystems = c.starSystems;

        o.fuel = c.startingFuel;
        o.credits = c.startingCredits;

        o.usedNames = c.usedNames;

        o.humanUnit = c.humanUnit;

        o.limits = c.limits;

        o.startingSystemID = c.startingSystemID;

        o.travelSpeed = c.travelSpeed;

        o.randomEventCooldown = c.randomEventCooldown;
        o.randomEventsAvailable = c.randomEvents;

        o.skillsLearned = c.skills;

        o.humanPlayer = c.humanPlayer;
        o.scavengerPlayer = c.scavengerPlayer;
        o.krigiaPlayer = c.krigiaPlayer;
        o.wertuPlayer = c.wertuPlayer;
        o.zythPlayer = c.zythPlayer;

        o.starBaseBySpaceUnit = new Dictionary<SpaceUnit, StarBase>();
        o.starSystemByStarBase = new Dictionary<StarBase, StarSystem>();
        foreach (var sys in c.starSystems) {
            if (sys.starBase != null) {
                o.starSystemByStarBase.Add(sys.starBase, sys);
                foreach (var u in sys.starBase.units) {
                    o.starBaseBySpaceUnit.Add(u, sys.starBase);
                }
            }
        }
        
        return o;
    }

    public KrigiaPlans krigiaPlans = new KrigiaPlans();

    public Dictionary<SpaceUnit, StarBase> starBaseBySpaceUnit;
    public Dictionary<StarBase, StarSystem> starSystemByStarBase;

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
    public int dronesOwned = 0;

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

    public HashSet<string> artifactsRecovered = new HashSet<string>{};

    public HashSet<string> technologiesResearched = new HashSet<string>{};
    public double researchProgress = 0;
    public string currentResearch = "";
    public int krigiaMaterial = 0;
    public int wertuMaterial = 0;
    public int zythMaterial = 0;
    public int scienceFunds = 0;
    public bool metKrigia = false;
    public bool metWertu = false;
    public bool metZyth = false;
    
    public HashSet<string> skillsLearned;

    public HashSet<string> usedNames;

    public HashSet<SpaceUnit> spaceUnits = new HashSet<SpaceUnit>{};

    public HashSet<string> randomEventsAvailable;
    public int randomEventCooldown;

    public SpaceUnit humanUnit;

    // starSystems is a list of all star systems existing in this session.
    // starSystems[startingSystemID] is a starting star system.
    public List<StarSystem> starSystems;

    public int startingSystemID;

    public static StarSystem StartingSystem() {
        return instance.starSystems[instance.startingSystemID];
    }

    public static double ResearchRate() {
        double value = 0.5;
        if (instance.scienceFunds >= 5) {
            var fundsScore = (Math.Log((double)instance.scienceFunds / 3) / 100) * 5;
            value += QMath.ClampMax(fundsScore, 0.5);
        }
        if (instance.currentResearch != "") {
            var research = Research.Find(instance.currentResearch);
            if (research.material != Research.Material.None) {
                // Without needed material, research is performed at the 25% rate.
                if (research.material == Research.Material.Krigia && instance.krigiaMaterial == 0) {
                    value *= 0.25;
                } else if (research.material == Research.Material.Wertu && instance.wertuMaterial == 0) {
                    value *= 0.25;
                } else if (research.material == Research.Material.Zyth && instance.zythMaterial == 0) {
                    value *= 0.25;
                }
                if (instance.technologiesResearched.Contains("Alien Tech Lab")) {
                    value = QMath.ClampMax(value + 0.1, 1);
                }
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