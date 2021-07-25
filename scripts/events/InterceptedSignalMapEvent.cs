using System.Collections.Generic;

public class InterceptedSignalMapEvent: AbstractMapEvent {
    public InterceptedSignalMapEvent() {
        title = "Intercepted Signal";
        luckScore = 6;
        triggerKind = TriggerKind.OnSystemEntered;
    }

    public override bool Condition() {
        return AtKrigiaSystem();
    }

    public override AbstractMapEvent Create(RandomEventContext ctx) {
        var e = new InterceptedSignalMapEvent();

        var visited = new List<StarSystem>();
        var unvisited = new List<StarSystem>();
        foreach (var sys in GameState().starSystems.objects.Values) {
            foreach (var planet in sys.resourcePlanets) {
                if (planet.artifact == "") {
                    continue;
                }
                if (sys.visitsNum == 0) {
                    unvisited.Add(sys);
                } else {
                    visited.Add(sys);
                }
            }
        }

        StarSystem targetSys = null;
        if (unvisited.Count != 0) {
            targetSys = QRandom.Element(unvisited);
        } else if (visited.Count != 0) {
            targetSys = QRandom.Element(visited);
        }

        if (targetSys != null) {
            e.text = MultilineText($@"
                The Krigia star base sent an encrypted message
                to all its units in this system.

                It's not possible to decipher the message in any
                reasonable amount of time, but you can extract
                the partial information.

                It contains some numbers that look like coordinates.
                After a quick check, you can confirm that
                its a {targetSys.name} star system location.
            ");
            
        } else {
            e.text = MultilineText(@"
                The Krigia star base sent an encrypted message
                to all its units in this system.

                It's impossible to decipher anything out of it.
            ");
        }

        e.actions.Add(new Action{
            name = "OK",
            apply = () => {
                var text = targetSys == null ?
                    "It's unfortunate that you were unable to understand the message." :
                    $"Perhaps you need to pay this {targetSys.name} system a little visit.";
                return new Result{
                    text = text,
                };
            }
        });

        return e;
    }
}
