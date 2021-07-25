using Godot;
using System;

public class SpaceNomadsMapEvent: AbstractMapEvent {
    public SpaceNomadsMapEvent() {
        title = "Space Nomads";
        luckScore = 8;
        triggerKind = TriggerKind.OnSystemEntered;
    }

    public override bool Condition() {
        if (GameState().credits < VesselDesign.Find("Nomad").sellingPrice) {
            return false;
        }
        return PlayerSpaceUnit().fleet.Count < SpaceUnit.maxFleetSize;
    }

    public override AbstractMapEvent Create(RandomEventContext ctx) {
        var e = new SpaceNomadsMapEvent();

        var nomadDesign = VesselDesign.Find("Nomad");

        Func<int> vesselPrice = () => {
            var price = (int)(nomadDesign.sellingPrice * 0.7);
            if (HasSpeakingSkill()) {
                price = (int)(price * 0.8);
            }
            return price;
        };

        var text = (@"
            A fleet of unknown affiliation enters the system.

            It looks like their crew consists of different races.
            The scanning confirms the Zyth and Draklid presence
            as well as some other members that are hard to identify at this distance.

            Their broadcast claims that they're willing to sell one of their Nomad class vessels.
        ");
        if (HasSpeakingSkill()) {
            text += "\n(Speaking) Your speaking skills earned you a 20% discount.";
        }
        e.text = MultilineText(text);

        e.actions.Add(new Action {
            name = "Buy a vessel",
            hint = () => "(" + vesselPrice() + " credits)",
            condition = () => RpgGameState.instance.credits >= vesselPrice(),
            apply = () => {
                var v = RpgGameState.instance.NewVessel(Faction.Earthling, nomadDesign);
                v.pilotName = PilotNames.UniqHumanName(RpgGameState.instance.usedNames);
                VesselFactory.PadEquipment(v);
                VesselFactory.InitStats(v);
            
                return new Result{
                    text = MultilineText(@"
                        A new vessel is piloted to your fleet by a grumpy Zyth thug.

                        You transfer some of your crew members to it and make it ready to go.
                    "),
                    expReward = 4,
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
            apply = () => {
                return new Result{
                    text = MultilineText(@"
                        Instead of getting just one vessel,
                        you can now produce as many Nomad vessels as you please.
                        
                        As long as you have enough resources, that is.
                    "),
                    expReward = 4,
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
            apply = () => {
                var v1 = VesselFactory.NewVessel(Faction.Neutral, "Nomad");
                var v2 = VesselFactory.NewVessel(Faction.Neutral, "Nomad");
                var v3 = VesselFactory.NewVessel(Faction.Neutral, "Nomad");
                var spaceUnit = NewSpaceUnit(Faction.RandomEventHostile, v1, v2, v3);
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
            apply = () => {
                return new Result{
                    text = "You have other plans for your resource units.",
                };
            }
        });

        return e;
    }
}
