public class PurpleSystemVisitorMapEvent: AbstractMapEvent {
    public PurpleSystemVisitorMapEvent() {
        title = "Purple System Visitor";
        luckScore = 5;
        triggerKind = TriggerKind.OnSystemEntered;
    }

    public override bool Condition() { return false; }

    public override AbstractMapEvent Create(RandomEventContext ctx) {
        var e = new PurpleSystemVisitorMapEvent();

        e.text = MultilineText(@"
            You entered a region of space that isn't exactly a normal star system, but rather an anomaly.
            There are no planets here.
            
            Few minutes later, you hear the board systems going crazy.
            Something is moving towards your fleet, but with a very unusual pattern.
            
            It looks like a battle station of unknown origin.
        ");

        e.actions.Add(new Action {
            name = "Prepare for the worst",
            apply = () => {
                var v = VesselFactory.NewVessel(Faction.Neutral, "Visitor");
                var spaceUnit = NewSpaceUnit(Faction.RandomEventHostile, v);
                spaceUnit.cargo.power = (int)(ctx.roll * 120);
                return new Result {
                    text = MultilineText(@"
                        This thing doesn't respond to your communication attempts.
                        
                        As this platform closes the distance by every minute,
                        you try to calculate your chances.
                    "),
                    effects = {
                        new Effect{
                            kind = EffectKind.EnterArena,
                            value = spaceUnit,
                        },
                    },
                };
            },
        });

        return e;
    }
}
