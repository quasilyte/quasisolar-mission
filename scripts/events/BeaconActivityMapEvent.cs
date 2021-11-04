using Godot;

public class BeaconActivityMapEvent: AbstractMapEvent {
    public BeaconActivityMapEvent() {
        title = "Beacon Activity";
        luckScore = 5;
        triggerKind = TriggerKind.OnSystemEntered;
    }

    public override bool Condition() {
        return GameState().day >= 300 && DarkBeaconBearer() != null;
    }

    private Vessel DarkBeaconBearer() {
        foreach (var v in PlayerSpaceUnit().fleet) {
            if (v.Get().modList.Contains("Dark Beacon")) {
                return v.Get();
            }
        }
        return null;
    }

    public override AbstractMapEvent Create(RandomEventContext ctx) {
        var e = new BeaconActivityMapEvent();

        e.text = MultilineText($@"
            After a throrough research, it became apparent that
            the beacon mod {DarkBeaconBearer().pilotName} is carrying emits some kind of a
            call signal. It broadcasts some information you can't decipher yet.

            It's probable that it can be traced back to your fleet,
            so there is some uncertainty whether it's safe to
            casually carry it around.

            The device itself seem to resonate differently with
            different stars. The hotter stars negate the signal almost completely
            while the coolest stars make it more intensive.
        ");

        e.actions.Add(new Action{
            name = "OK",
            apply = () => {
                return new Result{
                    skipText = true,
                };
            }
        });

        return e;
    }
}
