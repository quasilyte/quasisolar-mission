public class BlackAsteroidMapEvent: AbstractMapEvent {
    public BlackAsteroidMapEvent() {
        title = "Black Asteroid";
        luckScore = 9;
        triggerKind = TriggerKind.OnSystemEntered;
    }

    public override bool Condition() {
        return AtNeutralSystem() &&
            GameState().day > 1000 && GameState().fuel < 200;
    }

    public override AbstractMapEvent Create(RandomEventContext ctx) {
        var e = new BlackAsteroidMapEvent();

        var power = QRandom.IntRange(80, 120);
        e.text = MultilineText($@"
            A stray asteroid enters the scan range.

            Analysis shows that it contains a lot of compounds that
            can be used to synthesize fuel or other energy resource.

            If we would send our exploration drone, we could get
            those resources. The drone should stay intact after the operation.
        ");

        e.actions.Add(new Action {
            name = "Send drone",
            condition = () => GameState().explorationDrones.Count != 0,
            apply = () => {
                return new Result{
                    text = MultilineText($@"
                        After two hours your drone returns with {power} units of power resource.

                        We can use it to generate fuel. Or we can bring it to one of our star bases.
                    "),
                    effects = {
                        new Effect{
                            kind = EffectKind.AddPower,
                            value = power,
                        },
                    },
                };
            }
        });

        e.actions.Add(new Action{
            name = "Do nothing",
            apply = () => {
                return new Result{
                    skipText = true,
                };
            }
        });

        return e;
    }
}
