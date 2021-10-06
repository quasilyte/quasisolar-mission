public class PatrolAttackMapEvent: AbstractMapEvent {
    public PatrolAttackMapEvent() {
        title = "Patrol Attack";
        triggerKind = TriggerKind.OnScript;
    }

    public override AbstractMapEvent Create(RandomEventContext ctx) {
        var e = new PatrolAttackMapEvent();

        e.text = MultilineText($@"
            {ctx.spaceUnit.owner} star base patrol approaches your fleet
            in order to force you to leave this system.
        ");

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
                },
            },
        });

        return e;
    }
}
