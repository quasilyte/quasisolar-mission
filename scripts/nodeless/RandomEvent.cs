using Godot;
using System;
using System.Collections.Generic;

// TODO:
// - duel event; when you have 1 vessel

/*
Possible negative effects:
- race relation points loss
- vessel damage
- resources loss (mineral / organic / power)
- credits loss
- fuel loss
- energy loss
- drone loss
- battle

Possible positive effects:
- race relation points gain
- vessel repair
- resources gain (mineral / organic / power)
- credits gain
- fuel gain
- energy gain
- drone gain
- new ally
- additional experience
- reveal some unvisited system info
- get artifact
- get technology
- some useful info
- get a quest

Possible neutral effects:
- teleportation
*/

public class RandomEvent {
    public enum TriggerKind {
        OnSystemEntered,
        OnSystemPatroling,
        OnSpaceTravelling,
    }

    public enum EffectKind {
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
        DeclareWar,
        SpendAnyVesselBackupEnergy,
        ApplySlow,
        DamageFleetPercentage,
        DamageFlagshipPercentage,
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
        public bool skipText = false;
        public List<Effect> effects = new List<Effect>();
    }

    public class Action {
        public string name;
        public Func<string> hint = () => "";
        public Func<bool> condition = () => true;
        public Func<RandomEventContext, Result> apply = (RandomEventContext _) => null;
    }

    public string title;
    public int expReward = 0;
    public int luckScore = 0;
    public TriggerKind trigger;
    public string text;
    public List<Action> actions = new List<Action>();

    public Func<bool> condition = () => true;
    public Func<RandomEventContext, string> extraText = (RandomEventContext _) => "";

    private static string multilineText(params string[] lines) {
        return string.Join("\n", lines);
    }

    private static bool AtStartingSystem() {
        return RpgGameState.instance.humanUnit.Get().pos == RpgGameState.StartingSystem().pos;
    }

    private static bool AtFriendlySystem() {
        var starSystem = RpgGameState.starSystemByPos[RpgGameState.instance.humanUnit.Get().pos];
        return starSystem.starBase.id != 0 && starSystem.starBase.Get().owner == Faction.Earthling;
    }

    private static bool AtKrigiaSystem() {
        var starSystem = RpgGameState.starSystemByPos[RpgGameState.instance.humanUnit.Get().pos];
        return starSystem.starBase.id != 0 && starSystem.starBase.Get().owner == Faction.Krigia;
    }

    private static bool SystemHasStarBase() {
        return RpgGameState.starSystemByPos[RpgGameState.instance.humanUnit.Get().pos].starBase.id != 0;
    }

    private static bool IsFirstSystemVisit() {
        return RpgGameState.starSystemByPos[RpgGameState.instance.humanUnit.Get().pos].visitsNum == 1;
    }

    private static bool HasSpeaking() {
        return RpgGameState.instance.skillsLearned.Contains("Speaking");
    }

    private static SpaceUnit NewSpaceUnit(Faction faction, params Vessel[] fleet) {
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

    private static RandomEvent newBrokenRadar() {
        Func<StarSystem> knownWertuSystem = () => {
            foreach (var sys in RpgGameState.instance.starSystems.objects.Values) {
                if (sys.intel == null) {
                    continue;
                }
                if (sys.starBase.id != 0 && sys.starBase.Get().owner == Faction.Wertu) {
                    return sys;
                }
            }
            return null;
        };
        Func<StarSystem> knownKrigiaSystem = () => {
            foreach (var sys in RpgGameState.instance.starSystems.objects.Values) {
                if (sys.intel == null) {
                    continue;
                }
                if (sys.starBase.id != 0 && sys.starBase.Get().owner == Faction.Krigia) {
                    return sys;
                }
            }
            return null;
        };

        var e = new RandomEvent();
        e.title = "Broken Radar";
        e.expReward = 2;
        e.luckScore = 6;
        e.condition = () => RpgGameState.instance.day > 350 && !SystemHasStarBase();
        e.text = multilineText(
            "A single Wertu Guardian class vessel contacts you.",
            "",
            "Their radar is not working properly after the battle, so they can't calculate the proper jump coordinates.",
            "",
            "If you happen to know any Wertu system location, that could help them out."
        );
        e.actions.Add(new Action{
            name = "Transfer coordinates",
            hint = () => {
                var sys = knownWertuSystem();
                return sys == null ? "" : $"({sys.name})";
            },
            condition = () => knownWertuSystem() != null,
            apply = (RandomEventContext _) => {
                var sys = knownWertuSystem();
                var unit = NewSpaceUnit(Faction.RandomEventHostile, VesselFactory.NewVessel(Faction.Wertu, "Guardian"));
                unit.botProgram = SpaceUnit.Program.BackToTheBase;
                unit.waypoint = sys.pos;
                return new Result{
                    text = $"You directed this unit to the {sys.name} Wertu system.",
                    effects = {
                        new Effect{
                            kind = EffectKind.AddReputation,
                            value = 2,
                            value2 = Faction.Wertu,
                        },
                        new Effect{
                            kind = EffectKind.SpawnSpaceUnit,
                            value = unit,
                        },
                    },
                };
            }
        });
        e.actions.Add(new Action{
            name = "Lead them to Krigia",
            hint = () => {
                var sys = knownKrigiaSystem();
                return sys == null ? "" : $"({sys.name})";
            },
            condition = () => knownKrigiaSystem() != null,
            apply = (RandomEventContext _) => {
                var sys = knownKrigiaSystem();
                var unit = NewSpaceUnit(Faction.Wertu, VesselFactory.NewVessel(Faction.Wertu, "Guardian"));
                unit.botProgram = SpaceUnit.Program.AttackStarBase;
                unit.waypoint = sys.pos;
                return new Result{
                    text = $"You directed this unit to the {sys.name} Krigia system. Cruel!",
                    effects = {
                        new Effect{
                            kind = EffectKind.SpawnSpaceUnit,
                            value = unit,
                        },
                    },
                };
            }
        });
        e.actions.Add(new Action{
            name = "Attack the Guardian",
            apply = (RandomEventContext _) => {
                var unit = NewSpaceUnit(Faction.Wertu, VesselFactory.NewVessel(Faction.Wertu, "Guardian"));
                return new Result{
                    text = "You consider Wertu to be your enemy. This Guardian luck definitely ran out.",
                    effects = {
                        new Effect{
                            kind = EffectKind.DeclareWar,
                            value = Faction.Wertu,
                        },
                        new Effect{
                            kind = EffectKind.EnterArena,
                            value = unit,
                        },
                    },
                };
            }
        });
        e.actions.Add(new Action{
            name = "Fly away",
            apply = (RandomEventContext _) => {
                return new Result{
                    text = "You decided to leave that Wertu behind.",
                };
            }
        });
        return e;
    }

    // private static RandomEvent newZythOffer() {
    //     var e = new RandomEvent { };
    //     e.title = "Zyth Offer";
    //     e.expReward = 5;
    //     e.luckScore = 7;
    //     e.trigger = TriggerKind.OnSystemEntered;
    //     e.condition = () => RpgGameState.instance.day >= 300;
    //     e.text = multilineText(
    //         "Zyth captain contacts your flagship with some unusual offer.",
    //         "",
    //         "If you join their battle against two Krigia vessels, you'll get 5000 resource units.",
    //         "",
    //         "Note that you'll get that reward only if Zyth vessel survives the battle."
    //     );
    //     e.actions.Add(new Action{
    //         name = "Accept the offer",
    //     });
    //     e.actions.Add(new Action{
    //         name = "Attack the Zyth vessel",
    //     });
    //     e.actions.Add(new Action{
    //         name = "",
    //     });
    // }

    private static RandomEvent newTheAvenger() {
        var e = new RandomEvent { };
        e.title = "The Avenger";
        e.expReward = 8;
        e.luckScore = 9;
        e.trigger = TriggerKind.OnSystemEntered;
        e.condition = () => {
            if (RpgGameState.instance.day <= 300) {
                return false;
            }
            return RpgGameState.instance.humanUnit.Get().fleet.Count < SpaceUnit.maxFleetSize;
        };
        e.text = multilineText(
            "A single mean-looking vessel is about to engage a squad of pirates.",
            "",
            "Will you participate or will you see how the events unfold?"
        );
        e.actions.Add(new Action{
            name = "Attack pirates too",
            apply = (RandomEventContext ctx) => {
                var v1 = VesselFactory.NewVessel(Faction.Pirate, "Pirate");
                var v2 = VesselFactory.NewVessel(Faction.Pirate, "Pirate");
                var v3 = VesselFactory.NewVessel(Faction.Pirate, "Pirate");
                var v4 = VesselFactory.NewVessel(Faction.Pirate, "Pirate");
                var spaceUnit = NewSpaceUnit(Faction.RandomEventHostile, v1, v2, v3, v4);
                if (RpgGameState.instance.skillsLearned.Contains("Luck")) {
                    spaceUnit.cargo.minerals = (int)(ctx.roll * 60);
                }

                var avenger = RpgGameState.instance.NewVessel(Faction.Earthling, VesselDesign.Find("Avenger"));
                avenger.pilotName = PilotNames.UniqHumanName(RpgGameState.instance.usedNames);
                VesselFactory.Init(avenger, "Neutral Weak Avenger");

                return new Result {
                    text = multilineText(
                        $"The Avenger vessel captain, {avenger.pilotName}, promises to join your group after you deal with pirates.",
                        "",
                        "Side by side, you start the attack."
                    ),
                    effects = {
                        new Effect{
                            kind = EffectKind.AddVesselToFleet,
                            value = avenger,
                        },
                        new Effect{
                            kind = EffectKind.EnterArena,
                            value = spaceUnit,
                        },
                    },
                };
            }
        });
        e.actions.Add(new Action{
            name = "Attack the avenger",
            apply = (RandomEventContext ctx) => {
                var avenger = VesselFactory.NewVessel(Faction.Neutral, "Avenger");
                var spaceUnit = NewSpaceUnit(Faction.RandomEventHostile, avenger);
                return new Result {
                    text = multilineText(
                        "You intervention surprised the pirate hunter.",
                        "",
                        "The pirates used the chance and warped away."
                    ),
                    effects = {
                        new Effect{
                            kind = EffectKind.EnterArena,
                            value = spaceUnit,
                        },
                    },
                };
            }
        });
        e.actions.Add(new Action{
            name = "See what happens next",
            apply = (RandomEventContext _) => {
                return new Result{
                    text = multilineText(
                        "After almost an hour pirates manage to destroy the attacker.",
                        "Soon after that, they jump out of the system in a hurry.",
                        "",
                        "You'll probably never learn what this whole situation was about."
                    ),
                };
            }
        });
        return e;
    }

    private static RandomEvent newPiratesAttack() {
        Func<SpaceUnit> createPirates = () => {
            var v1 = VesselFactory.NewVessel(Faction.Pirate, "Pirate");
            var v2 = VesselFactory.NewVessel(Faction.Pirate, "Pirate");
            return NewSpaceUnit(Faction.RandomEventHostile, v1, v2);
        };

        var e = new RandomEvent { };
        e.title = "Pirates Attack";
        e.expReward = 3;
        e.luckScore = 2;
        e.trigger = TriggerKind.OnSystemEntered;
        e.text = multilineText(
            "Your badly-equipped fleet attracted a pirates group.",
            "They demand that you pay in credits or resources.",
            "",
            "Another bad news is: out warp engine is not ready for another jump just yet.",
            "We either pay, or fight our way out of it."
        );
        e.condition = () => {
            return RpgGameState.instance.day > 250 &&
                RpgGameState.instance.humanUnit.Get().FleetCost() < 17500 &&
                !AtFriendlySystem();
        };
        e.actions.Add(new Action{
            name = "Give all resources",
            apply = (RandomEventContext _) => {
                var resources = RpgGameState.instance.humanUnit.Get().cargo.minerals + 
                    RpgGameState.instance.humanUnit.Get().cargo.organic +
                    RpgGameState.instance.humanUnit.Get().cargo.power;
                if (resources > 25) {
                    RpgGameState.instance.humanUnit.Get().cargo.organic = 0;
                    RpgGameState.instance.humanUnit.Get().cargo.power = 0;
                    return new Result{
                        text = multilineText(
                            "Pirates look happy with their payment.",
                            "They fly avay after firing a few laser shots directed towards your fleet."
                        ),
                        effects = {
                            new Effect{
                                kind = EffectKind.AddMinerals,
                                value = -RpgGameState.instance.humanUnit.Get().cargo.minerals,
                            },
                            new Effect{
                                kind = EffectKind.AddOrganic,
                                value = -RpgGameState.instance.humanUnit.Get().cargo.organic,
                            },
                            new Effect{
                                kind = EffectKind.AddOrganic,
                                value = -RpgGameState.instance.humanUnit.Get().cargo.power,
                            },
                        },
                    };
                }
                return new Result{
                    text = multilineText(
                        "Pirates are not impressed by your proposal.",
                        "As they say `We'll take your vessel apart`, you start remembering that your cargo is somewhat too empty."
                    ),
                    effects = {
                        new Effect{
                            kind = EffectKind.EnterArena,
                            value = createPirates(),
                        },
                    },
                };
            },
        });
        e.actions.Add(new Action{
            name = "Pay 2000 credits",
            condition = () => RpgGameState.instance.credits >= 2000,
            apply = (RandomEventContext _) => {
                return new Result{
                    text = multilineText(
                        "Pirates look happy with their payment.",
                        "They fly avay after firing a few laser shots directed towards your fleet."
                    ),
                    effects = {
                        new Effect{
                            kind = EffectKind.AddCredits,
                            value = -2000,
                        },
                    },
                };
            },
        });
        e.actions.Add(new Action{
            name = "Attack them",
            apply = (RandomEventContext _) => {
                return new Result{
                    text = multilineText(
                        "You decided not to pay the toll.",
                        "Unsurprisingly, it made pirates angry.",
                        "If I were you, I would start charging the weapons..."
                    ),
                    effects = {
                        new Effect{
                            kind = EffectKind.EnterArena,
                            value = createPirates(),
                        },
                    },
                };
            }
        });
        return e;
    }

    public static RandomEvent rarilouEncounter = newRarilouEncounter();

    public static RandomEvent[] list;
    public static List<RandomEvent> onSystemEnteredList;
    public static List<RandomEvent> onSystemPatrolingList;
    public static List<RandomEvent> onSpaceTravellingList;
    public static Dictionary<string, RandomEvent> eventByTitle;

    public static void InitLists() {
        list = new RandomEvent[]{
            newHomeworldPortal(),
            newUnknownPortal(),
            newBatteryMalfunction(),
            newNebulaStorm(),
            newAsteroids(),
            newAbandonedVessel(),
            newPiratesAttack(),
            newTroubledLiner(),
            newSpaceNomads(),
            newTheAvenger(),
            newFuelTrader(),
            newLoneKrigiaScout(),
            newSkirmish(),
            newEarthlingScout(),
            newDevastatedHomeworld(),
            newGamblingExperiment(),
            newHijack(),
            newKrigiaDrone(),
            newEnigma(),
            newBrokenRadar(),

            newPurpleSystemVisitor(),
        };
        onSystemEnteredList = new List<RandomEvent>();
        onSystemPatrolingList = new List<RandomEvent>();
        onSpaceTravellingList = new List<RandomEvent>();
        eventByTitle = new Dictionary<string, RandomEvent>();
        foreach (var e in list) {
            eventByTitle.Add(e.title, e);
            if (e.trigger == TriggerKind.OnSystemEntered) {
                onSystemEnteredList.Add(e);
            } else if (e.trigger == TriggerKind.OnSystemPatroling) {
                onSystemPatrolingList.Add(e);
            } else if (e.trigger == TriggerKind.OnSpaceTravelling) {
                onSpaceTravellingList.Add(e);
            }
        }
    }

    private static StarSystem PeekNearbySystem(float roll) {
        // TODO: use systems connection graph.

        var nearbySystems = new List<StarSystem>();
        foreach (var sys in RpgGameState.instance.starSystems.objects.Values) {
            if (sys.pos == RpgGameState.instance.humanUnit.Get().pos) {
                continue;
            }
            if (sys.pos.DistanceTo(RpgGameState.instance.humanUnit.Get().pos) < 400) {
                nearbySystems.Add(sys);
            }
        }
        if (nearbySystems.Count == 0) {
            return null;
        }
        var index = (int)(roll * 100000) % nearbySystems.Count;
        return nearbySystems[index];
    }
}
