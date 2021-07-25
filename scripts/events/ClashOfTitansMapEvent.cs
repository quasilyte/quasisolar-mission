using Godot;
using System;

public class ClashOfTitansMapEvent: AbstractMapEvent {
    public ClashOfTitansMapEvent() {
        title = "Clash of Titans";
        luckScore = 5;
        triggerKind = TriggerKind.OnSystemEntered;
    }

    public override bool Condition() {
        return !AtPlayerSystem() && GameState().day >= 600;
    }

    public override AbstractMapEvent Create(RandomEventContext ctx) {
        var e = new ClashOfTitansMapEvent();

        Func<Faction, SpaceUnit> createWertuUnit = (Faction faction) => {
            var v1 = VesselFactory.NewVessel(Faction.Wertu, "Dominator");
            v1.spawnPos = new Vector2(1100, 200);
            var v2 = VesselFactory.NewVessel(Faction.Wertu, "Guardian");
            v2.spawnPos = new Vector2(1220, 140);
            var v3 = VesselFactory.NewVessel(Faction.Wertu, "Angel");
            v3.spawnPos = new Vector2(1050, 160);
            return NewSpaceUnit(faction, v1, v2, v3);
        };

        Func<SpaceUnit> createKrigiaUnit = () => {
            var v1 = VesselFactory.NewVessel(Faction.Krigia, "Horns");
            v1.spawnPos = new Vector2(1150, 800);
            var v2 = VesselFactory.NewVessel(Faction.Krigia, "Tusks");
            v2.spawnPos = new Vector2(1250, 900);
            var v3 = VesselFactory.NewVessel(Faction.Krigia, "Claws");
            v3.spawnPos = new Vector2(1320, 850);
            var v4 = VesselFactory.NewVessel(Faction.Krigia, "Claws");
            v4.spawnPos = new Vector2(1100, 820);
            return NewSpaceUnit(Faction.RandomEventHostile, v1, v2, v3, v4);
        };

        e.text = MultilineText(@"
            Two fleets are joined in the battle dance in this system.
            
            You can join the Wertu side and help them defeat the Krigia forces.
            Or you could side with no one and collect more valuable debris...
            
            (Krigia units will attack you even if you'll try to help them.
        ");

        e.actions.Add(new Action{
            name = "Join the Wertu side",
            apply = () => {
                return new Result{
                    text = MultilineText(@"
                        Your fleet enters the fray.
                        
                        Help Wertu to win in this battle.
                    "),
                    expReward = 6,
                    effects = {
                        new Effect{
                            kind = EffectKind.AddReputation,
                            value = +3,
                            value2 = Faction.Wertu,
                        },
                        new Effect{
                            kind = EffectKind.EnterArena,
                            value = createWertuUnit(Faction.RandomEventAlly),
                            value2 = createKrigiaUnit(),
                        },
                    },
                };
            }
        });

        e.actions.Add(new Action{
            name = "Attack everyone",
            apply = () => {
                return new Result{
                    text = MultilineText(@"
                        Your fleet enters the fray.
                        
                        Destroy all alien vessels to claim the victory.
                    "),
                    expReward = 3,
                    effects = {
                        new Effect{
                            kind = EffectKind.AddReputation,
                            value = -1,
                            value2 = Faction.Wertu,
                        },
                        new Effect{
                            kind = EffectKind.EnterArena,
                            value = createWertuUnit(Faction.RandomEventHostile2),
                            value2 = createKrigiaUnit(),
                        },
                    },
                };
            }
        });

        e.actions.Add(new Action{
            name = "Wait for the one side to win",
            apply = () => {
                var v1 = VesselFactory.NewVessel(Faction.Krigia, "Claws");
                var v2 = VesselFactory.NewVessel(Faction.Krigia, "Claws");
                return new Result{
                    text = MultilineText(@"
                        Krigia got an upper hand and destroyed all opposing forces.

                        Although greatly damaged, that surviving fleet turns towards your direction.
                        
                        Prepare for battle.
                    "),
                    expReward = 3,
                    effects = {
                        new Effect{
                            kind = EffectKind.EnterArena,
                            value = NewSpaceUnit(Faction.RandomEventHostile, v1, v2),
                        },
                    },
                };
            }
        });

        e.actions.Add(new Action{
            name = "Keep out of it",
            apply = () => {
                return new Result{
                    expReward = 2,
                    text = "Your fleet successfully avoided the confrontation.",
                };
            }
        });

        return e;
    }
}
