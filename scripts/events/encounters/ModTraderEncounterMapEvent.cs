public class ModTraderEncounterMapEvent: AbstractMapEvent {
    public ModTraderEncounterMapEvent() {
        title = "X-The-Bit Encounter";
        triggerKind = TriggerKind.OnScript;
    }

    public override AbstractMapEvent Create(RandomEventContext ctx) {
        var e = new ModTraderEncounterMapEvent();

        e.text = MultilineText(@"
            You've encountered an infamous travelling vessel mod trader,
            also known as X-The-Bit.

            The comm channel is available.
            You can contact the trader if you're interested in
            getting some mods.
        ");

        e.actions.Add(new Action{
            name = "Attack",
            apply = () => {
                return new Result{
                    skipText = true,
                    effects = {
                        new Effect{
                            kind = EffectKind.EnterArena,
                            value = ctx.spaceUnit,
                        },
                    },
                };
            }
        });

        e.actions.Add(new Action{
            name = "Communicate",
            apply = () => {
                return new Result{
                    skipText = true,
                    effects = {
                        new Effect{
                            kind = EffectKind.EnterTextQuest,
                            value = new ModTraderEncounterTQuest(),
                        }
                    },
                };
            }
        });

        e.actions.Add(new Action{
            name = "Ignore",
            apply = () => new Result{skipText = true},
        });

        return e;
    }
}
