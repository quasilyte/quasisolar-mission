using Godot;

public class RiftAmbushMapEvent: AbstractMapEvent {
    public RiftAmbushMapEvent() {
        title = "Rift Ambush";
        luckScore = 5;
        triggerKind = TriggerKind.OnSystemEntered;
    }

    public override bool Condition() {
        return GameState().day >= 400 && DarkBeaconBearer() != null &&
            CurrentStarSystem().color == StarColor.Red &&
            EventHappened("Beacon Activity");
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
        var e = new RiftAmbushMapEvent();

        System.Action extraReward = () => {
            ArenaSettings.extraReward = (BattleResult result) => {
                result.popupText = MultilineText(@"
                    Quite unexpectedly, the dark beacon started to go crazy.
                    Can it be related with what just happened?

                    Could it the reason someone found and attacked you?
                ");
                result.research = "Tempest";
            };
        };

        e.text = MultilineText($@"
            A group of vessels enters the system through the
            portal of unknown technology. The portal is opened right next to your fleet.

            The nav tracers predict their movement towards our direction.
            If their intentions are hostile, we will be unable to avoid the battle.
        ");

        e.actions.Add(new Action{
            name = "See what happens next",
            apply = () => {
                return new Result {
                    text = MultilineText($@"
                        The opposing group starts the attack.

                        It's certain that you have to do the same.
                        Prepare to fight back.
                    "),
                    effects = {
                        new Effect{
                            kind = EffectKind.EnterArena,
                            value = GetRavagersUnit(),
                            fn = extraReward,
                        },
                    },
                };
            }
        });

        return e;
    }

    private SpaceUnit GetRavagersUnit() {
        return NewSpaceUnit(Faction.RandomEventHostile,
            VesselFactory.NewVessel(Faction.Neutral, "Ravager", 3),
            VesselFactory.NewVessel(Faction.Neutral, "Ravager", 1));
    }
}
