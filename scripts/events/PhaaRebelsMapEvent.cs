using Godot;

public class PhaaRebelsMapEvent : AbstractMapEvent {
    public PhaaRebelsMapEvent() {
        title = "Phaa Rebels";
        luckScore = 5;
        triggerKind = TriggerKind.OnScript;
    }

    public override bool Condition() {
        return AtNeutralSystem() && CurrentStarSystem().id % 2 == 0;
    }

    public override AbstractMapEvent Create(RandomEventContext ctx) {
        var e = new PhaaRebelsMapEvent();

        var text = @"
            You have detected a strange group of Phaa vessels.

            Based on their coloring, you can tell that they're
            the rebels fleet you were ordered to destroy.

            They have 3 Spacehopper class vessels in their ranks.
        ";
        e.text = MultilineText(text);

        e.actions.Add(new Action {
            name = "Attack",
            apply = () => {
                var spaceUnit = NewSpaceUnit(Faction.RandomEventHostile,
                    VesselFactory.NewVessel(Faction.PhaaRebel, "Spacehopper"),
                    VesselFactory.NewVessel(Faction.PhaaRebel, "Spacehopper"),
                    VesselFactory.NewVessel(Faction.PhaaRebel, "Spacehopper"));
                spaceUnit.cargo.organic = (int)(ctx.roll * 40);
                spaceUnit.cargo.power = (int)(ctx.roll * 30);
                return new Result {
                    skipText = true,
                    effects = {
                        new Effect{
                            kind = EffectKind.EnterArena,
                            value = spaceUnit,
                        },
                    },
                };
            },
        });

        e.actions.Add(new Action {
            name = "Communicate",
            apply = () => {
                return new Result {
                    text = MultilineText(@"
                        Every asteroid that managed to get too close was shot
                        by the point-defense laser systems.

                        No damage taken.
                    "),
                    effects = {
                        new Effect{
                            kind = EffectKind.EnterTextQuest,
                            value = new PhaaRebelsTQuest(),
                        },
                    },
                    skipText = true,
                };
            },
        });

        return e;
    }
}
