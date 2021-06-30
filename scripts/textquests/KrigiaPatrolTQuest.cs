using System.Collections.Generic;

public class KrigiaPatrolTQuest : AbstractTQuest {
    public KrigiaPatrolTQuest() {
        var gameState = RpgGameState.instance;
        DeclareValue("Krigia reputation", () => gameState.reputations[Faction.Krigia], true);
        DeclareValue("Status", () => DiplomaticStatusString(gameState.diplomaticStatuses[Faction.Krigia]), true);
        DeclareValue("Credits", () => gameState.credits, true);
        DeclareValue("Minerals", () => gameState.humanUnit.Get().cargo.minerals, true);
        DeclareValue("Organic", () => gameState.humanUnit.Get().cargo.organic, true);
        DeclareValue("Power", () => gameState.humanUnit.Get().cargo.power, true);
        DeclareValue("Free cargo space", () => gameState.humanUnit.Get().CargoFree(), true);
    }

    public override TQuestCard GetFirstCard() {
        return DialogueRoot();
    }

    private string DiplomaticStatusString(DiplomaticStatus status) {
        if (status == DiplomaticStatus.War) {
            return "at war";
        }
        if (status == DiplomaticStatus.Alliance) {
            return "allies";
        }
        if (status == DiplomaticStatus.NonAttackPact) {
            return "non-aggression pact";
        }
        if (status == DiplomaticStatus.TradingAgreement) {
            return "trading agreement";
        }
        return "unspecified";
    }

    private TQuestCard DialogueRoot() {
        return new TQuestCard {
            image = "Krigia",
            text = ($@"
                You established a connection with a [u]Krigia patrol unit[/u].

                The fleet commander clearly doesn't look too chatty.

                - {GetGreetingsPhrase()}

                Since we're at war with this faction, you already know what's
                on their agenda (they're going to attack us).

                What exactly do we want to achieve here?
            "),
            actions = {
                new TQuestAction{
                    text = "Gossip.",
                    next = Gossip,
                },
                new TQuestAction{
                    text = "Propose a bribe to avoid a fight.",
                    next = Bribe,
                },
                new TQuestAction{
                    text = "Try to intimidate.",
                    next = Intimidate,
                },
                new TQuestAction{
                    text = "Ask questions.",
                },
                new TQuestAction{
                    text = "End conversation.",
                    next = () => TQuestCard.exitQuestEnterArena,
                }
            },
        };
    }

    private string GetIntimidationText() {
        return QRandom.Element(new List<string>{
            "If you attack us, we will destroy your fleet.",
            "This is a suicidal mission for you.",
            "The chance of your victory is negligible. Retreat while you can.",
            "You will never win this battle."
        });
    }

    private string GetIntimidationResponse() {
        return QRandom.Element(new List<string>{
            "This is not the first time we hear such statements. So far: we lived, the claimers did not.",
            "Pathetic.",
            "We calculated your fleet to be inferior to ours. Your statement doesn't hold.",
            "We have a chance to see if your fleet is as good as you claim it to be.",
            "Prepare to be destroyed.",
            "Prepare to be defeated.",
            "Prepare to be eradicated.",
            "Prepare to be annihilated.",
            "Even begging for surrender won't save your kind.",
            "Earthlings try to intimidate us? Fascinating.",
            "No, you will never win.",
        });
    }

    private TQuestCard IntimidationFailure() {
        return new TQuestCard {
            text = ($@"
                You tried to make your enemies flee the battlefield:

                - {GetIntimidationText()}

                - {GetIntimidationResponse()}

                Looks like your intimidation attempt failed.
            "),
            actions = {
                new TQuestAction{
                    text = "Prepare for battle.",
                    next = () => TQuestCard.exitQuestEnterArena,
                }
            },
        };
    }

    private TQuestCard Intimidate() {
        bool hasIntimidation = RpgGameState.instance.skillsLearned.Contains("Intimidation");
        float failChance = hasIntimidation ? 0.05f : 0.35f;
        if (QRandom.Float() < failChance) {
            return IntimidationFailure();
        }

        var alliedForces = (float)RpgGameState.instance.humanUnit.Get().FleetCost();
        var enemyForces = (float)RpgGameState.arenaUnit1.FleetCost();
        if (!hasIntimidation) {
            alliedForces *= 0.6f;
        }
        Godot.GD.Print(alliedForces + " vs " + enemyForces);
        if (alliedForces < enemyForces) {
            return IntimidationFailure();
        }

        return new TQuestCard {
            text = ($@"
                The combination of your fleet strength and your unprecedented
                intimidation session made your opponents tremble.

                Without saying much, their commander turns off the communication
                to calculate the risks.

                After a short period of time, you've noticed that they started
                to charge up the system jump.
            "),
            actions = {
                new TQuestAction{
                    text = "Leave.",
                    next = () => {
                        RpgGameState.transition = RpgGameState.MapTransition.EnemyUnitRetreats;
                        return TQuestCard.exitQuestEnterMap;
                    },
                }
            },
        };
    }

    private TQuestCard BribeFailure() {
        return new TQuestCard {
            text = ($@"
                - We're deaf to any pleas. You'll fight as any noble race should.

                Looks like your bribe attempt failed.
            "),
            actions = {
                new TQuestAction{
                    text = "Prepare for battle.",
                    next = () => TQuestCard.exitQuestEnterArena,
                }
            },
        };
    }

    private TQuestCard Bribe() {
        bool hasSpeaking = RpgGameState.instance.skillsLearned.Contains("Speaking");
        float failChance = hasSpeaking ? 0.1f : 0.2f;
        if (QRandom.Float() < failChance) {
            return BribeFailure();
        }

        int price = 1000 + QRandom.IntRange(0, 400);
        price += (int)(0.1f * RpgGameState.arenaUnit1.FleetCost());
        if (hasSpeaking) {
            price = (int)(0.55f * price);
        }
        return new TQuestCard {
            text = ($@"
                Krigia treats Earthlings as a vassal race, so they may accept
                credits and let us go.

                - {GetBribePhrase(price)}
            "),
            actions = {
                new TQuestAction{
                    text = "Pay the toll.",
                    cond = () => RpgGameState.instance.credits >= price,
                    next = () => {
                        RpgGameState.instance.credits -= price;
                        RpgGameState.transition = RpgGameState.MapTransition.EnemyUnitRetreats;
                        return TQuestCard.exitQuestEnterMap;
                    },
                },
                new TQuestAction{
                    text = "Decline.",
                    next = () => TQuestCard.exitQuestEnterArena,
                }
            },
        };
    }

    private string GetBribePhrase(int price) {
        return QRandom.Element(new List<string>{
            $"We'll leave this system for a small toll of {price} credits.",
            $"The price of your escape is {price} credits",
        });
    }

    private TQuestCard Gossip() {
        return new TQuestCard {
            text = ($@"
                You tried to trick them into giving you some extra information:

                - {GetGossipQuery()}

                - {GetGossip()}

                After saying these words, the Krigia commander ends the transmission.
                Some things never change.
            "),
            actions = {
                new TQuestAction{
                    text = "End conversation.",
                    next = () => TQuestCard.exitQuestEnterArena,
                }
            },
        };
    }

    private string GetGossipQuery() {
        return QRandom.Element(new List<string>{
            "We come in peace, your offence is not justified.",
            "We want to put an end to this conflict.",
        });
    }

    private string GetGossip() {
        var options = new List<string>();

        {
            var daysLeft = RpgGameState.instance.missionDeadline - RpgGameState.instance.day;
            options.Add($"This war will be over in [u]{daysLeft}[/u] days. Your resistance only postpones the inevitable. Pathetic.");
        }

        {
            StarBase starBase = null;
            foreach (var x in RpgGameState.humanBases) {
                if (x.discoveredByKrigia != 0) {
                    starBase = x;
                    break;
                }
            }
            if (starBase != null) {
                options.Add($"Your fleet is as doomed as a star base at the [u]{starBase.system.Get().name}[/u] system.");
            }
        }

        return QRandom.Element(options);
    }

    private string GetGreetingsPhrase() {
        return QRandom.Element(new List<string>{
            "I see an Earthling fleet, but not a single human is on board. Fascinating.",
            "It's been a while since I saw the Earthling forces flying around. Refreshing.",
            "An Earthling fleet without Wertu escort. Fascinating.",
            "We don't have any reasons to negotiate. Make it quick, Earthling.",
        });
    }
}
