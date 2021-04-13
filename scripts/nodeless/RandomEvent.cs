using Godot;
using System;
using System.Collections.Generic;

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
        AddFlagshipBackupEnergy,
        AddFleetBackupEnergyPercentage,
        AddWertuReputation,
        SpendAnyVesselBackupEnergy,
        ApplySlow,
        DamageFleetPercentage,
        TeleportToSystem,
        EnterArena,
    }

    public class Effect {
        public EffectKind kind;
        public object value;
    }

    public class Result {
        public string text;
        public List<Effect> effects = new List<Effect>();
    }

    public class Action {
        public string name;
        public Func<bool> condition = () => true;
        public Func<RandomEventContext, Result> apply = (RandomEventContext _) => null;
    }

    public string title;
    public int expReward;
    public int luckScore;
    public TriggerKind trigger;
    public string text;
    public List<Action> actions = new List<Action>();

    public Func<bool> condition = () => true;
    public Func<RandomEventContext, string> extraText = (RandomEventContext _) => "";

    private static string multilineText(params string[] lines) {
        return string.Join("\n", lines);
    }

    private static RandomEvent newTroubledLiner() {
        Func<float, string, bool, Result> rewardForHelping = (float roll, string headline, bool spendEnergy) => {
            var text = headline + "\n\n";
            var effects = new List<Effect>();
            if (roll < 0.4) {
                effects.Add(new Effect{
                    kind = EffectKind.AddOrganic,
                    value = 50,
                });
                text += "In reward, they shared some of their transported goods worth 50 units of organic.";
            } else if (roll < 0.8) {
                effects.Add(new Effect{
                    kind = EffectKind.AddCredits,
                    value = 900,
                });
                text += "In reward, they paid 900 credits for your assistance.";
            } else {
                effects.Add(new Effect{
                    kind = EffectKind.AddWertuReputation,
                    value = 5,
                });
                text += multilineText(
                    "The transport crew gives you the infinite gratitude, but they don't have anything valuable to share.",
                    "",
                    "The vessel captain promised to spread a word about you. This may affect your Wertu repuration in a positive fashion."
                );
            }
            if (spendEnergy) {
                effects.Add(new Effect{
                    kind = EffectKind.SpendAnyVesselBackupEnergy,
                    value = 40,
                });
            }
            return new Result{
                text = text,
                effects = effects,
            };
        };

        var e = new RandomEvent { };
        e.title = "Troubled Liner";
        e.text = multilineText(
            "You received a message from a vessel.",
            "",
            "`This is a non-military Wertu liner from Zeta Draconis. Our batteries are exhausted and we can't charge the warp engine to leave this system. Please help.`"
        );
        e.extraText = (RandomEventContext ctx) => {
            if (RpgGameState.technologiesResearched.Contains("Long-range Scanners") || RpgGameState.technologiesResearched.Contains("Long-range Scanners II")) {
                if (ctx.roll < 0.4) {
                    return "(Long-range Scanners) Vessel cargo contains valuable resources.";
                }
                return "(Long-range Scanners) Vessel cargo is empty.";
            }
            return "";
        };
        e.expReward = 6;
        e.luckScore = 8;
        e.trigger = TriggerKind.OnSystemEntered;
        e.actions.Add(new Action{
            name = "Transfer the energy",
            condition = () => {
                foreach (var v in RpgGameState.humanUnit.fleet) {
                    if (v.energy > 40) {
                        return true;
                    }
                }
                return false;
            },
            apply = (RandomEventContext ctx) => {
                return rewardForHelping(ctx.roll, "You transfered 40 units of energy to them.", true);
            },
        });
        e.actions.Add(new Action{
            name = "Attack the transport",
            apply = (RandomEventContext ctx) => {
                var liner = new Vessel {
                    isBot = true,
                    player = RpgGameState.wertuPlayer,
                    pilotName = "FIXME",
                };
                VesselFactory.Init(liner, VesselDesign.Find("Wertu", "Transporter"));
                var spaceUnit = new SpaceUnit {
                    owner = RpgGameState.wertuPlayer,
                    pos = RpgGameState.humanUnit.pos,
                    fleet = new List<Vessel>{liner},
                };
                if (ctx.roll < 0.4) {
                    spaceUnit.cargo.organic = (int)((ctx.roll + 0.6f) * 150);
                }
                return new Result{
                    text = multilineText(
                        "Pirates or scavengers may get to them at some point, so why not take the scraps yourself?",
                        "",
                        "Undoubtedly, this will affect your Wertu reputation."
                    ),
                    effects = {
                        new Effect{
                            kind = EffectKind.EnterArena,
                            value = spaceUnit,
                        },
                        new Effect{
                            kind = EffectKind.AddWertuReputation,
                            value = -2,
                        },
                    },
                };
            },
        });
        e.actions.Add(new Action{
            name = "Help to repair their reactor",
            condition = () => RpgGameState.skillsLearned.Contains("Repair I") || RpgGameState.skillsLearned.Contains("Repair II"),
            apply = (RandomEventContext ctx) => {
                return rewardForHelping(ctx.roll, "You board the vessel and fix their reactor.", false);
            }
        });
        e.actions.Add(new Action{
            name = "Ignore the call",
            apply = (RandomEventContext _) => {
                return new Result{
                    text = "You can't help everyone; more over, you have a mission to accomplish!",
                };
            }
        });
        return e;
    }

    private static RandomEvent newPiratesAttack() {
        Func<SpaceUnit> createPirates = () => {
            var v1 = new Vessel {
                isBot = true,
                player = RpgGameState.wertuPlayer, // FIXME
                pilotName = "FIXME",
            };
            VesselFactory.Init(v1, VesselDesign.Find("Neutral", "Pirate"));
            var v2 = new Vessel {
                isBot = true,
                player = RpgGameState.wertuPlayer, // FIXME
                pilotName = "FIXME",
            };
            VesselFactory.Init(v2, VesselDesign.Find("Neutral", "Pirate"));
            return new SpaceUnit {
                owner = RpgGameState.wertuPlayer, // FIXME
                pos = RpgGameState.humanUnit.pos,
                fleet = new List<Vessel>{v1, v2},
            };
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
            return RpgGameState.day > 150 && RpgGameState.humanUnit.FleetCost() < 17500;
        };
        e.actions.Add(new Action{
            name = "Give all resources",
            apply = (RandomEventContext _) => {
                var resources = RpgGameState.humanUnit.cargo.minerals + 
                    RpgGameState.humanUnit.cargo.organic +
                    RpgGameState.humanUnit.cargo.power;
                if (resources > 25) {
                    RpgGameState.humanUnit.cargo.organic = 0;
                    RpgGameState.humanUnit.cargo.power = 0;
                    return new Result{
                        text = multilineText(
                            "Pirates look happy with their payment.",
                            "They fly avay after firing a few laser shots directed towards your fleet."
                        ),
                        effects = {
                            new Effect{
                                kind = EffectKind.AddCredits,
                                value = -RpgGameState.humanUnit.cargo.minerals,
                            },
                            new Effect{
                                kind = EffectKind.AddOrganic,
                                value = -RpgGameState.humanUnit.cargo.organic,
                            },
                            new Effect{
                                kind = EffectKind.AddOrganic,
                                value = -RpgGameState.humanUnit.cargo.power,
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
            condition = () => RpgGameState.credits >= 2000,
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

    private static RandomEvent newAbandonedVessel() {
        var e = new RandomEvent { };
        e.title = "Abandoned Vessel";
        e.expReward = 9;
        e.luckScore = 7;
        e.trigger = TriggerKind.OnSystemEntered;
        e.text = multilineText(
            "You detect a small vessel that free floats the space around the planet. Scanning shows no signs of life on board.",
            "",
            "Should we fly to it and scavenge what we can?"
        );
        e.actions.Add(new Action {
            name = "Collect resources",
            apply = (RandomEventContext ctx) => {
                if (ctx.roll < 0.55) {
                    var credits = (int)(ctx.roll * 750) + 10;
                    var fuel = (int)((1 - ctx.roll) * 100) + 1;
                    return new Result {
                        text = multilineText(
                            "The vessel was pretty much empty, but you managed to get some scraps out.",
                            "",
                            $"Received {credits} credits.",
                            $"Received {fuel} fuel."
                        ),
                        effects = {
                            new Effect{
                                kind = EffectKind.AddCredits,
                                value = credits,
                            },
                            new Effect{
                                kind = EffectKind.AddFuel,
                                value = fuel,
                            },
                        },
                    };
                }
                var v = new Vessel {
                    isBot = true,
                    player = RpgGameState.zythPlayer,
                    pilotName = "FIXME",
                };
                VesselFactory.Init(v, VesselDesign.Find("Zyth", "Hunter"));
                return new Result {
                    text = multilineText(
                        "The abandoned vessel was a Zyth trick!",
                        "",
                        "We're ambushed, prepare for battle!"
                    ),
                    effects = {
                        new Effect{
                            kind = EffectKind.EnterArena,
                            value = new List<Vessel>{v},
                        },
                    },
                };
            },
        });
        e.actions.Add(new Action {
            name = "Carry on",
            apply = (RandomEventContext _) => {
                return new Result {
                    text = "It doesn't look good, we should probably fly by.",
                };
            },
        });
        return e;
    }

    private static RandomEvent newHomeworldPortal() {
        var e = new RandomEvent { };
        e.title = "Homeworld Portal";
        e.expReward = 4;
        e.luckScore = 6;
        e.trigger = TriggerKind.OnSystemEntered;
        e.text = multilineText(
            "You see a very familiar rift that is believed to lead right into your homeworld space, the Quasisolar system.",
            "",
            "It should be a relatively safe and fast transition."
        );
        e.condition = () => RpgGameState.humanUnit.pos != RpgGameState.StartingSystem().pos;
        e.actions.Add(new Action {
            name = "Enter the portal",
            apply = (RandomEventContext _) => {
                return new Result {
                    text = multilineText(
                        "As expected, the portal led to the Quasisolar system.",
                        "",
                        "Welcome home."
                    ),
                    effects = {
                        new Effect{
                            kind = EffectKind.TeleportToSystem,
                            value = RpgGameState.StartingSystem(),
                        },
                    },
                };
            },
        });
        e.actions.Add(new Action {
            name = "Ignore it",
            apply = (RandomEventContext _) => {
                return new Result {
                    text = "You decided to leave the portal alone.",
                };
            },
        });
        return e;
    }

    private static RandomEvent newUnknownPortal() {
        var e = new RandomEvent { };
        e.title = "Unknown Portal";
        e.expReward = 7;
        e.luckScore = 5;
        e.trigger = TriggerKind.OnSystemEntered;
        e.text = multilineText(
            "A strange, unidentified portal appears on your radar.",
            "Most likely, it's connected to another star system.",
            "",
            "Would you try to enter that portal?"
        );
        e.extraText = (RandomEventContext ctx) => {
            if (RpgGameState.skillsLearned.Contains("Navigation II")) {
                var sys = PeekNearbySystem(ctx.roll);
                return $"(Navigation II) This portal will lead you to the {sys.name} system.";
            }
            return "";
        };
        e.actions.Add(new Action {
            name = "Enter the portal",
            apply = (RandomEventContext ctx) => {
                var sys = PeekNearbySystem(ctx.roll);
                return new Result {
                    text = multilineText(
                        $"The portal brought you to the {sys.name} system."
                    ),
                    effects = {
                        new Effect{
                            kind = EffectKind.TeleportToSystem,
                            value = sys,
                        },
                    },
                };
            },
        });
        e.actions.Add(new Action {
            name = "Ignore it",
            apply = (RandomEventContext _) => {
                return new Result {
                    text = multilineText(
                        "You decided to leave the portal alone."
                    ),
                };
            },
        });
        return e;
    }

    private static RandomEvent newBatteryMalfunction() {
        var e = new RandomEvent { };
        e.title = "Battery Malfunction";
        e.expReward = 3;
        e.luckScore = 4;
        e.trigger = TriggerKind.OnSpaceTravelling;
        e.text = multilineText(
            "Flagship battery started to overheat.",
            "At this rate, it could lead to a total meltdown.",
            "",
            "You can try doing a quick repair, but it would require some resources."
        );
        e.condition = () => RpgGameState.humanUnit.fleet[0].energySource != EnergySource.Find("None");
        e.actions.Add(new Action {
            name = "Switch to the backup energy",
            apply = (RandomEventContext _) => {
                return new Result {
                    text = "You managed to save the battery at the cost of the 20 backup energy units.",
                    effects = {
                        new Effect{
                            kind = EffectKind.AddFlagshipBackupEnergy,
                            value = -20f,
                        },
                    },
                };
            },
        });
        e.actions.Add(new Action {
            name = "Switch to power resource supply",
            condition = () => RpgGameState.humanUnit.cargo.power >= 3,
            apply = (RandomEventContext _) => {
                return new Result {
                    text = multilineText(
                        "You let the battery cool down by switching to a different source of power.",
                        "That trick took 3 power units from your cargo."
                    ),
                    effects = {
                        new Effect{
                            kind = EffectKind.AddPower,
                            value = -3,
                        },
                    },
                };
            },
        });
        e.actions.Add(new Action {
            name = "Repair the battery",
            condition = () => {
                if (RpgGameState.skillsLearned.Contains("Repair II")) {
                    return RpgGameState.humanUnit.cargo.minerals >= 1;
                } else if (RpgGameState.skillsLearned.Contains("Repair I")) {
                    return RpgGameState.humanUnit.cargo.minerals >= 5;
                }
                return RpgGameState.humanUnit.cargo.minerals >= 10;
            },
            apply = (RandomEventContext _) => {
                if (RpgGameState.skillsLearned.Contains("Repair II")) {
                    return new Result {
                        text = "With your advanced skills, it only took 1 mineral unit to make it work as a new.",
                        effects = {
                            new Effect{
                                kind = EffectKind.AddMinerals,
                                value = -1,
                            },
                        },
                    };
                } else if (RpgGameState.skillsLearned.Contains("Repair I")) {
                    return new Result {
                        text = "Since you have some experience in this kind of stuff, 5 units of minerals was enough.",
                        effects = {
                            new Effect{
                                kind = EffectKind.AddMinerals,
                                value = -5,
                            },
                        },
                    };
                }
                return new Result {
                    text = "For such a rookie mechanic, you had to waste 10 units of minerals to finish the repairs.",
                    effects = {
                        new Effect{
                            kind = EffectKind.AddMinerals,
                            value = -10,
                        },
                    }
                };
            },
        });
        return e;
    }

    private static RandomEvent newNebulaStorm() {
        var e = new RandomEvent();
        e.title = "Nebula Storm";
        e.expReward = 2;
        e.luckScore = 4;
        e.trigger = TriggerKind.OnSpaceTravelling;
        e.text = multilineText(
            "Your route lies through the nebula ion storm.",
            "It you continue this way, your batteries may lose some charges,",
            "but if you choose not to, that will force you to go around it."
        );
        e.actions.Add(new Action {
            name = "Into the storm",
            apply = (RandomEventContext _) => {
                return new Result {
                    text = multilineText(
                        "Every vessel suffered energy damage.",
                        "On the bright side, you managed to collect 5 points of power resource."
                    ),
                    effects = {
                        new Effect{
                            kind = EffectKind.AddFleetBackupEnergyPercentage,
                            value = new Vector2(0.1f, 0.5f),
                        },
                        new Effect{
                            kind = EffectKind.AddPower,
                            value = 5,
                        },
                    },
                };
            },
        });
        e.actions.Add(new Action {
            name = "Fly around it",
            apply = (RandomEventContext _) => {
                return new Result {
                    text = multilineText(
                        "Better safe than sorry.",
                        "Your fleet takes the longer way to the destination.",
                        "Movement speed decreased for a short period of time."
                    ),
                    effects = {
                        new Effect{
                            kind = EffectKind.ApplySlow,
                            value = 10,
                        },
                    },
                };
            },
        });
        return e;
    }

    private static RandomEvent newAsteroids() {
        var e = new RandomEvent { };
        e.title = "Asteroids";
        e.expReward = 5;
        e.luckScore = 3;
        e.trigger = TriggerKind.OnSystemEntered;
        e.text = multilineText(
            "You've entered the system and got right into the asteroids hell.",
            "",
            "It looks like it will be very hard to get out unscratched."
        );
        e.actions.Add(new Action {
            name = "Take the hit",
            apply = (RandomEventContext _) => {
                return new Result {
                    text = "Although you took some damage, every vessel from your fleet made it through.",
                    effects = {
                        new Effect{
                            kind = EffectKind.DamageFleetPercentage,
                            value = new Vector2(0.01f, 0.1f),
                        },
                    },
                };
            },
        });
        e.actions.Add(new Action {
            name = "Warp away",
            condition = () => RpgGameState.fuel >= 70,
            apply = (RandomEventContext _) => {
                return new Result {
                    text = multilineText(
                        "To avoid a risk of being hit, you performed an emergency warp jump.",
                        "70 units of fuel is not a high price for safety, right?"
                    ),
                    effects = {
                        new Effect{
                            kind = EffectKind.AddFuel,
                            value = -70,
                        },
                    },
                };
            },
        });
        return e;
    }

    public static RandomEvent[] list;
    public static List<RandomEvent> onSystemEnteredList;
    public static List<RandomEvent> onSystemPatrolingList;
    public static List<RandomEvent> onSpaceTravellingList;

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
        };
        onSystemEnteredList = new List<RandomEvent>();
        onSystemPatrolingList = new List<RandomEvent>();
        onSpaceTravellingList = new List<RandomEvent>();
        foreach (var e in list) {
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
        var nearbySystems = new List<StarSystem>();
        foreach (var sys in RpgGameState.starSystems) {
            if (sys.pos == RpgGameState.humanUnit.pos) {
                continue;
            }
            if (sys.pos.DistanceTo(RpgGameState.humanUnit.pos) < 400) {
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
