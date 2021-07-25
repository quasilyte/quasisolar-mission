public class KrigiaDroneMapEvent: AbstractMapEvent {
    public KrigiaDroneMapEvent() {
        title = "Krigia Drone";
        luckScore = 7;
        triggerKind = TriggerKind.OnSystemEntered;
    }

    public override bool Condition() {
        return AtKrigiaSystem();
    }

    public override AbstractMapEvent Create(RandomEventContext ctx) {
        var e = new KrigiaDroneMapEvent();

        e.text = MultilineText(@"
            A lone Krigia exploration drone flies across the planet nearby.

            It's possible to intercept that drone and take whatever it managed to scavenge out there.

            It's also possible to capture it, but this maneuver would
            require more fuel to perform.
        ");

        e.actions.Add(new Action{
            name = "Capture the drone",
            hint = () => "(100 fuel)",
            condition = () => RpgGameState.instance.fuel >= 100 && RpgGameState.MaxExplorationDrones() > RpgGameState.instance.explorationDrones.Count,
            apply = () => {
                var ru = (int)(ctx.roll * 1000) + 1500;
                return new Result{
                    text = $"You successfully captured the Scavenger drone along with {ru} resource units it carried.",
                    expReward = 6,
                    effects = {
                        new Effect{
                            kind = EffectKind.AddFuel,
                            value = -100,
                        },
                        new Effect{
                            kind = EffectKind.AddDrone,
                            value = "Scavenger",
                        },
                        new Effect{
                            kind = EffectKind.AddCredits,
                            value = ru,
                        },
                    },
                };
            }
        });

        e.actions.Add(new Action{
            name = "Gun down the drone",
            hint = () => "(10 fuel)",
            condition = () => RpgGameState.instance.fuel >= 10,
            apply = () => {
                var ru = (int)(ctx.roll * 1000) + 1000;
                return new Result{
                    text = MultilineText($@"
                        A few minutes later, the drone is no more.

                        What remains is there for you to harvest ({ru} resource units).
                    "),
                    expReward = 4,
                    effects = {
                        new Effect{
                            kind = EffectKind.AddFuel,
                            value = -10,
                        },
                        new Effect{
                            kind = EffectKind.AddCredits,
                            value = ru,
                        },
                    },
                };
            }
        });

        e.actions.Add(new Action{
            name = "Don't do anything",
            apply = () => {
                return new Result{
                    text = "It would be impractical to spend any fuel on this chasing game right now.",
                };
            }
        });

        return e;
    }
}
