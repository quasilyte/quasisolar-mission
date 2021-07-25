using Godot;

public class AsteroidsMapEvent : AbstractMapEvent {
    public AsteroidsMapEvent() {
        title = "Asteroids";
        luckScore = 3;
        triggerKind = TriggerKind.OnSystemEntered;
    }

    public override AbstractMapEvent Create(RandomEventContext ctx) {
        var e = new AsteroidsMapEvent();

        var text = @"
            You've entered the system and got right into the asteroids hell.
            
            It looks like it will be very hard to get out unscratched.
        ";
        bool hasPointLaser = HasWeapon(PointDefenseLaserWeapon.Design);
        if (hasPointLaser) {
            text += "\n(Point-Defense Laser) You can guard against asteroids using your point-defense system.\n";
        }
        e.text = MultilineText(text);

        e.actions.Add(new Action {
            name = "Take the hit",
            apply = () => {
                return new Result {
                    text = "Although you took some damage, every vessel from your fleet made it through.",
                    expReward = 4,
                    effects = {
                        new Effect {
                            kind = EffectKind.DamageFleetPercentage,
                            value = new Vector2(0.05f, 0.15f),
                        },
                    },
                };
            },
        });

        e.actions.Add(new Action {
            name = "Use point-defense laser",
            condition = () => hasPointLaser, 
            apply = () => {
                return new Result {
                    text = MultilineText(@"
                        Every asteroid that managed to get too close was shot
                        by the point-defense laser systems.

                        No damage taken.
                    "),
                    expReward = 6,
                };
            },
        });

        e.actions.Add(new Action {
            name = "Warp away",
            hint = () => "(70 fuel)",
            condition = () => GameState().fuel >= 70,
            apply = () => {
                return new Result {
                    text = MultilineText(@"
                        To avoid a risk of being hit, you performed an emergency warp jump.

                        The fleet took no damage at the cost of 70 fuel units.
                    "),
                    expReward = 3,
                    effects = {
                        new Effect {
                            kind = EffectKind.AddFuel,
                            value = -70,
                        },
                    },
                };
            },
        });

        return e;
    }
}
