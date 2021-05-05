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
        AddVesselToFleet,
        AddTechnology,
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
        public Func<string> hint = () => "";
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

    private static SpaceUnit newSpaceUnit(Faction faction, params Vessel[] fleet) {
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

    private static RandomEvent newSpaceNomads() {
        var nomadDesign = VesselDesign.Find("Nomad");
        Func<int> vesselPrice = () => {
            var price = (int)(nomadDesign.sellingPrice * 0.7);
            if (RpgGameState.instance.skillsLearned.Contains("Diplomacy")) {
                price = (int)(price * 0.8);
            }
            return price;
        };

        // var blueprintPrice = (int)(vesselPrice * 1.2);

        var e = new RandomEvent { };
        e.title = "Space Nomads";
        e.expReward = 3;
        e.luckScore = 8;
        e.trigger = TriggerKind.OnSystemEntered;
        e.condition = () => {
            if (RpgGameState.instance.credits < nomadDesign.sellingPrice) {
                return false;
            }
            return RpgGameState.instance.humanUnit.Get().fleet.Count < SpaceUnit.maxFleetSize;
        };
        e.text = multilineText(
            "A group of space nomads crosses this system.",
            "",
            "They broadcast a message that says that they're willing to sell one of their Nomad class vessels."
        );
        e.extraText = (RandomEventContext _) => {
            if (RpgGameState.instance.skillsLearned.Contains("Diplomacy")) {
                return "Your diplomacy skill earned you a 20% discount.";
            }
            return "";
        };
        e.actions.Add(new Action {
            name = "Buy a vessel",
            hint = () => "(" + vesselPrice() + " credits)",
            condition = () => RpgGameState.instance.credits >= vesselPrice(),
            apply = (RandomEventContext _) => {
                var v = RpgGameState.instance.NewVessel(Faction.Human, nomadDesign);
                v.pilotName = PilotNames.UniqHumanName(RpgGameState.instance.usedNames);
                VesselFactory.PadEquipment(v);
            
                return new Result{
                    text = multilineText(
                        "A new vessel is piloted to your fleet by a grumpy Zyth thug.",
                        "You transfer some of your crew members to it and make it ready to go."
                    ),
                    effects = {
                        new Effect{
                            kind = EffectKind.AddCredits,
                            value = -vesselPrice(),
                        },
                        new Effect{
                            kind = EffectKind.AddVesselToFleet,
                            value = v,
                        },
                    },
                };
            }
        });
        e.actions.Add(new Action {
            name = "Buy blueprints",
            hint = () => "(" + (int)(vesselPrice() * 1.2) + " credits)",
            condition = () => RpgGameState.instance.credits >= (int)(vesselPrice() * 1.2),
            apply = (RandomEventContext _) => {
                return new Result{
                    text = multilineText(
                        "Instead of getting just one vessel, now you can produce as much Nomad vessels as you please.",
                        "",
                        "As long as you have enough resources, that is."
                    ),
                    effects = {
                        new Effect{
                            kind = EffectKind.AddCredits,
                            value = -(int)(vesselPrice() * 1.2),
                        },
                        new Effect{
                            kind = EffectKind.AddTechnology,
                            value = "Nomad",
                        },
                    },
                };
            }
        });
        e.actions.Add(new Action{
            name = "Attack the group",
            apply = (RandomEventContext ctx) => {
                var v1 = RpgGameState.instance.NewVessel(Faction.RandomEventHostile, nomadDesign);
                VesselFactory.Init(v1, nomadDesign);
                var v2 = RpgGameState.instance.NewVessel(Faction.RandomEventHostile, nomadDesign);
                VesselFactory.Init(v2, nomadDesign);
                var v3 = RpgGameState.instance.NewVessel(Faction.RandomEventHostile, nomadDesign);
                VesselFactory.Init(v3, nomadDesign);

                var spaceUnit = newSpaceUnit(Faction.RandomEventHostile, v1, v2, v3);
                spaceUnit.cargo.minerals = (int)(ctx.roll * 70);
                if (RpgGameState.instance.skillsLearned.Contains("Luck")) {
                    spaceUnit.cargo.minerals *= 3;
                }
                return new Result {
                    text = "You decided to attack the nomads group.",
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
            name = "Don't contact them",
            apply = (RandomEventContext _) => {
                return new Result{
                    text = "You have other plans for your credits.",
                };
            }
        });
        return e;
    }

    private static RandomEvent newPurpleSystemVisitor() {
        var e = new RandomEvent { };
        e.title = "Purple System Visitor";
        e.expReward = 20;
        e.luckScore = 1;
        e.trigger = TriggerKind.OnSystemEntered;
        e.condition = () => false;
        e.text = multilineText(
            "You entered a region of space that isn't exactly a normal star system, but rather an anomaly. There are no planets here.",
            "",
            "Few minutes later, you hear the board systems going crazy.",
            "Something is moving towards your fleet, but with a very unusual pattern.",
            "",
            "It looks like a battle station of unknown design."
        );
        e.actions.Add(new Action {
            name = "Prepare for the worst",
            apply = (RandomEventContext ctx) => {
                var v = RpgGameState.instance.vessels.New();
                v.isBot = true;
                v.faction = Faction.RandomEventHostile;
                v.pilotName = "FIXME";
                VesselFactory.Init(v, VesselDesign.Find("Visitor"));
                var spaceUnit = newSpaceUnit(Faction.RandomEventHostile, v);
                spaceUnit.cargo.power = (int)(ctx.roll * 50);
                if (RpgGameState.instance.skillsLearned.Contains("Luck")) {
                    spaceUnit.cargo.power *= 3;
                }
                return new Result {
                    text = multilineText(
                        "This thing doesn't respond to your communication attempts.",
                        "",
                        "As this platform closes the distance by every minute, you try to calculate your chances."
                    ),
                    effects = {
                        new Effect{
                            kind = EffectKind.EnterArena,
                            value = spaceUnit,
                        },
                    },
                };
            },
        });
        return e;
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
            if (RpgGameState.instance.technologiesResearched.Contains("Long-range Scanners") || RpgGameState.instance.technologiesResearched.Contains("Long-range Scanners II")) {
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
                foreach (var v in RpgGameState.instance.humanUnit.Get().fleet) {
                    if (v.Get().energy > 40) {
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
                var liner = RpgGameState.instance.vessels.New();
                liner.isBot = true;
                liner.faction = Faction.Wertu;
                liner.pilotName = "FIXME";
                VesselFactory.Init(liner, VesselDesign.Find("Transporter"));
                var spaceUnit = newSpaceUnit(Faction.RandomEventHostile, liner);
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
            condition = () => RpgGameState.instance.skillsLearned.Contains("Repair I") || RpgGameState.instance.skillsLearned.Contains("Repair II"),
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
            var v1 = RpgGameState.instance.vessels.New();
            v1.isBot = true;
            v1.faction = Faction.Pirate;
            v1.pilotName = "FIXME";
            VesselFactory.Init(v1, VesselDesign.Find("Pirate"));

            var v2 = RpgGameState.instance.vessels.New();
            v2.isBot = true;
            v2.faction = Faction.Pirate;
            v2.pilotName = "FIXME";
            VesselFactory.Init(v2, VesselDesign.Find("Pirate"));

            return newSpaceUnit(Faction.RandomEventHostile, v1, v2);
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
            return RpgGameState.instance.day > 150 && RpgGameState.instance.humanUnit.Get().FleetCost() < 17500;
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
                var v = RpgGameState.instance.vessels.New();
                v.isBot = true;
                v.faction = Faction.Zyth;
                v.pilotName = "FIXME";
                VesselFactory.Init(v, VesselDesign.Find("Hunter"));
                return new Result {
                    text = multilineText(
                        "The abandoned vessel was a Zyth trick!",
                        "",
                        "We're ambushed, prepare for battle!"
                    ),
                    effects = {
                        new Effect{
                            kind = EffectKind.EnterArena,
                            value = newSpaceUnit(Faction.RandomEventHostile, v),
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
        e.condition = () => RpgGameState.instance.humanUnit.Get().pos != RpgGameState.StartingSystem().pos;
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
            if (RpgGameState.instance.skillsLearned.Contains("Navigation II")) {
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
        e.condition = () => RpgGameState.instance.humanUnit.Get().fleet[0].Get().energySourceName != "None";
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
            condition = () => RpgGameState.instance.humanUnit.Get().cargo.power >= 3,
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
                if (RpgGameState.instance.skillsLearned.Contains("Repair II")) {
                    return RpgGameState.instance.humanUnit.Get().cargo.minerals >= 1;
                } else if (RpgGameState.instance.skillsLearned.Contains("Repair I")) {
                    return RpgGameState.instance.humanUnit.Get().cargo.minerals >= 5;
                }
                return RpgGameState.instance.humanUnit.Get().cargo.minerals >= 10;
            },
            apply = (RandomEventContext _) => {
                if (RpgGameState.instance.skillsLearned.Contains("Repair II")) {
                    return new Result {
                        text = "With your advanced skills, it only took 1 mineral unit to make it work as a new.",
                        effects = {
                            new Effect{
                                kind = EffectKind.AddMinerals,
                                value = -1,
                            },
                        },
                    };
                } else if (RpgGameState.instance.skillsLearned.Contains("Repair I")) {
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
            condition = () => RpgGameState.instance.fuel >= 70,
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
