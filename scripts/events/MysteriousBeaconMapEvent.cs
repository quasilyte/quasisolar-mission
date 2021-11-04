using Godot;

public class MysteriousBeaconMapEvent: AbstractMapEvent {
    public MysteriousBeaconMapEvent() {
        title = "Mysterious Beacon";
        luckScore = 5;
        triggerKind = TriggerKind.OnSystemEntered;
    }

    public override bool Condition() {
        return GameState().day >= 200 && AtNeutralSystem();
    }

    public override AbstractMapEvent Create(RandomEventContext ctx) {
        var e = new MysteriousBeaconMapEvent();

        e.text = MultilineText(@"
            You discovered a weak signal coming from a small
            uninhabited planet.

            The investigation revealed a half-destroyed hangar
            containing an artifact that was a source of the signals
            we catched from the planet orbit.

            A closer inspection shows that it can be used as a vessel mod.
            Upon installation, it's expected to increase the host
            energy recovery rate.
        ");

        for (int i = 0; i < PlayerSpaceUnit().fleet.Count; i++) {
            var v = PlayerSpaceUnit().fleet[i].Get();
            var index = i; // Copy to capture the correct value
            e.actions.Add(new Action {
                name = "Install mod to " + v.pilotName,
                condition = () => v.modList.Count < 5,
                apply = () => {
                    return new Result{
                        skipText = true,
                        effects = {
                            new Effect{
                                kind = EffectKind.AddVesselMod,
                                value = index,
                                value2 = "Dark Beacon",
                            },
                        },
                    };
                }
            });
        }

        e.actions.Add(new Action{
            name = "Ignore it",
            apply = () => {
                return new Result{
                    skipText = true,
                };
            }
        });

        return e;
    }
}
