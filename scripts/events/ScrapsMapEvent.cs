public class ScrapsMapEvent: AbstractMapEvent {
    public ScrapsMapEvent() {
        title = "Scraps";
        luckScore = 8;
        triggerKind = TriggerKind.OnSystemEntered;
    }

    public override bool Condition() {
        return !AtPlayerSystem() && GameState().fuel < 120;
    }

    public override AbstractMapEvent Create(RandomEventContext ctx) {
        var e = new ScrapsMapEvent();

        var fuel = QRandom.IntRange(100, 200);
        var minerals = QRandom.IntRange(20, 50);
        e.text = MultilineText($@"
            While navigating through the asteroids belt, you found a
            big cloud of scraps swimming the space.

            It's probably a victim of asteroids, you concluded.
            Since it was on your way, you colledted some useful parts.

            Found {minerals} minerals and {fuel} fuel units.
        ");

        e.actions.Add(new Action {
            name = "OK",
            apply = () => {
                return new Result{
                    skipText = true,
                    effects = {
                        new Effect{
                            kind = EffectKind.AddFuel,
                            value = fuel,
                        },
                        new Effect{
                            kind = EffectKind.AddMinerals,
                            value = minerals,
                        },
                    },
                };
            }
        });

        return e;
    }
}
