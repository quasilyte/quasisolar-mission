public class HackingMapEvent: AbstractMapEvent {
    public HackingMapEvent() {
        title = "Hacking";
        luckScore = 6;
        triggerKind = TriggerKind.OnSystemEntered;
    }

    public override bool Condition() {
        return AtNeutralSystem();
    }

    public override AbstractMapEvent Create(RandomEventContext ctx) {
        var e = new HackingMapEvent();

        e.text = MultilineText(@"
            Two Earthling-design Explorer class vessels fly towards your fleet.
            The weapons are charged.

            They could be re-programmed units from the previous war.
            But how your enemies managed to get their hands on our vessels?
            And how did they manage to re-program them?

            You can attempt to rewire these vessels back.
        ");

        e.actions.Add(new Action{
            name = "Let them closer",
            apply = () => {
                var v1 = VesselFactory.NewVessel(Faction.Neutral, "Explorer");
                var v2 = VesselFactory.NewVessel(Faction.Neutral, "Explorer");
                var unit = NewSpaceUnit(Faction.RandomEventHostile, v1, v2);
                unit.cargo.minerals = (int)(ctx.roll * 90) + 10;
                return new Result{
                    expReward = 5,
                    text = MultilineText(@"
                        As you suspected, they're acting hostile.
                        Not much left to do here; prepare your fleet for battle.
                    "),
                    effects = {
                        new Effect{
                            kind = EffectKind.EnterArena,
                            value = unit,
                        },
                    },
                };
            }
        });

        e.actions.Add(new Action{
            name = "Try to hack them",
            hint = () => "(text quest)",
            apply = () => {
                return new Result{
                    skipText = true,
                    effects = {
                        // new Effect{
                        //     kind = EffectKind.EnterTextQuest,
                        //     value = new HackingTQuest(),
                        // }
                    },
                };
            }
        });

        e.actions.Add(new Action{
            name = "Avoid the fight",
            apply = () => {
                return new Result{
                    expReward = 2,
                    text = "You managed to evade this confrontation.",
                };
            }
        });

        return e;
    }
}
