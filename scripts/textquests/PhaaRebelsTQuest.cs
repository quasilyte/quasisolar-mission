using System.Collections.Generic;

public class PhaaRebelsTQuest : AbstractDiplomacyTQuest {
    public PhaaRebelsTQuest(): base(Faction.PhaaRebel) {
        DeclareValue("Phaa Rebels reputation", () => GetReputation(), true);
    }

    public override TQuestCard GetFirstCard() {
        return DialogueRootCard(true);
    }

    private TQuestCard DialogueRootCard(bool canIntroduce) {
        var actions = new List<TQuestAction>();
        if (canIntroduce) {
            actions.Add(new TQuestAction{
                text = "Greetings. We're an exploration group sent from Quasisol. We come in peace.",
                apply = () => DoChangeReputation(+1),
                next = () => DialogueRootCard(false),
            });
            actions.Add(new TQuestAction{
                text = "I am here to destroy your outlaw unit.",
                apply = () => DoChangeReputation(-1),
                next = EngageCard,
            });
        }

        var text = (@"
            I presume you're one of the automated agents sent by
            the Earthlings during the war.

            Your race resembles ours in a way.
            You never physically left your homeworld, but due to a different
            reasons of course.

            We tried not to be discovered. The fact that you did discover
            us implies that you were specifically looking for us.
            Why were you searching for us?
        ");
        if (!canIntroduce) {
            text = (@"
                Greetings, visitors from the Quasisol system.

                I would like to avoid redundant communications
                as we're expecting our Phaa dictators retaliation.
            ");
        }

        actions.Add(new TQuestAction{
            text = "Why Phaa commander seeks your destruction?",
            next = WhyCommanderHuntsRebelsCard,
        });

        return new TQuestCard {
            image = "Phaa",
            text = text,
            actions = actions,
        };
    }

    private TQuestCard WhyCommanderHuntsRebelsCard() {
        return new TQuestCard{
            text = (@"
                The Phaa society has a long history of tension.

                The way we were solving it for a long time is our
                loyalty oath that forces everyone to follow the only
                approved path. Our main culture is very binary due to
                that fact. There is always one direction that is right
                while all other possibilities are considered to be
                incorrect or irrelevant. That culture is what kept
                us from populationg the other planets.

                Commander wants to punish the ones that tried to
                escape the price of breaking the oath.

                What we want is freedom and more flexible model of society.
            "),
            actions = {
                new TQuestAction{
                    text = "Tell me more about the Phaa culture.",
                    next = TellMeMoreCard,
                },
                new TQuestAction{
                    text = "Commander sent us to destroy your group.",
                    next = ChoiceCard,
                },
            },
        };
    }

    private TQuestCard TellMeMoreCard() {
        return new TQuestCard{
            text = (@"
                The ruling classes that have the most power enforce
                our political system by controlling everything they can.

                Many members of our society value our history and
                thrive to keep our identity even in these times.
                They want to be isolated in a sense, so other factions
                can't make them change in ways they may not immediately appreciate.
                This same fear of changes makes it easy for the dictators
                to forbid any worlds colonization as it could lead to
                a more diversity between the worlds, leading to a
                less coherent identity of the race.

                For them, populating new worlds is problematic.
                It means that it will be harder for them to keep
                everything under their direct influence.
            "),
            actions = {
                new TQuestAction{
                    text = "Commander sent us to destroy your group.",
                    next = ChoiceCard,
                },
            },
        };
    }

    private TQuestCard ChoiceCard() {
        return new TQuestCard{
            text = (@"
                We suspected as much. But he have an offer too.

                Instead of attacking us, you can accept our will
                to be free. We can even give you something in return for that.

                If you like our vessels design, I can give you the
                schematics so you can build them on your star bases.

                Or we can give you 3 Bubble Gun weapons for free.

                What do you think, captain?
            "),
            actions = {
                new TQuestAction{
                    text = "You don't need to give anything to us. I wish you good luck.",
                    apply = () => DoChangeReputation(+3),
                    next = PeacefulEndCard,
                },
                new TQuestAction{
                    text = "I accept your offer. (Take Spacehopper vessel design schematics)",
                    apply = () => _gameState.technologiesResearched.Add("Spacehopper"),
                    next = PeacefulEndCard,
                },
                new TQuestAction{
                    text = "I accept your offer. (Take 3 Bubble gun weapons)",
                    cond = () => _gameState.StorageFleeSlotsNum() >= 3,
                    apply = () => {
                        for (int i = 0; i < 3; i++) {
                            _gameState.PutItemToStorage(BubbleGunWeapon.Design);
                        }
                    },
                    next = PeacefulEndCard,
                },
                new TQuestAction{
                    text = "I am going to finish my mission.",
                    next = EngageCard,
                },
            },
        };
    }

    private TQuestCard PeacefulEndCard() {
        DoChangeReputation(+1);

        _gameState.activeQuests.Remove(_gameState.activeQuests.Find((x) => x.name == "Phaa Rebels"));
        _gameState.reputations[Faction.Phaa] -= 3;
        _gameState.diplomaticStatuses[Faction.Phaa] = DiplomaticStatus.War;

        return new TQuestCard{
            text = (@"
                I was hoping you would say that.

                Captain, I am sure that this is not our last encounter.

                Until then.
            "),
            actions = {
                new TQuestAction{
                    text = "[End conversation]",
                    next = () => TQuestCard.exitQuestEnterMap,
                },
            },
        };
    }

    private TQuestCard EngageCard() {
        return new TQuestCard{
            text = (@"
                So it comes down to this anyway.

                We expected a different behavior from your kind,
                given that you're not constrained by the Phaa oath.
            "),

            actions = {
                new TQuestAction{
                    text = "[Attack]",

                    next = () => {
                        var unit = GetRebelsUnit();
                        var humanUnit = RpgGameState.instance.humanUnit.Get();
                        RpgGameState.arenaUnit1 = unit;
                        ArenaManager.SetArenaSettings(GetCurrentStarSystem(), unit.fleet, humanUnit.fleet);
                        return TQuestCard.exitQuestEnterArena;
                    },
                },
            },
        };
    }

    private SpaceUnit GetRebelsUnit() {
        return ArenaManager.NewSpaceUnit(Faction.RandomEventHostile,
            VesselFactory.NewVessel(Faction.PhaaRebel, "Spacehopper"),
            VesselFactory.NewVessel(Faction.PhaaRebel, "Spacehopper"),
            VesselFactory.NewVessel(Faction.PhaaRebel, "Spacehopper"));
    }
}
