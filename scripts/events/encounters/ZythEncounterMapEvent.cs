public class ZythEncounterMapEvent: AbstractMapEvent {
    public ZythEncounterMapEvent() {
        title = "Zyth Encounter";
        triggerKind = TriggerKind.OnScript;
    }

    public override AbstractMapEvent Create(RandomEventContext ctx) {
        var e = new ZythEncounterMapEvent();

        var duelAvailable = PlayerSpaceUnit().fleet.Count > 1;

        var attacker = ctx.spaceUnit.fleet[0].Get();

        var text = "";
        if (duelAvailable) {
            text = ($@"
                A single Zyth {attacker.designName} class vessel approaches your fleet.

                The captain challenges your flagship to perform a duel.
                This means that the rest of your fleet will not join the battle.

                Whether we accept these conditions or not, they'll try to
                defeat the flagship.

                Will you honor the Zyth rules or will you avoid the risk
                and take this {attacker.designName} by numbers?
            ");
        } else {
            text = ($@"
                A single Zyth {attacker.designName} class vessel approaches you.

                You receive a one-way message from their captains that can
                be roughly translated as `prepare for battle`.
            ");
        }

        e.text = MultilineText(text);

        if (duelAvailable) {
            e.actions.Add(new Action{
                name = "Accept a duel",
                apply = () => {
                    return new Result{
                        skipText = true,
                        effects = {
                            new Effect{
                                kind = EffectKind.AddReputation,
                                value = +4,
                                value2 = Faction.Zyth,
                            },
                            new Effect{
                                kind = EffectKind.EnterDuelArena,
                                value = ctx.spaceUnit,
                            },
                        },
                    };
                }
            });
            e.actions.Add(new Action{
                name = "Attack with your entire fleet",
                apply = () => {
                    return new Result{
                        skipText = true,
                        effects = {
                            new Effect{
                                kind = EffectKind.AddReputation,
                                value = -1,
                                value2 = Faction.Zyth,
                            },
                            new Effect{
                                kind = EffectKind.EnterArena,
                                value = ctx.spaceUnit,
                            },
                        },
                    };
                }
            });
        } else {
            e.actions.Add(new Action{
                name = "Attack",
                apply = () => {
                    return new Result{
                        skipText = true,
                        effects = {
                            new Effect{
                                kind = EffectKind.AddReputation,
                                value = +2,
                                value2 = Faction.Zyth,
                            },
                            new Effect{
                                kind = EffectKind.EnterArena,
                                value = ctx.spaceUnit,
                            },
                        },
                    };
                }
            });
        }

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
