using Godot;

public class EarthlingScoutMapEvent: AbstractMapEvent {
    public EarthlingScoutMapEvent() {
        title = "Earthling Scout";
        luckScore = 6;
        triggerKind = TriggerKind.OnSystemEntered;
    }

    public override bool Condition() {
        return IsFirstSystemVisit() && PlayerSpaceUnit().fleet.Count < SpaceUnit.maxFleetSize;
    }

    public override AbstractMapEvent Create(RandomEventContext ctx) {
        var e = new EarthlingScoutMapEvent();

        e.text = MultilineText(@"
            An automated Earthling scout drifts across this system.
            
            It's probably a war relict forgotten by everyone.
        ");

        e.actions.Add(new Action{
            name = "Capture the vessel",
            apply = () => {
                var scout = RpgGameState.instance.NewVessel(Faction.Earthling, VesselDesign.Find("Scout"));
                scout.pilotName = PilotNames.UniqHumanName(RpgGameState.instance.usedNames);
                VesselFactory.Init(scout, "Earthling Scout");
                scout.hp = 10;
                scout.energy = 0;

                return new Result {
                    text = MultilineText(@"
                        As you tried to approach it, it opened fire.

                        While you succeded in your mission to capture the vessel,
                        your fleet suffered some minor damage.
                        
                        The scout hull was heavily damaged and the batteries are completely dried out.
                    "),
                    expReward = 4,
                    effects = {
                        new Effect{
                            kind = EffectKind.DamageFleetPercentage,
                            value = new Vector2(0.05f, 0.1f),
                        },
                        new Effect{
                            kind = EffectKind.AddVesselToFleet,
                            value = scout,
                        },
                    },
                };
            }
        });

        e.actions.Add(new Action{
            name = "Ignore the scout",
            apply = () => {
                return new Result{
                    text = "Today you give a zero fox about the unidentified scout vessels.",
                    expReward = 2,
                };
            }
        });

        return e;
    }
}
