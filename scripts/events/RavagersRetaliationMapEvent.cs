using Godot;
using System;

public class RavagersRetaliationMapEvent: AbstractMapEvent {
    public RavagersRetaliationMapEvent() {
        title = "Ravagers Retaliation";
        luckScore = 4;
        triggerKind = TriggerKind.OnSystemEntered;
    }

    public override bool Condition() {
        return GameState().day >= 2000 && PlayerSpaceUnit().fleet.Count >= 2 && 
            !AtPlayerSystem() && EventHappened("Noise Spam");
    }

    public override AbstractMapEvent Create(RandomEventContext ctx) {
        var e = new RavagersRetaliationMapEvent();

        e.text = MultilineText($@"
            The familiar spaceships we named Ravagers are trying to surround our flagship.

            Based on what we can decode now, they want to have a 1-on-1
            match with the flagship captain, {Flagship().pilotName}.

            Do we agree on these terms or should we attack with our entire fleet?
        ");

        System.Action extraReward = () => {
            var ru = QRandom.IntRange(5000, 9000);
            ArenaSettings.extraReward = (BattleResult result) => {
                result.ru = ru;
                result.popupText = MultilineText($@"
                    As {Flagship().pilotName} wins the battle, other Ravager
                    vessels start to turn and fly away.
                    One of them transfers {ru} RU to your fleet,
                    honoring your choice to accept their challenge.
                ");
            };
        };

        e.actions.Add(new Action{
            name = "Accept 1-on-1 challenge",
            apply = () => {
                return new Result {
                    skipText = true,
                    effects = {
                        new Effect{
                            kind = EffectKind.EnterDuelArena,
                            value = NewSpaceUnit(Faction.RandomEventHostile, VesselFactory.NewVessel(Faction.Neutral, "Ravager", 3)),
                            fn = extraReward,
                        },
                    },
                };
            }
        });

        e.actions.Add(new Action{
            name = "Attack with a whole fleet",
            apply = () => {
                var index = QRandom.IntRange(1, PlayerSpaceUnit().fleet.Count - 1);
                var pilot = PlayerSpaceUnit().fleet[index].Get().pilotName;
                return new Result {
                    text = MultilineText($@"
                        You decided that you have better chances in fleet-vs-fleet battle.
                    "),
                    effects = {
                        new Effect{
                            kind = EffectKind.EnterArena,
                            value = GetRavagersUnit(),
                        },
                    },
                };
            }
        });

        return e;
    }

    private SpaceUnit GetRavagersUnit() {
        var u = NewSpaceUnit(Faction.RandomEventHostile,
            VesselFactory.NewVessel(Faction.Neutral, "Ravager", 1),
            VesselFactory.NewVessel(Faction.Neutral, "Ravager", 3),
            VesselFactory.NewVessel(Faction.Neutral, "Ravager"),
            VesselFactory.NewVessel(Faction.Neutral, "Ravager"));
        return u;
    }
}
