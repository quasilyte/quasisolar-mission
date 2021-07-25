public class LoneKrigiaScoutMapEvent: AbstractMapEvent {
    public LoneKrigiaScoutMapEvent() {
        title = "Lone Krigia Scout";
        luckScore = 5;
        triggerKind = TriggerKind.OnSystemEntered;
    }

    public override bool Condition() {
        return !AtPlayerSystem();
    }

    public override AbstractMapEvent Create(RandomEventContext ctx) {
        var e = new LoneKrigiaScoutMapEvent();

        e.text = MultilineText(@"
            You locate a single Talons class Krigia vessel.
            It could be a patrol unit remainings that tries to leave this system.
            
            As it doesn't possess any threat, it's up to you whether you want to attack it.
        ");

        e.actions.Add(new Action{
            name = "Attack the scout",
            apply = () => {
                var v = VesselFactory.NewVessel(Faction.Krigia, "Talons");
                var spaceUnit = NewSpaceUnit(Faction.RandomEventHostile, v);
                if (ctx.roll > 0.5) {
                    spaceUnit.cargo.minerals = (int)(ctx.roll * 20);
                }
                spaceUnit.cargo.power = (int)(ctx.roll * 15);
                if (HasLuckSkill()) {
                    spaceUnit.cargo.minerals *= 2;
                    spaceUnit.cargo.power *= 2;
                }

                return new Result {
                    text = MultilineText(@"
                        It looks like they accept your challenge.

                        You should prepare for the battle too.
                    "),
                    effects = {
                        new Effect{
                            kind = EffectKind.AddReputation,
                            value = -1,
                            value2 = Faction.Krigia,
                        },
                        new Effect{
                            kind = EffectKind.EnterArena,
                            value = spaceUnit,
                        },
                    },
                };
            }
        });

        e.actions.Add(new Action{
            name = "Let it be",
            apply = () => {
                return new Result{
                    text = MultilineText(@"
                        The scout charges its engine and jumps away.
                        
                        Will it come back with a fleet?
                    "),
                    effects = {
                        new Effect{
                            kind = EffectKind.AddReputation,
                            value = +1,
                            value2 = Faction.Krigia,
                        },
                    },
                };
            }
        });

        return e;
    }
}
