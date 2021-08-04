using Godot;
using System;

public class SpectreAttackMapEvent: AbstractMapEvent {
    public SpectreAttackMapEvent() {
        title = "Spectre Attack";
        luckScore = 4;
        triggerKind = TriggerKind.OnSystemEntered;
    }

    public override bool Condition() {
        if (GameState().day < 1200 || !AtNeutralSystem()) {
            return false;
        }
        if (!EventHappened("Spectre")) {
            return false;
        }
        return PlayerSpaceUnit().fleet.Count >= 2;
    }

    public override AbstractMapEvent Create(RandomEventContext ctx) {
        var e = new SpectreAttackMapEvent();

        e.text = MultilineText(@"
            This is your second encounter with a Spectre vessel.

            You can detect its missle tracker targeting the flagship.
            
            It's not possible to outmaneuver it, so the battle can't be avoided.

            It should be possible to sacrifice one of your vessels to
            get it distracted so the rest of your fleet can escape.
        ");

        e.actions.Add(new Action{
            name = "Attack",
            apply = () => {
                var spectre = VesselFactory.NewVessel(Faction.Neutral, "Spectre");
                return new Result {
                    text = MultilineText(@"
                        We don't know the reasons, but we know the intentions.

                        Be prepared for a tough battle.
                    "),
                    effects = {
                        new Effect{
                            kind = EffectKind.EnterArena,
                            value = NewSpaceUnit(Faction.RandomEventHostile, spectre),
                        },
                    },
                };
            }
        });

        e.actions.Add(new Action{
            name = "Make a sacrifice",
            apply = () => {
                var index = QRandom.IntRange(1, PlayerSpaceUnit().fleet.Count - 1);
                var pilot = PlayerSpaceUnit().fleet[index].Get().pilotName;
                return new Result {
                    text = MultilineText($@"
                        {pilot} was chosen to take a distraction mission.

                        Your fleet managed to avoid that grave encounter,
                        but the cost was high.
                    "),
                    effects = {
                        new Effect{
                            kind = EffectKind.DestroyVessel,
                            value = index,
                        },
                    },
                };
            }
        });

        return e;
    }
}
