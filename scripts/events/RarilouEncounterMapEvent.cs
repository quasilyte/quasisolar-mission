public class RarilouEncounterMapEvent: AbstractMapEvent {
    public RarilouEncounterMapEvent() {
        title = "Rarilou Encounter";
        luckScore = 5;
        triggerKind = TriggerKind.OnSystemEntered;
    }

    public override bool Condition() { return false; }

    public override AbstractMapEvent Create(RandomEventContext ctx) {
        var e = new RarilouEncounterMapEvent();

        e.text = MultilineText(@"
            You've encountered a Rarilou fleet.
            
            They're already trying to leave this system,
            so if you have any business with them,
            you need to do it now.
        ");

        e.actions.Add(new Action{
            name = "Attack",
            apply = () => {
                return new Result{
                    skipText = true,
                    effects = {
                        new Effect{
                            kind = EffectKind.DeclareWar,
                            value = Faction.Rarilou,
                        },
                        new Effect{
                            kind = EffectKind.AddReputation,
                            value = -4,
                            value2 = Faction.Rarilou,
                        },
                        new Effect{
                            kind = EffectKind.EnterArena,
                            value = ctx.spaceUnit,
                        },
                    },
                };
            }
        });

        e.actions.Add(new Action{
            name = "Communicate",
            apply = () => {
                return new Result{
                    skipText = true,
                    effects = {
                        new Effect{
                            kind = EffectKind.PrepareArenaSettings,
                        },
                        new Effect{
                            kind = EffectKind.EnterTextQuest,
                            value = new RarilouEncounterTQuest(),
                        }
                    },
                };
            }
        });

        e.actions.Add(new Action{
            name = "Ignore",
            apply = () => {
                return new Result{
                    skipText = true,
                };
            }
        });

        return e;
    }
}
