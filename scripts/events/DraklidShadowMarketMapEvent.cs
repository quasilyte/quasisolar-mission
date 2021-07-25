public class DraklidShadowMarketMapEvent: AbstractMapEvent {
    public DraklidShadowMarketMapEvent() {
        title = "Draklid Shadow Market";
        luckScore = 8;
        triggerKind = TriggerKind.OnSystemEntered;
    }

    public override bool Condition() {
        return AtNeutralSystem() && GameState().credits > 5000 && GameState().StorageFreeSlot() != -1;
    }

    public override AbstractMapEvent Create(RandomEventContext ctx) {
        var e = new DraklidShadowMarketMapEvent();

        e.text = MultilineText(@"
            A Draklid vessel captain contacts you.

            `Fancy buying some weapons? We have the best prices.`

            You tried to trace the signal back, but failed to do so.
            They're probably hiding somewhere on a nearby planet.
        ");

        e.actions.Add(new Action{
            name = "Buy assault laser",
            hint = () => "(2800 RU)",
            condition = () => GameState().credits >= 2800,
            apply = () => {
                return new Result {
                    text = MultilineText($@"
                        As you transfered the payment, the hidden stash location was revealed to you.

                        When you arrived to the spot, you found an assault laser, just as promised.
                    "),
                    expReward = 4,
                    effects = {
                        new Effect{
                            kind = EffectKind.AddItem,
                            value = AssaultLaserWeapon.Design,
                        },
                        new Effect{
                            kind = EffectKind.AddCredits,
                            value = -2800,
                        },
                    },
                };
            }
        });

        e.actions.Add(new Action{
            name = "Buy disruptor",
            hint = () => "(2300 RU)",
            condition = () => GameState().credits >= 2300,
            apply = () => {
                return new Result {
                    text = MultilineText($@"
                        As you transfered the payment, the hidden stash location was revealed to you.

                        When you arrived to the spot, you found a disruptor, just as promised.
                    "),
                    expReward = 4,
                    effects = {
                        new Effect{
                            kind = EffectKind.AddItem,
                            value = DisruptorWeapon.Design,
                        },
                        new Effect{
                            kind = EffectKind.AddCredits,
                            value = -2300,
                        },
                    },
                };
            }
        });

        e.actions.Add(new Action{
            name = "Don't buy anything",
            apply = () => {
                return new Result{
                    text = "No shadow market deals today.",
                };
            }
        });

        return e;
    }
}
