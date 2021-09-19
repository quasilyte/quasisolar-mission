using Godot;
using System;

public class NoiseSpamMapEvent: AbstractMapEvent {
    public NoiseSpamMapEvent() {
        title = "Noise Spam";
        luckScore = 3;
        triggerKind = TriggerKind.OnSystemEntered;
    }

    public override bool Condition() {
        if (GameState().day < 1500 || !AtNeutralSystem()) {
            return false;
        }
        return PlayerSpaceUnit().fleet.Count >= 3;
    }

    public override AbstractMapEvent Create(RandomEventContext ctx) {
        var e = new NoiseSpamMapEvent();

        e.text = MultilineText(@"
            An unknown alien group is trying to jam our radars
            with a heavy noise stream. So far we don't know if
            they're trying to communicate with us or it's really
            an attempt to make a surprise attack.

            Their fleet consists of 3 medium-sized vessels.
        ");

        System.Action extraReward = () => ArenaSettings.extraReward = (BattleResult result) => result.research = "Tempest";

        e.actions.Add(new Action{
            name = "Attack",
            apply = () => {
                return new Result {
                    skipText = true,
                    effects = {
                        new Effect{
                            kind = EffectKind.EnterArena,
                            value = GetRavagersUnit(),
                            fn = extraReward,
                        },
                    },
                };
            }
        });

        e.actions.Add(new Action{
            name = "Wait",
            apply = () => {
                var index = QRandom.IntRange(1, PlayerSpaceUnit().fleet.Count - 1);
                var pilot = PlayerSpaceUnit().fleet[index].Get().pilotName;
                return new Result {
                    text = MultilineText($@"
                        You decided to avoid the premature interpretation and wait.

                        The fleet approached us and opened fire.
                        Some of your vessels were caught in that line of fire.

                        They have the advantage now.
                    "),
                    effects = {
                        new Effect {
                            kind = EffectKind.DamageFleetPercentage,
                            value = new Vector2(0.1f, 0.2f),
                        },
                        new Effect{
                            kind = EffectKind.EnterArena,
                            value = GetRavagersUnit(),
                            fn = extraReward,
                        },
                    },
                };
            }
        });

        return e;
    }

    private SpaceUnit GetRavagersUnit() {
        var u = NewSpaceUnit(Faction.RandomEventHostile,
            VesselFactory.NewVessel(Faction.Neutral, "Ravager"),
            VesselFactory.NewVessel(Faction.Neutral, "Ravager", 3),
            VesselFactory.NewVessel(Faction.Neutral, "Ravager"));
        if (QRandom.Bool()) {
            u.cargo.minerals = QRandom.IntRange(40, 100);
        } else {
            u.cargo.power = QRandom.IntRange(30, 80);
        }
        return u;
    }
}
