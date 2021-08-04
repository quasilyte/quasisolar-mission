using System;
using System.Collections.Generic;

public abstract class AbstractMapEvent {
    public enum TriggerKind {
        OnSystemEntered,
        OnSystemPatroling,
        OnSpaceTravelling,
    }

    public enum EffectKind {
        AddVesselStatus,
        AddCredits,
        AddMinerals,
        AddOrganic,
        AddPower,
        AddFuel,
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
        EnterTextQuest,
        PrepareArenaSettings,
        SpawnSpaceUnit,
    }

    public class Effect {
        public EffectKind kind;
        public object value;
        public object value2 = null; // When one value is not enough.
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
        return !GameState().randomEventsAvailable.Contains(name);
    }

    protected static Vessel Flagship() { return PlayerSpaceUnit().fleet[0].Get(); }
    protected static RpgGameState GameState() { return RpgGameState.instance; }
    protected static SpaceUnit PlayerSpaceUnit() { return GameState().humanUnit.Get(); }
    protected static string MultilineText(string s) { return Utils.FormatMultilineText(s); }

    protected static bool HasLuckSkill() { return GameState().skillsLearned.Contains("Luck"); }
    protected static bool HasSalvagingSkill() { return GameState().skillsLearned.Contains("Salvaging"); }
    protected static bool HasSpeakingSkill() { return GameState().skillsLearned.Contains("Speaking"); }

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
        var starSystem = RpgGameState.starSystemByPos[RpgGameState.instance.humanUnit.Get().pos];
        return starSystem.starBase.id != 0 && starSystem.starBase.Get().owner == Faction.Earthling;
    }

    protected static bool AtKrigiaSystem() {
        var starSystem = RpgGameState.starSystemByPos[PlayerSpaceUnit().pos];
        return starSystem.starBase.id != 0 && starSystem.starBase.Get().owner == Faction.Krigia;
    }

    protected static bool AtNeutralSystem() {
        return RpgGameState.starSystemByPos[PlayerSpaceUnit().pos].starBase.id == 0;
    }

    protected static bool IsFirstSystemVisit() {
        return RpgGameState.starSystemByPos[PlayerSpaceUnit().pos].visitsNum == 1;
    }

    protected static SpaceUnit NewSpaceUnit(Faction faction, params Vessel[] fleet) {
        var fleetList = new List<Vessel.Ref>();
        foreach (var v in fleet) {
            fleetList.Add(v.GetRef());
        }

        var spaceUnit = RpgGameState.instance.spaceUnits.New();
        spaceUnit.owner = faction;
        spaceUnit.pos = RpgGameState.instance.humanUnit.Get().pos;
        spaceUnit.fleet = fleetList;
        return spaceUnit;
    }
}
