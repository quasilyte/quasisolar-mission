public class TreasureVaultMapEvent: AbstractMapEvent {
    public TreasureVaultMapEvent() {
        title = "A Treasure Vault";
        luckScore = 8;
        triggerKind = TriggerKind.OnSystemEntered;
    }

    public override bool Condition() {
        return !AtPlayerSystem();
    }

    public override AbstractMapEvent Create(RandomEventContext ctx) {
        var e = new TreasureVaultMapEvent();

        e.text = MultilineText(@"
            It's your lucky day!

            You found an open vault on a surface of one of the moons.
            You can scavenge all the resources from there.

            One central door is closed. You can try blasting that door open, but that could trigger the alarm which
            may or may not lead to complications that will make it impossible to collect any resources.

            Are you playing safe or taking a risk in order to get it all?
        ");

        e.actions.Add(new Action{
            name = "Play it safe",
            apply = () => {
                var bounty = 2000 + QRandom.IntRange(0, 300);
                return new Result{
                    text = MultilineText($@"
                        You harvested all the resources that were reachable through the open areas.
                        
                        As a result, you loaded {bounty} resource units on board of your flagship.
                    "),
                    expReward = 3,
                    effects = {
                        new Effect{
                            kind = EffectKind.AddCredits,
                            value = bounty,
                        },
                    },
                };
            }
        });

        e.actions.Add(new Action{
            name = "Take a risk",
            apply = () => {
                if (ctx.roll < 0.5) {
                    return new Result{
                        expReward = 5,
                        text = MultilineText(@"
                            As soon as you started to interact with the door, an emergency
                            power system kicked in. The floor trembled.

                            You managed to detect the self-destruct like program running
                            in the core of the vault system in time.

                            No resources were salvaged.
                        "),
                    };
                }
                var bounty = 4000 + QRandom.IntRange(0, 600);
                return new Result{
                    text = MultilineText($@"
                        The door took a few shots before it finally gave up.

                        Behind that door lies second storage which you emptied in addition to the first one.
                        The bounty is basically doubled!
                        
                        As a result, you loaded {bounty} resource units on board of your flagship.
                    "),
                    expReward = 3,
                    effects = {
                        new Effect{
                            kind = EffectKind.AddCredits,
                            value = bounty,
                        },
                    },
                };
            }
        });

        e.actions.Add(new Action{
            name = "Ignore the vault",
            apply = () => {
                return new Result{
                    text = MultilineText(@"
                        This could very well be dangerous even if we do it in a safer way.
                        To keep the crew intact, you decided to leave the vault undisturbed.
                    "),
                    expReward = 7,
                };
            }
        });

        return e;
    }
}
