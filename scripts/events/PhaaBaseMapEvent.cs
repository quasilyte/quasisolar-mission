public class PhaaBaseMapEvent: AbstractMapEvent {
    public PhaaBaseMapEvent() {
        title = "Phaa Base";
        luckScore = 5;
        triggerKind = TriggerKind.OnSystemEntered;
    }

    public override bool Condition() { return false; }

    public override AbstractMapEvent Create(RandomEventContext ctx) {
        var e = new PhaaBaseMapEvent();

        e.text = MultilineText(@"
            This system has a Phaa star base.

            The communication channel is open, so it's possible to contact them.
        ");

        e.actions.Add(new Action{
            name = "Communicate",
            apply = () => {
                AbstractTQuest q;
                if (RpgGameState.instance.phaaPlans.visited) {
                    q = new PhaaBaseTQuest();
                } else {
                    q = new PhaaIntroTQuest();
                }
                RpgGameState.instance.phaaPlans.visited = true;
                return new Result{
                    skipText = true,
                    effects = {
                        new Effect{
                            kind = EffectKind.EnterTextQuest,
                            value = q,
                        }
                    },
                };
            }
        });

        e.actions.Add(new Action{
            name = "Ignore",
            apply = () => {
                return new Result{
                    skipText = true,
                };
            }
        });

        return e;
    }
}
