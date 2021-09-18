using Godot;
using System;
using System.Collections.Generic;

public class RpgGameState {
    public static RpgGameState instance;

    public static RandomNumberGenerator rng;

    public static List<StarSystem> starSystemList;
    public static Dictionary<StarSystem, List<StarSystem>> starSystemConnections;
    public static Dictionary<Vector2, StarSystem> starSystemByPos;
    public static Dictionary<string, StarSystem> starSystemByName;
    public static HashSet<StarBase> humanBases;
    public static StarBase phaaBase;
    public static StarBase vespionBase;

    public static int enemyBaseNumAttackers = 0;
    public static SpaceUnit arenaUnit1;
    public static SpaceUnit arenaUnit2;
    public static StarBase garrisonStarBase = null;
    public static StarBase enteredBase = null;
    public static MapTransition transition = MapTransition.NewGame;
    public static BattleResult lastBattleResult = null;
    public static AbstractTQuest selectedTextQuest = null;

    public class Config {
        public ulong gameSeed;

        public RandomNumberGenerator rng;

        public ObjectPool<SpaceUnit> spaceUnits = new ObjectPool<SpaceUnit>();
        public ObjectPool<StarSystem> starSystems = new ObjectPool<StarSystem>();
        public ObjectPool<StarBase> starBases = new ObjectPool<StarBase>();
        public ObjectPool<Vessel> vessels = new ObjectPool<Vessel>();

        public int exodusPrice;

        public int missionDeadline;

        public float travelSpeed;

        public int startingFuel;
        public int startingCredits;
        public int randomEventCooldown;

        public long startingSystemID;

        public HashSet<string> randomEvents;

        public HashSet<string> usedNames = new HashSet<string>();

        public GameLimits limits;

        public SpaceUnit.Ref humanUnit;
    }

    public class GameLimits {
        public float maxFuel;
    }

    public enum MapTransition {
        NewGame,
        LoadGame,
        ExitStarBase,
        ExitResearchScreen,
        ExitQuestLogScreen,
        ExitTQuest,
        ExitSettings,
        UnitDestroyed,
        BaseAttackSimulation,
        EnemyUnitDestroyed,
        EnemyBaseAttackRepelled,
        EnemyUnitRetreats,
    }

    public class MapState {
        public bool movementEnabled = false;
        public UnitMode mode = UnitMode.Idle;
    }

    public class KrigiaPlans {
        public int taskForceDelay = 0;
    }

    public class PhaaPlans {
        public int transitionDelay = 5;
        public bool visited = false;
    }

    public class RarilouPlans {
        public int unitSpawnDelay = 0;
    }

    public RpgGameState() {}

    public void InitStaticState(bool newGame) {
        enemyBaseNumAttackers = 0;
        arenaUnit1 = null;
        arenaUnit2 = null;
        garrisonStarBase = null;
        enteredBase = null;
        transition = newGame ? MapTransition.NewGame : MapTransition.LoadGame;
        lastBattleResult = null;

        starSystemList = new List<StarSystem>();
        foreach (var sys in starSystems.objects.Values) {
            starSystemList.Add(sys);
        }

        humanBases = new HashSet<StarBase>();
        foreach (var sys in starSystemList) {
            if (sys.starBase.id == 0) {
                continue;
            }
            
            var starBase = starBases.Get(sys.starBase.id);
            if (starBase.owner == Faction.Earthling) {
                humanBases.Add(starBases.Get(sys.starBase.id));
                continue;
            }
            if (starBase.owner == Faction.Phaa) {
                phaaBase = starBases.Get(sys.starBase.id);
                continue;
            }
            if (starBase.owner == Faction.Vespion) {
                vespionBase = starBases.Get(sys.starBase.id);
                continue;
            }
        }

        starSystemByPos = new Dictionary<Vector2, StarSystem>();
        starSystemByName = new Dictionary<string, StarSystem>();
        foreach (var sys in starSystemList) {
            starSystemByPos[sys.pos] = sys;
            starSystemByName[sys.name] = sys;
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
        
        for (int i = 0; i < starSystemList.Count; i++) {
            var sys = starSystemList[i];
            for (int j = 0; j < starSystemList.Count; j++) {
                if (i == j) {
                    continue;
                }
                var other = starSystemList[j];
                if (sys.pos.DistanceTo(other.pos) > 600) {
                    continue;
                }
                addToGraph(sys, other);
                addToGraph(other, sys);
            }
        }

        foreach (var sys in starSystemList) {
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

        o.exodusPrice = c.exodusPrice;

        o.starSystems = c.starSystems;
        o.spaceUnits = c.spaceUnits;
        o.starBases = c.starBases;
        o.vessels = c.vessels;

        o.fuel = c.startingFuel;
        o.credits = c.startingCredits;

        o.usedNames = c.usedNames;

        o.humanUnit = c.humanUnit;

        o.limits = c.limits;

        o.startingSystemID = c.startingSystemID;

        o.travelSpeed = c.travelSpeed;

        o.randomEventCooldown = c.randomEventCooldown;
        o.randomEventsAvailable = c.randomEvents;

        o.missionDeadline = c.missionDeadline;

        foreach (var kv in o.reputations) {
            o.alienCurrency[kv.Key] = 0;
        }
        
        return o;
    }

    public bool FactionsAtWar(Faction x, Faction y) {
        // TODO: implement a real system

        if (x == Faction.Earthling && y == Faction.RandomEventAlly) {
            return false;
        }

        return x != y;
    }

    public bool rpgMode = false;

    public KrigiaPlans krigiaPlans = new KrigiaPlans();
    public PhaaPlans phaaPlans = new PhaaPlans();
    public RarilouPlans rarilouPlans = new RarilouPlans();

    public Dictionary<Faction, int> reputations = new Dictionary<Faction, int>{
        {Faction.Krigia, -25},
        {Faction.Wertu, 5},
        {Faction.Zyth, -10},
        {Faction.Vespion, 0},
        {Faction.Phaa, 0},
        {Faction.PhaaRebel, 0},
        {Faction.Draklid, -5},
        {Faction.Rarilou, 0},
    };

    public Dictionary<Faction, int> alienCurrency = new Dictionary<Faction, int>{};

    // TODO: move somewhere else?
    public static Dictionary<Faction, string> alienCurrencyNames = new Dictionary<Faction, string>{
        {Faction.Krigia, "?"},
        {Faction.Wertu, "Plasmoids"},
        {Faction.Zyth, "?"},
        {Faction.Vespion, "Perfect Combs"},
        {Faction.Phaa, "Pure Scales"},
        {Faction.Draklid, "?"},
        {Faction.Rarilou, "Rosy Crystals"},
    };

    public Dictionary<Faction, DiplomaticStatus> diplomaticStatuses = new Dictionary<Faction, DiplomaticStatus>{
        {Faction.Krigia, DiplomaticStatus.War},
        {Faction.Wertu, DiplomaticStatus.NonAttackPact},
        {Faction.Zyth, DiplomaticStatus.Unspecified},
        {Faction.Vespion, DiplomaticStatus.Unspecified},
        {Faction.Phaa, DiplomaticStatus.Unspecified},
        {Faction.Draklid, DiplomaticStatus.War},
        {Faction.Rarilou, DiplomaticStatus.Unspecified},
    };

    public List<string> explorationDrones = new List<string>{};

    public ulong seed;

    public ItemInfo[] storage = new ItemInfo[14];

    public int credits = 0;

    public int exodusPrice;

    public int experience = 0;

    public float fuel = 0;

    public int day = 1;
    public int missionDeadline;

    public int travelSlowPoints = 0;
    public float travelSpeed;

    public List<Quest.Data> activeQuests = new List<Quest.Data>();
    public HashSet<string> completedQuests = new HashSet<string>();
    public HashSet<string> issuedQuests = new HashSet<string>();

    public MapState mapState = new MapState();

    public GameLimits limits;

    public HashSet<string> artifactsRecovered = new HashSet<string>{};

    public HashSet<string> technologiesResearched = new HashSet<string>{};
    public double researchProgress = 0;
    public string currentResearch = "";
    
    public DebrisContainer researchMaterial = new DebrisContainer();

    public int scienceFunds = 0;
    
    public HashSet<string> usedNames;

    public ObjectPool<SpaceUnit> spaceUnits;

    public ObjectPool<Vessel> vessels;

    // starSystems is a list of all star systems existing in this session.
    // starSystems[startingSystemID] is a starting star system.
    public ObjectPool<StarSystem> starSystems;

    public ObjectPool<StarBase> starBases;

    public HashSet<string> randomEventsAvailable;
    public HashSet<string> eventsResolved = new HashSet<string>();
    public int randomEventCooldown;

    public SpaceUnit.Ref humanUnit;

    public long startingSystemID;

    public static StarSystem StartingSystem() {
        return instance.starSystems.objects[instance.startingSystemID];
    }

    public void CollectGarbage() {
        instance.spaceUnits.RemoveInactive();
        instance.vessels.RemoveInactive();
        instance.starBases.RemoveInactive();
    }

    public static double ResearchRate() {
        double value = 0.5;
        if (instance.scienceFunds >= 5) {
            var fundsScore = (Math.Log((double)instance.scienceFunds / 3) / 100) * 5;
            value += QMath.ClampMax(fundsScore, 0.5);
        }
        if (instance.currentResearch != "") {
            var research = Research.Find(instance.currentResearch);
            if (research.material != Faction.Neutral) {
                // Without needed material, research is performed at the 25% rate.
                if (instance.researchMaterial.Count(research.material) == 0) {
                    value *= 0.25;
                }
                if (instance.technologiesResearched.Contains("Alien Tech Lab")) {
                    value = QMath.ClampMax(value + 0.1, 1);
                }
            }
        }
        return value;
    }

    public static int MaxExplorationDrones() {
        return 3;
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

    public Vessel NewVessel(Faction faction, VesselDesign design) {
        var v = vessels.New();
        v.isBot = true;
        v.faction = faction;
        v.designName = design.name;
        return v;
    }

    public int StorageFleeSlotsNum() {
        int num = 0;
        for (int i = 0; i < 14; i++) {
            if (instance.storage[i] == null) {
                num++;
            }
        }
        return num;
    }

    public int StorageFreeSlot() {
        for (int i = 0; i < 14; i++) {
            if (instance.storage[i] == null) {
                return i;
            }
        }
        return -1;
    }

    public void PutItemToStorage(IItem item, int index) {
        instance.storage[index] = ItemInfo.Of(item);
    }

    public void PutItemToStorage(IItem item) {
        int i = StorageFreeSlot();
        if (i != -1) {
            PutItemToStorage(item, i);
        }
    }
}
