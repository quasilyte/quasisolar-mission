using Godot;

public class GuardianCrashSiteMapEvent : AbstractMapEvent {
    public GuardianCrashSiteMapEvent() {
        title = "Guardian Crash Site";
        luckScore = 9;
        triggerKind = TriggerKind.OnSystemEntered;
    }

    // public override bool Condition() {
    //     return IsFirstSystemVisit() &&
    //         AtNeutralSystem() &&
    //         GameState().day > 200 &&
    //         GameState().StorageFreeSlot() != -1;
    // }

    public override AbstractMapEvent Create(RandomEventContext ctx) {
        var e = new GuardianCrashSiteMapEvent();

        e.text = MultilineText(@"
            It looks like there has been a Phaa-Wertu confrontation in this system recently.

            You pick up the two signals: one from the intact Phaa vessel and another one from
            the remainings of the Wertu Guardian vessel.

            The Guardian wreckage looks salvagable, but Phaa is about to erase all the traces
            of what happened here.

            You can interfere to collect the Guardian remains.
        ");

        e.actions.Add(new Action {
            name = "Intervene",
            apply = () => {
                var mantis = VesselFactory.NewVessel(Faction.Phaa, "Mantis");
                WeaponDesign item = (ctx.roll <= 0.5) ? PhotonBurstCannonWeapon.Design : TwinPhotonBurstCannonWeapon.Design;
                return new Result {
                    text = MultilineText($@"
                        All attempts to contact the Phaa vessel failed.
                        You launched a few warning shots to make it go away.

                        During that opening, you managed to extract the {item.name} weapon.

                        To your surprise, that vessel is coming back and
                        this time it won't leave any loose ends.
                    "),
                    expReward = 3,
                    effects = {
                        new Effect {
                            kind = EffectKind.AddItem,
                            value = item,
                        },
                        new Effect {
                            kind = EffectKind.EnterArena,
                            value = NewSpaceUnit(Faction.RandomEventHostile, mantis),
                        },
                    },
                };
            },
        });

        e.actions.Add(new Action {
            name = "Keep out of it",
            apply = () => {
                return new Result {
                    text = "The Phaa unit destroys the wreckage and warps away.",
                };
            },
        });

        return e;
    }
}
