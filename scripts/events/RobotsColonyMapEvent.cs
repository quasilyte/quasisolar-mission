public class RobotsColonyMapEvent: AbstractMapEvent {
    public RobotsColonyMapEvent() {
        title = "Robots Colony";
        luckScore = 9;
        triggerKind = TriggerKind.OnSystemEntered;
    }

    public override bool Condition() {
        if (GameState().credits < 5000 || GameState().day < 200) {
            return false;
        }
        if (!AtNeutralSystem()) {
            return false;
        }
        return !Flagship().modList.Contains("Energy Deviator") ||
            !Flagship().modList.Contains("Star Heat Resistor") &&
            Flagship().modList.Count < 5;
    }

    public override AbstractMapEvent Create(RandomEventContext ctx) {
        var e = new RobotsColonyMapEvent();

        e.text = MultilineText(@"
            You found a group of expedition robots roaming over the planet's surface.
            It looks like there is some resource shortage which makes
            it impossible for this colony to operate.
            
            They offered you a flagship upgrade in exchange for raw resources.

            The energy deviator increases the vessel energy resistance,
            but reduces kinetic resistance slightly.           
            The star heat resistor decreases the amount of damage your
            vessel receives from the star heat.
        ");

        e.actions.Add(new Action {
            name = "Pick energy deviator",
            hint = () => "(1500 RU)",
            condition = () => GameState().credits >= 1500 && !Flagship().modList.Contains("Energy Deviator"),
            apply = () => {
                return new Result{
                    text = MultilineText(@"
                        The energy deviator is successfully installed on your flagship.

                        Automated planet visitors thank you and return back to their duties.
                    "),
                    expReward = 3,
                    effects = {
                        new Effect{
                            kind = EffectKind.AddCredits,
                            value = -1500,
                        },
                        new Effect{
                            kind = EffectKind.AddVesselMod,
                            value = 0,
                            value2 = "Energy Deviator",
                        },
                    },
                };
            }
        });

        e.actions.Add(new Action {
            name = "Pick heat resistor",
            hint = () => "(500 RU)",
            condition = () => GameState().credits >= 500 && !Flagship().modList.Contains("Star Heat Resistor"),
            apply = () => {
                return new Result{
                    text = MultilineText(@"
                        The star heat resistor is successfully installed on your flagship.

                        Automated planet visitors thank you and return back to their duties.
                    "),
                    expReward = 3,
                    effects = {
                        new Effect{
                            kind = EffectKind.AddCredits,
                            value = -500,
                        },
                        new Effect{
                            kind = EffectKind.AddVesselMod,
                            value = 0,
                            value2 = "Star Heat Resistor",
                        },
                    },
                };
            }
        });

        e.actions.Add(new Action{
            name = "Leave the planet",
            apply = () => {
                return new Result{
                    text = "You decided not to help them.",
                };
            }
        });

        return e;
    }
}
