using Godot;

public class LurkingThreatMapEvent: AbstractMapEvent {
    public LurkingThreatMapEvent() {
        title = "Lurking Threat";
        luckScore = 5;
        triggerKind = TriggerKind.OnSystemEntered;
    }

    public override bool Condition() {
        return GameState().day >= 500 && DarkBeaconBearer() != null &&
            CurrentStarSystem().color == StarColor.Red &&
            EventHappened("Rift Ambush");
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
        var e = new LurkingThreatMapEvent();

        System.Action extraReward = () => {
            var ru = QRandom.IntRange(6000, 10000);
            ArenaSettings.extraReward = (BattleResult result) => {
                result.ru = ru;
                result.popupText = MultilineText(@"
                    The enemies are defeated once again.

                    Should we get rid of that dark beacon now?
                ");
            };
        };

        e.text = MultilineText($@"
            The dark beacon is active again.

            You scanned the area and detected three vessels of familiar kind.
            The same kind that attacked you previously.

            This time we have some distance and can try to escape the combat.
        ");

        e.actions.Add(new Action{
            name = "Attack",
            apply = () => {
                return new Result {
                    skipText = true,
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

        e.actions.Add(new Action{
            name = "Evade the combat",
            apply = () => {
                return new Result{
                    text = MultilineText(@"
                        You successfully avoided the battle.

                        Should we get rid of that dark beacon?
                    "),
                };
            }
        });

        return e;
    }

    private SpaceUnit GetRavagersUnit() {
        var u = NewSpaceUnit(Faction.RandomEventHostile,
            VesselFactory.NewVessel(Faction.Neutral, "Ravager", 3),
            VesselFactory.NewVessel(Faction.Neutral, "Ravager", 2),
            VesselFactory.NewVessel(Faction.Neutral, "Ravager", 1));
        u.cargo.power = QRandom.IntRange(40, 70);
        return u;
    }
}
