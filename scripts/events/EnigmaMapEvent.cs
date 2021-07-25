public class EnigmaMapEvent: AbstractMapEvent {
    public EnigmaMapEvent() {
        title = "Enigma";
        luckScore = 7;
        triggerKind = TriggerKind.OnSystemEntered;
    }

    public override bool Condition() {
        if (RpgGameState.MaxExplorationDrones() <= GameState().explorationDrones.Count) {
            return false;
        }
        return PlayerSpaceUnit().cargo.minerals != 0 ||
            PlayerSpaceUnit().cargo.organic != 0 ||
            PlayerSpaceUnit().cargo.power != 0;
    }

    public override AbstractMapEvent Create(RandomEventContext ctx) {
        var e = new EnigmaMapEvent();

        e.text = MultilineText(@"
            An automated merchant vessel aproached your fleet.

            Among all the things it can offer, you found only one
            thing that caught your attntion.

            Experimental Enigma exploration drone prototype that
            is said to be superior to all drone you've seen before.

            This merchant only accepts raw materials, like minerals.
        ");

        e.actions.Add(new Action{
            name = "Give minerals",
            hint = () => "(50)",
            condition = () => PlayerSpaceUnit().cargo.minerals >= 50,
            apply = () => {
                return new Result{
                    text = $"50 minerals is a good price for such a nice drone.",
                    expReward = 3,
                    effects = {
                        new Effect{
                            kind = EffectKind.AddMinerals,
                            value = -50,
                        },
                        new Effect{
                            kind = EffectKind.AddDrone,
                            value = "Enigma",
                        },
                    },
                };
            }
        });

        e.actions.Add(new Action{
            name = "Give organic",
            hint = () => "(25)",
            condition = () => PlayerSpaceUnit().cargo.organic >= 25,
            apply = () => {
                return new Result{
                    text = $"25 organic is a good price for such a nice drone.",
                    expReward = 3,
                    effects = {
                        new Effect{
                            kind = EffectKind.AddOrganic,
                            value = -25,
                        },
                        new Effect{
                            kind = EffectKind.AddDrone,
                            value = "Enigma",
                        },
                    },
                };
            }
        });

        e.actions.Add(new Action{
            name = "Give power",
            hint = () => "(15)",
            condition = () => RpgGameState.instance.humanUnit.Get().cargo.power >= 15,
            apply = () => {
                return new Result{
                    text = $"15 power is a good price for such a nice drone.",
                    expReward = 3,
                    effects = {
                        new Effect{
                            kind = EffectKind.AddPower,
                            value = -15,
                        },
                        new Effect{
                            kind = EffectKind.AddDrone,
                            value = "Enigma",
                        },
                    },
                };
            }
        });

        e.actions.Add(new Action{
            name = "Ignore this offer",
            apply = () => {
                return new Result{
                    text = "You decided that you don't need that exploration drone right now.",
                };
            }
        });

        return e;
    }
}
