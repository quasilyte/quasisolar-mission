using System.Collections.Generic;

public class PhaaIntroTQuest : AbstractDiplomacyTQuest {
    public PhaaIntroTQuest(): base(Faction.Phaa) {
        _gameState = RpgGameState.instance;
        DeclareValue("Phaa reputation", () => GetReputation(), true);
        DeclareValue("Status", () => DiplomaticStatusString(GetStatus()), true);
    }

    public override TQuestCard GetFirstCard() {
        return new TQuestCard {
            image = "Phaa",
            text = (@"
                Attention, unidentified fleet!
                You've entered Phaa territory.
                
                Our scanners can't detect any life forms on board,
                yet the signal is coming from your vessel.

                What exactly are you and why are you contacting us?
            "),
            actions = {
                new TQuestAction{
                    text = "I was built to represent the humans of the Earth. We come in peace.",
                    next = ComeInPeaceCard,
                },
            },
        };
    }

    private TQuestCard ComeInPeaceCard() {
        return new TQuestCard {
            image = "Phaa",
            text = (@"
                Humans from the planet Earth.

                This is our first official contact, humans.
                We heard of Earthlings that helped Wertu to fight
                against Krigia. We never saw an actual human nonetheless.

                Are you aware that Wertu is our enemy?
                Do Earthlings still fight on their side?
            "),
            actions = {
                new TQuestAction{
                    text = "We're building an alliance with them.",
                    apply = () => DoChangeReputation(-4),
                    next = AlliedWertuCard,
                },
                new TQuestAction{
                    text = "Our terms with them are neutral.",
                    cond = () => _gameState.diplomaticStatuses[Faction.Wertu] != DiplomaticStatus.Alliance,
                    next = PhaaStoryCard,
                },
                new TQuestAction{
                    text = "We're at war with them.",
                    cond = () => _gameState.diplomaticStatuses[Faction.Wertu] == DiplomaticStatus.War,
                    apply = () => DoChangeReputation(+5),
                    next = PhaaStoryCard,
                },
            },
        };
    }

    private TQuestCard AlliedWertuCard() {
        DoDeclareWar();
        return new TQuestCard{
            text = (@"
                This means that you're our enemy as well.
            "),
            actions = {
                new TQuestAction{
                    text = "[End conversation]",
                    next = () => TQuestCard.exitQuestEnterMap,
                },
            }
        };
    }

    private TQuestCard PhaaStoryCard() {
        return new TQuestCard{
            text = (@"
                We lost our homeworld due to the Wertu war.

                Many of our fleets were destroyed during that time.
                When the Krigia came, we had almost no chances to
                stand our ground. We had to make a tough decision of
                leaving our only star system in favor of a nomadic life style.

                This way, they can't coordinate their attacks and destroy
                our main ark ship.

                Now, let us repeat our question once more:
                why are you contacting us?
            "),
            actions = {
                new TQuestAction{
                    text = "We want to build an alliance against our mutual foes.",
                    apply = () => DoSignNonAttackPact(),
                    next = BuildAllianceCard,
                },
                new TQuestAction{
                    text = "We seek help in our war against Krigia and its allies.",
                    apply = () => DoChangeReputation(+1),
                    next = SeekHelpCard,
                },
                new TQuestAction{
                    text = "We're here to declare a war.",
                    next = DeclareWarCard,
                },
            },
        };
    }

    private TQuestCard BuildAllianceCard() {
        if (_gameState.diplomaticStatuses[Faction.Wertu] == DiplomaticStatus.Alliance) {
            DoChangeReputation(-2);
            return new TQuestCard{
                text = (@"
                    We would never build an alliance with someone who sides with Wertu.

                    You should leave, until we change our mind and attack you right away.
                "),
                actions = {
                    new TQuestAction{
                        text = "[End conversation]",
                        next = () => TQuestCard.exitQuestEnterMap,
                    },
                }
            };
        }

        return new TQuestCard{
            text = (@"
                We can start from a pact of non-aggression.

                We're currently at war with Wertu, Krigia and Draklid factions.
                As long as you don't side with one of these, we can play along.
            "),
            actions = {
                new TQuestAction{
                    text = "[End conversation]",
                    next = () => TQuestCard.exitQuestEnterMap,
                },
            }
        };
    }  

    private TQuestCard SeekHelpCard() {
        return new TQuestCard{
            text = (@"
                We can't help you with that, Earthlings,
                but we appreciate your honesty.

                Perhaps we should meet again in the future.
            "),
            actions = {
                new TQuestAction{
                    text = "[End conversation]",
                    next = () => TQuestCard.exitQuestEnterMap,
                },
            }
        };
    }    

    private TQuestCard DeclareWarCard() {
        DoDeclareWar();
        DoChangeReputation(-3);

        return new TQuestCard{
            text = (@"
                War it is then.

                Now leave this system or else your fleet will be destroyed.
            "),
            actions = {
                new TQuestAction{
                    text = "[End conversation]",
                    next = () => TQuestCard.exitQuestEnterMap,
                },
            }
        };
    }
}
