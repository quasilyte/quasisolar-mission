public class NaturePresentMapEvent: AbstractMapEvent {
    public NaturePresentMapEvent() {
        title = "A Nature Present";
        luckScore = 9;
        triggerKind = TriggerKind.OnSystemEntered;
    }

    public override bool Condition() {
        return !AtPlayerSystem();
    }

    public override AbstractMapEvent Create(RandomEventContext ctx) {
        var e = new NaturePresentMapEvent();

        var vesselIndex = QRandom.IntRange(0, PlayerSpaceUnit().fleet.Count-1);
        var vessel = PlayerSpaceUnit().fleet[vesselIndex].Get();

        e.text = MultilineText($@"
            You found a planet with a jungle-like nature.

            While {vessel.pilotName} was on a reconnaissance mission,
            an odd thing was discovered.
            An organic form of life occupied the vessel exterior,
            forming some kind of a shell around it.

            The analysis predicts it to have a positive effect on a
            hull overall durability, but it might make it less resistant
            to thermal damage.
        ");

        e.actions.Add(new Action {
            name = "Keep the organic shell",
            apply = () => {
                return new Result{
                    text = MultilineText($@"
                        {vessel.pilotName} vessel keeps the organic shell.

                        The shell looks happy.
                    "),
                    expReward = 2,
                    effects = {
                        new Effect{
                            kind = EffectKind.AddPatch,
                            value = vesselIndex,
                            value2 = "Organic Shell",
                        },
                    },
                };
            }
        });

        e.actions.Add(new Action{
            name = "Get rid of the shell",
            apply = () => {
                return new Result{
                    text = MultilineText($@"
                        As you don't know all possible consequences of keeping
                        that thing around, you decided to avoid it.

                        It took several hours to remove that living thing from the vessel.
                    "),
                    expReward = 3,
                };
            }
        });

        return e;
    }
}
