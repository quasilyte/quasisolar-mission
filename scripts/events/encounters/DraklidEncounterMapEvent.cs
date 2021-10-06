public class DraklidEncounterMapEvent: AbstractMapEvent {
    public DraklidEncounterMapEvent() {
        title = "Draklid Encounter";
        triggerKind = TriggerKind.OnScript;
    }

    public override AbstractMapEvent Create(RandomEventContext ctx) {
        var e = new DraklidEncounterMapEvent();

        var canIgnore = !e.DraklidsWantToAttack(ctx.spaceUnit);

        var text = "Short-range radars detect a Draklid raid unit.\n\n";

        if (canIgnore) {
            text += "It looks like they're not looking for a fight.\n";
        } else {
            text += @"
                Based on the fact that it's moving towards your direction,
                the battle is imminent, unless you sacrifice some fuel to warp away.
            ";
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

        if (canIgnore) {
            e.actions.Add(new Action{
                name = "Ignore",
                apply = () => {
                    return new Result{
                        skipText = true,
                        effects = {
                            new Effect{
                                kind = EffectKind.SpaceUnitRetreat,
                                value = ctx.spaceUnit,
                            }
                        },
                    };
                },
            });
        } else {
            e.actions.Add(new Action{
                name = "Escape",
                hint = () => "(" + RpgGameState.RetreatFuelCost() + " fuel)",
                condition = () => GameState().fuel >= RpgGameState.RetreatFuelCost(),
                apply = () => new Result{
                    skipText = true,
                    effects = {
                        new Effect{
                            kind = EffectKind.AddFuel,
                            value = -RpgGameState.RetreatFuelCost(),
                        },
                    },
                },
            });
        }
        

        return e;
    }

    private bool DraklidsWantToAttack(SpaceUnit u) {
        if (u.CargoFree() == 0) {
            return false;
        }
        var draklidForce = u.FleetCost();
        var humanForce = RpgGameState.instance.humanUnit.Get().FleetCost();
        return draklidForce * 2 > humanForce;
    }
}
