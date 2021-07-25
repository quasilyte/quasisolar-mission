using Godot;
using System;

public class SpectreMapEvent: AbstractMapEvent {
    public SpectreMapEvent() {
        title = "Spectre";
        luckScore = 5;
        triggerKind = TriggerKind.OnSystemEntered;
    }

    public override bool Condition() {
        return GameState().day > 400 && AtNeutralSystem();
    }

    public override AbstractMapEvent Create(RandomEventContext ctx) {
        var e = new SpectreMapEvent();

        e.text = MultilineText(@"
            You have detected an unknown, very fast moving vessel
            pursuing a Krigia battleship.

            It looks like Krigia vessel is losing.

            You probably can't make it in time to join either
            of the sides, but it should be possible to get a little
            closer to have a better look at the encounter.
        ");

        e.actions.Add(new Action{
            name = "Watch closer",
            apply = () => {
                var spectre = VesselFactory.NewVessel(Faction.Neutral, "Spectre");
                spectre.spawnPos = new Vector2(1150, 800);
                var fangs = VesselFactory.NewVessel(Faction.Krigia, "Fangs", 1);
                fangs.spawnPos = new Vector2(1220, 140);
                fangs.energy = 0;
                fangs.hp = 90;
                return new Result {
                    text = MultilineText(@"
                        As you get closer, you observe a commencing battle
                        in all of its beauty.
                    "),
                    expReward = 2,
                    effects = {
                        new Effect{
                            kind = EffectKind.EnterArena,
                            value = NewSpaceUnit(Faction.RandomEventAlly, spectre),
                            value2 = NewSpaceUnit(Faction.RandomEventHostile, fangs),
                        },
                    },
                };
            }
        });

        e.actions.Add(new Action{
            name = "Don't get any closer",
            apply = () => {
                return new Result{
                    text = MultilineText(@"
                        The Krigia vessel gets destroyed in a few minutes.

                        That `Spectre` vessel vanished from your radars soon after.
                    "),
                };
            }
        });

        return e;
    }
}
