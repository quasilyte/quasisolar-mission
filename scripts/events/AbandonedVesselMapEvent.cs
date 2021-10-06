using Godot;

public class AbandonedVesselMapEvent : AbstractMapEvent {
    public AbandonedVesselMapEvent() {
        title = "Abandoned Vessel";
        luckScore = 5;
        triggerKind = TriggerKind.OnSystemEntered;
    }

    public override bool Condition() {
        return !AtPlayerSystem();
    }

    public override AbstractMapEvent Create(RandomEventContext ctx) {
        var e = new AbandonedVesselMapEvent();

        e.text = MultilineText(@"
            You detect a small vessel that free floats the space.
            
            Scanning shows no signs of life on board.
            It should be possible to extract some fuel out of it.
            
            Should we fly to it and scavenge what we can?
        ");

        e.actions.Add(new Action {
            name = "Collect resources",
            apply = () => {
                if (ctx.roll < 0.6) {
                    var ru = (int)(ctx.roll * 750) + 300;
                    var fuel = (int)((1 - ctx.roll) * 100) + 5;
                    return new Result {
                        text = MultilineText($@"
                            The vessel was pretty much empty, but you managed to get some scraps out.

                            Received {ru} resource units.\n
                            Received {fuel} fuel.
                        "),
                        expReward = 5,
                        effects = {
                            new Effect{
                                kind = EffectKind.AddCredits,
                                value = ru,
                            },
                            new Effect{
                                kind = EffectKind.AddFuel,
                                value = fuel,
                            },
                        },
                    };
                }
                var v = VesselFactory.NewVessel(Faction.Zyth, "Hunter");
                return new Result {
                    text = MultilineText(@"
                        The abandoned vessel was a Zyth trick!
                        
                        We're ambushed, prepare for battle!
                    "),
                    expReward = 3,
                    effects = {
                        new Effect{
                            kind = EffectKind.EnterArena,
                            value = NewSpaceUnit(Faction.RandomEventHostile, v),
                        },
                    },
                };
            },
        });

        e.actions.Add(new Action {
            name = "Carry on",
            apply = () => {
                return new Result {
                    text = "It doesn't look good, we should probably fly by.",
                    expReward = 2,
                };
            },
        });

        return e;
    }
}
