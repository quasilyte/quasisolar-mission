using Godot;
using System;
using System.Collections.Generic;

public static class RpgGameState {
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

    public static KrigiaPlans krigiaPlans;

    public static MapTransition transition;
    public static BattleResult lastBattleResult;

    public static StarBase enteredBase;

    public static Player humanPlayer;
    public static Player scavengerPlayer;
    public static Player krigiaPlayer;
    public static Player wertuPlayer;
    public static Player zythPlayer;

    public static int krigiaReputation = 0;
    public static int wertuReputation = 0;
    public static int zythReputation = 0;

    public static int drones;
    public static int dronePrice;

    public static ulong seed;
    public static RandomNumberGenerator rng;

    public static IItem[] storage;

    public static int credits;

    public static int repairPrice;
    public static int fuelPrice;
    public static int exodusPrice;

    public static int experience;

    public static float fuel;

    public static int day;

    public static int travelSlowPoints;
    public static float travelSpeed;

    public static MapState mapState;

    public static GameLimits limits;

    public static HashSet<ArtifactDesign> artifactsRecovered;

    public static HashSet<string> technologiesResearched;
    public static double researchProgress;
    public static Research currentResearch;
    public static int krigiaMaterial;
    public static int wertuMaterial;
    public static int zythMaterial;
    public static int scienceFunds;
    public static bool metKrigia;
    public static bool metWertu;
    public static bool metZyth;
    
    public static HashSet<string> skillsLearned;

    public static HashSet<string> usedNames;
    public static HashSet<ResourcePlanet> planetsWithMines;
    public static HashSet<StarBase> humanBases;

    public static HashSet<SpaceUnit> spaceUnits;

    public static HashSet<RandomEvent> randomEventsAvailable;
    public static int randomEventCooldown;

    public static SpaceUnit humanUnit;

    // starSystems is a list of all star systems existing in this session.
    // starSystems[startingSystemID] is a starting star system.
    public static List<StarSystem> starSystems;

    public static Dictionary<StarSystem, List<StarSystem>> starSystemConnections;
    public static Dictionary<Vector2, StarSystem> starSystemByPos;

    public static int startingSystemID;

    public static int enemyBaseNumAttackers;
    public static SpaceUnit enemyAttackerUnit;
    public static StarBase garrisonStarBase;

    public static StarSystem StartingSystem() { return starSystems[startingSystemID]; }

    public static double ResearchRate() {
        double value = 0.5;
        if (scienceFunds >= 5) {
            var fundsScore = (Math.Log((double)scienceFunds / 3) / 100) * 5;
            value += QMath.ClampMax(fundsScore, 0.5);
        }
        if (currentResearch != null && currentResearch.material != Research.Material.None) {
            // Without needed material, research is performed at the 25% rate.
            if (currentResearch.material == Research.Material.Krigia && krigiaMaterial == 0) {
                value *= 0.25;
            } else if (currentResearch.material == Research.Material.Wertu && wertuMaterial == 0) {
                value *= 0.25;
            } else if (currentResearch.material == Research.Material.Zyth && zythMaterial == 0) {
                value *= 0.25;
            }
            if (RpgGameState.technologiesResearched.Contains("Alien Tech Lab")) {
                value = QMath.ClampMax(value + 0.1, 1);
            }
        }
        return value;
    }

    public static void AddFuel(float amount) {
        fuel = QMath.Clamp(fuel + amount, 0, MaxFuel());
    }

    public static float MaxFuel() {
        var value = limits.maxFuel;
        if (technologiesResearched.Contains("Improved Fuel Tanks III")) {
            value *= 1.2f;
        } else if (technologiesResearched.Contains("Improved Fuel Tanks II")) {
            value *= 1.1f;
        } else if (technologiesResearched.Contains("Improved Fuel Tanks")) {
            value *= 1.05f;
        }
        return value;
    }

    public static float RadarRange() {
        var value = 300f;
        if (technologiesResearched.Contains("Jump Tracer Mk3")) {
            value *= 1.25f;
        } else if (technologiesResearched.Contains("Jump Tracer Mk2")) {
            value *= 1.15f;
        }
        return value;
    }

    public static int DebrisSellPrice() { return 12; }
    public static int MineralsSellPrice() { return 14; }
    public static int OrganicSellPrice() { return 20; }
    public static int PowerSellPrice() { return 22; }

    public static void PutItemToStorage(IItem item) {
        for (int i = 0; i < 14; i++) {
            if (RpgGameState.storage[i] == null) {
                RpgGameState.storage[i] = item;
                return;
            }
        }
    }

    public static void Reset(ulong gameSeed) {
        day = 1;

        drones = 0;
        credits = 0;

        seed = gameSeed;

        artifactsRecovered = new HashSet<ArtifactDesign>{};

        randomEventCooldown = 10;
        randomEventsAvailable = new HashSet<RandomEvent>();
        foreach (var e in RandomEvent.list) {
            randomEventsAvailable.Add(e);
        }

        krigiaReputation = 0;
        wertuReputation = 0;
        zythReputation = 0;

        krigiaPlans = new KrigiaPlans();

        researchProgress = 0;
        currentResearch = null;
        technologiesResearched = new HashSet<string>{};
        krigiaMaterial = 0;
        wertuMaterial = 0;
        zythMaterial = 0;
        scienceFunds = 0;
        metKrigia = false;
        metWertu = false;
        metZyth = false;

        skillsLearned = new HashSet<string>{};

        humanBases = new HashSet<StarBase>{};

        spaceUnits = new HashSet<SpaceUnit>{};

        usedNames = new HashSet<string>{};
        planetsWithMines = new HashSet<ResourcePlanet>{};

        storage = new IItem[14];

        rng = new RandomNumberGenerator();
        rng.Seed = seed;

        mapState = new MapState();

        humanUnit = new SpaceUnit();

        starSystems = new List<StarSystem>{};
        starSystemConnections = new Dictionary<StarSystem, List<StarSystem>>{};
        starSystemByPos = new Dictionary<Vector2, StarSystem>{};

        credits = 1000;
    }
}