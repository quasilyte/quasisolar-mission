public class FuelTraderMapEvent: AbstractMapEvent {
    public FuelTraderMapEvent() {
        title = "Fuel Trader";
        luckScore = 7;
        triggerKind = TriggerKind.OnSystemEntered;
    }

    public override bool Condition() {
        return AtNeutralSystem() && GameState().fuel < 100;
    }

    public override AbstractMapEvent Create(RandomEventContext ctx) {
        var e = new FuelTraderMapEvent();

        e.text = MultilineText(@"
            A small automated refuel drone approaches your fleet.

            It's willing to sell you some fuel.

            If you can't afford buying it, there is a special
            offer to get 50 fuel units for free.
        ");

        e.actions.Add(new Action {
            name = "Accept 50 fuel units for free",
            apply = () => {
                return new Result{
                    text = MultilineText(@"
                        The drone boards you flagship and transfers exactly 50 units
                        of fuel, just as it promised.
                    "),
                    effects = {
                        new Effect{
                            kind = EffectKind.AddFuel,
                            value = +50,
                        },
                    },
                };
            }
        });

        e.actions.Add(new Action {
            name = "Buy 250 fuel units",
            hint = () => "(750 RU)",
            condition = () => GameState().credits >= 750,
            apply = () => {
                return new Result{
                    text = MultilineText(@"
                        The drone boards you flagship and transfers exactly 250 units of fuel.

                        3 RU per fuel unit is a good deal.
                    "),
                    effects = {
                        new Effect{
                            kind = EffectKind.AddCredits,
                            value = -750,
                        },
                        new Effect{
                            kind = EffectKind.AddFuel,
                            value = +250,
                        },
                    },
                };
            }
        });

        e.actions.Add(new Action{
            name = "Ignore it",
            apply = () => {
                return new Result{
                    skipText = true,
                };
            }
        });

        return e;
    }
}
