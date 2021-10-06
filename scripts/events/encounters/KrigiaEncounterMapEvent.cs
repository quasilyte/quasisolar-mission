public class KrigiaEncounterMapEvent: AbstractMapEvent {
    public KrigiaEncounterMapEvent() {
        title = "Krigia Encounter";
        triggerKind = TriggerKind.OnScript;
    }

    public override AbstractMapEvent Create(RandomEventContext ctx) {
        var e = new KrigiaEncounterMapEvent();

        var text = "Krigia unit searches this area.\n\n";
        if (AtPlayerSystem()) {
            text += "If we don't intercept them, they will discover our star base.\n";
        } else {
            text += "We should either engage them or retreat while we have the opportunity.\n";
        }
        e.text = MultilineText(text);

        e.actions.Add(new Action{
            name = "Attack",
            apply = () => {
                return new Result{
                    skipText = true,
                    effects = {
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
                            value = new KrigiaPatrolTQuest(),
                        }
                    },
                };
            }
        });

        e.actions.Add(new Action{
            name = "Retreat",
            hint = () => "(" + RpgGameState.RetreatFuelCost() + " fuel)",
            condition = () => GameState().fuel >= RpgGameState.RetreatFuelCost(),
            apply = () => new Result{
                skipText = true,
                effects = {
                    new Effect{
                        kind = EffectKind.AddFuel,
                        value = -(int)RpgGameState.RetreatFuelCost(),
                    },
                    new Effect{
                        kind = EffectKind.KrigiaDetectsStarBase,
                        value = CurrentStarSystem(),
                    }
                },
            },
        });

        return e;
    }
}
