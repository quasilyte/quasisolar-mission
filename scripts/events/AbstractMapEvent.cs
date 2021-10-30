using System;
using System.Collections.Generic;

public abstract class AbstractMapEvent {
    public enum TriggerKind {
        OnSystemEntered,
        OnSystemPatroling,
        OnSpaceTravelling,
        OnScript,
    }

    public enum EffectKind {
        None,
        AddVesselMod,

        AddCredits,
        AddMinerals,
        AddOrganic,
        AddPower,
        AddFuel,

        KrigiaDetectsStarBase,
        AddDrone,
        AddFlagshipBackupEnergy,
        AddFleetBackupEnergyPercentage,
        AddVesselToFleet,
        AddTechnology,
        AddReputation,
        AddItem,
        DeclareWar,
        SpendAnyVesselBackupEnergy,
        ApplySlow,
        DamageFleetPercentage,
        DamageFlagshipPercentage,
        DestroyVessel,
        AddKrigiaMaterial,
        TeleportToSystem,
        EnterArena,
        EnterDuelArena,
        EnterTextQuest,
        PrepareArenaSettings,
        SpawnSpaceUnit,
        SpaceUnitRetreat,
    }

    public class Effect {
        public EffectKind kind;
        public object value;
        public object value2 = null; // When one value is not enough.
        public System.Action fn = () => {};
    }

    public class Result {
        public string text;
        public int expReward = 0;
        public bool skipText = false;
        public List<Effect> effects = new List<Effect>();
    }

    public class Action {
        public string name;
        public Func<string> hint = () => "";
        public Func<bool> condition = () => true;
        public Func<Result> apply = () => null;
    }

    // These fields should be initialized in constructor.
    public string title;
    public int luckScore;
    public TriggerKind triggerKind;

    // These fields should be initialized in Create method.
    public List<Action> actions = new List<Action>();
    public string text;

    public abstract AbstractMapEvent Create(RandomEventContext ctx);

    // Override Condition() to add event trigger condition constraints.
    public virtual bool Condition() { return true; }

    protected static bool EventHappened(string name) {
        return GameState().eventsResolved.Contains(name);
    }

    protected static Vessel Flagship() { return PlayerSpaceUnit().fleet[0].Get(); }
    protected static RpgGameState GameState() { return RpgGameState.instance; }
    protected static StarSystem CurrentStarSystem() { return RpgGameState.starSystemByPos[PlayerSpaceUnit().pos]; }
    protected static SpaceUnit PlayerSpaceUnit() { return GameState().humanUnit.Get(); }
    protected static string MultilineText(string s) { return Utils.FormatMultilineText(s); }

    protected static bool HasWeapon(WeaponDesign w) {
        foreach (var u in PlayerSpaceUnit().fleet) {
            foreach (var weaponName in u.Get().weapons) {
                if (w.name == weaponName) {
                    return true;
                }
            }
        }
        return false;
    }

    protected static bool AtPlayerSystem() {
        var starSystem = CurrentStarSystem();
        return starSystem.starBase.id != 0 && starSystem.starBase.Get().owner == Faction.Earthling;
    }

    protected static bool AtKrigiaSystem() {
        var starSystem = CurrentStarSystem();
        return starSystem.starBase.id != 0 && starSystem.starBase.Get().owner == Faction.Krigia;
    }

    protected static bool AtNeutralSystem() {
        return CurrentStarSystem().starBase.id == 0;
    }

    protected static bool IsFirstSystemVisit() {
        return CurrentStarSystem().visitsNum == 1;
    }

    protected static SpaceUnit NewSpaceUnit(Faction faction, params Vessel[] fleet) {
        return ArenaManager.NewSpaceUnit(faction, fleet);
    }
}
