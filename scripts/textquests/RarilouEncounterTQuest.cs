using System.Collections.Generic;

public class RarilouEncounterTQuest : AbstractTQuest {
    private RpgGameState _gameState;

    private bool _checkedQuest = false;
    private Quest.Template _rolledQuest = null;
    private Quest.Data _currentQuest = null;

    private string Currency() { return RpgGameState.alienCurrencyNames[Faction.Rarilou]; }
    private int Reputation() { return _gameState.reputations[Faction.Rarilou]; }
    private DiplomaticStatus Status() { return _gameState.diplomaticStatuses[Faction.Rarilou]; }

    public RarilouEncounterTQuest() {
        _gameState = RpgGameState.instance;
        DeclareValue("Rarilou reputation", () => Reputation(), true);
        DeclareValue("Status", () => DiplomaticStatusString(Status()), true);
        DeclareValue("RU", () => _gameState.credits, true);
        DeclareValue(Currency(), () => _gameState.alienCurrency[Faction.Rarilou], true);
        DeclareValue("Minerals", () => _gameState.humanUnit.Get().cargo.minerals, true);
        DeclareValue("Organic", () => _gameState.humanUnit.Get().cargo.organic, true);
        DeclareValue("Power", () => _gameState.humanUnit.Get().cargo.power, true);
        DeclareValue("Free cargo space", () => _gameState.humanUnit.Get().CargoFree(), true);

        foreach (var q in _gameState.activeQuests) {
            if (q.faction == Faction.Rarilou) {
                _currentQuest = q;
                break;
            }
        }
        if (_currentQuest == null) {
            RollNextQuest();
        }
    }

    private void RollNextQuest() {
        var availableQuests = new List<Quest.Template>();
        foreach (var tmpl in RarilouQuests.list) {
            if (_gameState.issuedQuests.Contains(tmpl.name)) {
                continue;
            }
            if (!tmpl.cond()) {
                continue;
            }
            availableQuests.Add(tmpl);
        }
        if (availableQuests.Count != 0) {
            _rolledQuest = QRandom.Element(availableQuests);
        }
    }

    public override TQuestCard GetFirstCard() {
        return new TQuestCard {
            image = "Rarilou",
            text = GetGreetingsText(),
            actions = GetRootActions(),
        };
    }

    private string GetGreetingsText() {
        if (Status() == DiplomaticStatus.War) {
            return (@"
                We don't want to attack your fleet, but if you will get
                any closer, we will have to open fire. Let us leave this
                system so every side can avoid unnecessary casualties.
            ");
        }
        if (Status() == DiplomaticStatus.Alliance) {
            return (@"
                Hail to the allied Earthling fleet.
                What are you up to, captain?
            ");
        }
        return (@"
            Greetings to the Earthlings.
            Our protocols require us to leave this system,
            but we are open to hear you out.
        ");
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
        return "unspecified";
    }

    private List<TQuestAction> GetRootActions() {
        var actions = new List<TQuestAction>();

        if (Status() != DiplomaticStatus.War) {
            actions.Add(new TQuestAction{
                text = "[Do business]",
                next = () => DoBusiness(null),
            });
        }

        actions.Add(new TQuestAction{
            text = "[Ask questions]",
            next = AskQuestions,
        });

        bool canAttack = Status() == DiplomaticStatus.War || Status() == DiplomaticStatus.Unspecified;
        if (canAttack) {
            actions.Add(new TQuestAction{
                text = "[Hostile actions]",
                next = HostileActions,
            });
        }

        actions.Add(new TQuestAction{
            text = "[End conversation]",
            next = () => TQuestCard.exitQuestEnterMap,
        });

        return actions;
    }

    private TQuestCard DialogueRoot() {
        return new TQuestCard {
            actions = GetRootActions(),
        };
    }

    private TQuestCard QuestComplete(Quest.Template template, Quest.CompletionData completion) {
        Quest.Complete(_gameState, completion);
        _gameState.completedQuests.Add(template.name);
        _gameState.activeQuests.Remove(_currentQuest);
        _currentQuest = null;
        RollNextQuest();
        var text = template.completionResponse(completion);
        text += "\n\n";
        foreach (var reward in completion.quest.rewards) {
            if (reward.kind == Quest.RewardKind.GetReputation) {
                text += $"> Got {reward.value} reputation points\\n";
            } else if (reward.kind == Quest.RewardKind.GetAlienCurrency) {
                text += $"> Received {reward.value} {Currency()}\\n";
            } else if (reward.kind == Quest.RewardKind.GetRU) {
                text += $"> Received {reward.value} RU\\n";
            } else if (reward.kind == Quest.RewardKind.GetTechnology) {
                text += $"> Got {reward.value} technology\\n";
            }
        }
        text += "\n";
        return new TQuestCard{
            // text = template.completionResponse(completion),
            text = text,
            actions = GetBusinessActions(),
        };
    }

    private TQuestCard QuestRequest(Quest.Template template) {
        _checkedQuest = true;
        var q = template.CreateQuestData();
        return new TQuestCard{
            text = template.issueText(q),
            actions = {
                new TQuestAction{
                    text = "Agreed.",
                    apply = () => {
                        _gameState.activeQuests.Add(q);
                        _currentQuest = q;
                    },
                    next = () => DoBusiness(template.acceptResponse(q)),
                },
                new TQuestAction{
                    text = "I will have to decline this request.",
                    next = () => DoBusiness("That is very unfortuitous."),
                },
            },
        };
    }

    private TQuestCard DoBusiness(string textOverride) {
        return new TQuestCard{
            text = textOverride,
            actions = GetBusinessActions(),
        };
    }

    private List<TQuestAction> GetBusinessActions() {
        var actions = new List<TQuestAction>();

        if (_currentQuest == null && _rolledQuest != null && !_checkedQuest) {
            actions.Add(new TQuestAction{
                text = "Can I be of any service?",
                apply = () => _gameState.issuedQuests.Add(_rolledQuest.name),
                next = () => QuestRequest(_rolledQuest),
            });
        } else if (_currentQuest != null) {
            var completion = Quest.CheckRequirements(_gameState, _currentQuest);
            var template = RarilouQuests.Find(_currentQuest.name);
            if (completion != null) {
                actions.Add(new TQuestAction{
                    text = template.completionPhrase(completion),
                    next = () => QuestComplete(template, completion),
                });
            }
        }

        if (Status() == DiplomaticStatus.Unspecified) {
            actions.Add(new TQuestAction{
                text = "I propose a pact of non-aggression.",
                next = TryNonAttackPact,
            });
        } else if (Status() == DiplomaticStatus.NonAttackPact) {
            actions.Add(new TQuestAction{
                text = "I propose to form an alliance.",
                next = TryAlliance,
            });
        }

        actions.Add(new TQuestAction{
            text = "We need your assistance.",
            next = AssistMenu,
        });

        actions.Add(new TQuestAction{
            text = "[Back]",
            next = DialogueRoot,
        });

        return actions;
    }

    private TQuestCard BuyMaterial() {
        _gameState.alienCurrency[Faction.Rarilou] -= 10;
        var amount = QRandom.IntRange(15, 35);
        _gameState.researchMaterial.rarilou += amount;
        return new TQuestCard{
            text = ($@"
                We'll transfer [u]{amount}[/u] units of our materials to your vessel immediately.

                This should help your scientists to develop the technologies similar to ours.

                Anything else?
            "),
            actions = GetAssistMenuActions(),
        };
    }

    private TQuestCard BuyVessel() {
        _gameState.alienCurrency[Faction.Rarilou] -= 70;
        
        var leviathan = RpgGameState.instance.NewVessel(Faction.Earthling, VesselDesign.Find("Scout"));
        leviathan.pilotName = PilotNames.UniqRarilouName(RpgGameState.instance.usedNames);
        VesselFactory.Init(leviathan, "Rarilou Leviathan");
        leviathan.isMercenary = true;
        _gameState.humanUnit.Get().fleet.Add(leviathan.GetRef());

        return new TQuestCard{
            text = ($@"
                We'll send you one of our Leviathan class vessels.

                Anything else?
            "),
            actions = GetAssistMenuActions(),
        };
    }

    private List<TQuestAction> GetAssistMenuActions() {
        var actions = new List<TQuestAction>();

        actions.Add(new TQuestAction{
            text = "We need your technologies. (10)",
            cond = () => _gameState.alienCurrency[Faction.Rarilou] >= 10,
            next = BuyMaterial,
        });

        actions.Add(new TQuestAction{
            text = "I want to recruit a vessel. (70)",
            cond = () => {
                if (Status() != DiplomaticStatus.Alliance) {
                    return false;
                }
                if (_gameState.humanUnit.Get().fleet.Count == 4) {
                    return false;
                }
                return _gameState.alienCurrency[Faction.Rarilou] >= 70;
            },
            next = BuyVessel,
        });

        actions.Add(new TQuestAction{
            text = "Nevermind.",
            next = GetFirstCard,
        });

        return actions;
    }

    private TQuestCard AssistMenu() {

        return new TQuestCard{
            text = (@"
                We're ready to offer you our services in exchange
                for the Rosy Crystals.

                So, what is it you need?
            "),
            actions = GetAssistMenuActions(),
        };

    }

    private TQuestCard TryNonAttackPact() {
        if (Reputation() < 5) {
            return new TQuestCard{
                text = (@"
                    I am afraid we have to decline your offer.

                    We don't trust you enough to sign it.
                "),
                actions = GetBusinessActions(),
            };
        }
        _gameState.diplomaticStatuses[Faction.Rarilou] = DiplomaticStatus.NonAttackPact;
        return new TQuestCard{
            text = (@"
                So be it!

                People of Rarilou and Earthlings shall not attack
                each other anymore.
            "),
            actions = GetBusinessActions(),
        };
    }

    private TQuestCard TryAlliance() {
        if (Reputation() < 20) {
            return new TQuestCard{
                text = (@"
                    Forming an allience is a serious commitment.
                    It implies a trust between our races.

                    At this point, we're not ready to make this step.
                "),
                actions = GetBusinessActions(),
            };
        }
        _gameState.diplomaticStatuses[Faction.Rarilou] = DiplomaticStatus.Alliance;
        return new TQuestCard{
            text = (@"
                Our races had several successful interactions in the past,
                so we see a value in forming an alliance given the
                events that are unfolding in this part of the galaxy.

                You should not expect a lot of firepower help from us,
                but we can help you with technologies as well as information
                we gather all around the space.
            "),
            actions = GetBusinessActions(),
        };
    }

    private TQuestCard AskQuestions() {
        // TODO.
        return new TQuestCard {
            actions = GetRootActions(),
        };
    }

    private TQuestCard HostileActions() {
        return new TQuestCard{
            actions = {
                new TQuestAction{
                    text = "[Attack]",
                    next = () => {
                        RpgGameState.instance.reputations[Faction.Rarilou] -= 5;
                        RpgGameState.instance.diplomaticStatuses[Faction.Rarilou] = DiplomaticStatus.War;
                        return TQuestCard.exitQuestEnterArena;
                    },
                },
                new TQuestAction{
                    text = "[Back]",
                    next = DialogueRoot,
                },
            },
        };
    }
}
