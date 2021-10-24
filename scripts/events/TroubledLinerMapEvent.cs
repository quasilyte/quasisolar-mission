using System;
using System.Collections.Generic;

public class TroubledLinerMapEvent: AbstractMapEvent {
    public TroubledLinerMapEvent() {
        title = "Troubled Liner";
        luckScore = 7;
        triggerKind = TriggerKind.OnSystemEntered;
    }

    public override bool Condition() {
        return AtNeutralSystem();
    }

    public override AbstractMapEvent Create(RandomEventContext ctx) {
        var e = new TroubledLinerMapEvent();

        e.text = MultilineText(@"
            You received a message from a vessel.
            
            `This is a non-military Wertu transporter.
            Our batteries are exhausted and we can't charge the engine to leave this system.
            Please help.`
        ");

        Func<float, string, bool, Result> rewardForHelping = (float roll, string headline, bool spendEnergy) => {
            var text = headline + "\n\n";
            var effects = new List<Effect>();
            if (roll < 0.4) {
                effects.Add(new Effect{
                    kind = EffectKind.AddOrganic,
                    value = 90,
                });
                text += "In reward, they shared some of their transported goods worth 90 units of organic.\n";
            } else if (roll < 0.8) {
                effects.Add(new Effect{
                    kind = EffectKind.AddCredits,
                    value = 2700,
                });
                text += "In reward, they paid 2700 RU for your assistance.\n";
            } else {
                effects.Add(new Effect{
                    kind = EffectKind.AddReputation,
                    value = 3,
                    value2 = Faction.Wertu,
                });
                text += "The transport crew gives you the infinite gratitude, but they don't have anything valuable to share.\n";
                text += "\n";
                text += "The vessel captain promised to spread a word about you. This may affect your Wertu repuration in a positive way.\n";
            }
            if (spendEnergy) {
                effects.Add(new Effect{
                    kind = EffectKind.SpendAnyVesselBackupEnergy,
                    value = 40,
                });
            }
            return new Result{
                text = MultilineText(text),
                expReward = 2,
                effects = effects,
            };
        };

        e.actions.Add(new Action{
            name = "Transfer the energy",
            condition = () => {
                foreach (var v in PlayerSpaceUnit().fleet) {
                    if (v.Get().energy >= 40) {
                        return true;
                    }
                }
                return false;
            },
            apply = () => {
                return rewardForHelping(ctx.roll, "You transfered 40 units of energy to them.", true);
            },
        });

        e.actions.Add(new Action{
            name = "Attack the transport",
            apply = () => {
                var liner = VesselFactory.NewVessel(Faction.Wertu, "Transporter");
                var spaceUnit = NewSpaceUnit(Faction.RandomEventHostile, liner);
                if (ctx.roll < 0.6) {
                    spaceUnit.cargo.organic = (int)((ctx.roll + 0.6f) * 150);
                }
                return new Result{
                    text = MultilineText(@"
                        Pirates or Draklids may get to them at some point, so why not take the scraps yourself?
                        
                        Undoubtedly, this will affect your Wertu reputation.
                    "),
                    effects = {
                        new Effect{
                            kind = EffectKind.EnterArena,
                            value = spaceUnit,
                        },
                        new Effect{
                            kind = EffectKind.AddReputation,
                            value = -2,
                            value2 = Faction.Wertu,
                        },
                    },
                };
            },
        });

        e.actions.Add(new Action{
            name = "Ignore the call",
            apply = () => {
                return new Result{
                    text = "You can't help to everyone; more over, you have a mission to accomplish.",
                };
            }
        });

        return e;
    }
}
