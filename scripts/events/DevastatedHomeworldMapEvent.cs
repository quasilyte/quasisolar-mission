using Godot;

public class DevastatedHomeworldMapEvent: AbstractMapEvent {
    public DevastatedHomeworldMapEvent() {
        title = "Devastated Homeworld";
        luckScore = 6;
        triggerKind = TriggerKind.OnSystemEntered;
    }

    public override bool Condition() {
        return IsFirstSystemVisit() && AtNeutralSystem();
    }

    public override AbstractMapEvent Create(RandomEventContext ctx) {
        var e = new DevastatedHomeworldMapEvent();

        e.text = MultilineText(@"
            As you were performing a common planet scanning routine,
            you found traces of the civilization that met its demise.
            
            No signs of life, all cities are in ruins.
            It looks like this planet was bombed from the orbit for several days.
            
            The climate on this planet is very rough, so it's dangerous to explore it.
            There is a chance that you can find something of merit in there too.
        ");

        e.actions.Add(new Action{
            name = "Search the planet",
            apply = () => {
                var bounty = 175;
                return new Result {
                    text = MultilineText($@"
                        Your flagship took a lot of damage, but it was worth it.
                        
                        Now you have a proof that this race was destroyed by Krigia forces.
                        They left a lot of traces all over this planet.
                        
                        You've collected {bounty} Krigia material for the research purposes.
                    "),
                    expReward = 3,
                    effects = {
                        new Effect{
                            kind = EffectKind.DamageFlagshipPercentage,
                            value = new Vector2(0.5f, 0.8f),
                        },
                        new Effect{
                            kind = EffectKind.AddKrigiaMaterial,
                            value = bounty,
                        },
                    },
                };
            }
        });

        e.actions.Add(new Action{
            name = "Ignore this planet",
            apply = () => {
                return new Result{
                    text = "You decided to avoid the wrath of this planets' weather.",
                };
            }
        });

        return e;
    }
}
