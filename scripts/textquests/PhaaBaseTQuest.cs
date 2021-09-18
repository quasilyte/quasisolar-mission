using System.Collections.Generic;
using System;

public class PhaaBaseTQuest : AbstractDiplomacyTQuest {
    private bool _checkedQuest = false;
    private Quest.Template _rolledQuest = null;
    private Quest.Data _currentQuest = null;

    public PhaaBaseTQuest(): base(Faction.Phaa) {
        DeclareValue("Phaa reputation", () => GetReputation(), true);
        DeclareValue("Status", () => DiplomaticStatusString(GetStatus()), true);
        DeclareValue("RU", () => _gameState.credits, true);
        DeclareValue(GetCurrencyName(), () => GetCurrency(), true);
        DeclareValue("Minerals", () => _gameState.humanUnit.Get().cargo.minerals, true);
        DeclareValue("Organic", () => _gameState.humanUnit.Get().cargo.organic, true);
        DeclareValue("Power", () => _gameState.humanUnit.Get().cargo.power, true);
        DeclareValue("Free cargo space", () => _gameState.humanUnit.Get().CargoFree(), true);

        foreach (var q in _gameState.activeQuests) {
            if (q.faction == Faction.Phaa) {
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
        foreach (var tmpl in PhaaQuests.list) {
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
            image = "Phaa",
            text = GetGreetingsText(),
            actions = GetRootActions(),
        };
    }

    private string GetGreetingsText() {
        if (GetStatus() == DiplomaticStatus.War) {
            return (@"
                Your fleet is identified as hostile.

                Leave this system, until we start hostile actions of our own.
            ");
        }
        if (GetStatus() == DiplomaticStatus.Alliance) {
            return (@"
                Your fleet is identified as alied.

                Allied business is possible.
            ");
        }
        if (GetStatus() == DiplomaticStatus.NonAttackPact) {
            return (@"
                Your fleet is identified as non-dangerous.

                Friendly business is possible.
            ");
        }
        return (@"
            Your fleet is unidentified.

            State your business, unidentified fleet.
        ");
    }

    private List<TQuestAction> GetRootActions() {
        var actions = new List<TQuestAction>();

        if (GetStatus() != DiplomaticStatus.War) {
            actions.Add(new TQuestAction{
                text = "[Do business]",
                next = () => DoBusiness(null),
            });
        }

        actions.Add(new TQuestAction{
            text = "[Hostile actions]",
            next = HostileActions,
        });

        actions.Add(new TQuestAction{
            text = "[End conversation]",
            next = () => TQuestCard.exitQuestEnterMap,
        });

        return actions;
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
            var template = PhaaQuests.Find(_currentQuest.name);
            if (completion != null) {
                actions.Add(new TQuestAction{
                    text = template.completionPhrase(completion),
                    next = () => QuestComplete(template, completion),
                });
            }
        }

        if (GetStatus() == DiplomaticStatus.Unspecified) {
            actions.Add(new TQuestAction{
                text = "I propose a pact of non-aggression.",
                next = TryNonAttackPact,
            });
        } else if (GetStatus() == DiplomaticStatus.NonAttackPact) {
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

    private TQuestCard TryNonAttackPact() {
        if (GetReputation() < 0) {
            return new TQuestCard{
                text = (@"
                    We consider your kind to be too unpredictable for such arrangement.
                "),
                actions = GetBusinessActions(),
            };
        }
        _gameState.diplomaticStatuses[Faction.Phaa] = DiplomaticStatus.NonAttackPact;
        return new TQuestCard{
            text = (@"
                From now on, your fleet will be identified as non-dangerous.

                This status will be reconsidered if you will perform
                any hostile acts towards Phaa.
            "),
            actions = GetBusinessActions(),
        };
    }

    private TQuestCard AssistMenu() {
        return new TQuestCard{
            text = ($@"
                You help us - we give you {GetCurrencyName()}.
                You give us {GetCurrencyName()} - we help you.

                Do you need our help?
            "),
            actions = GetAssistMenuActions(),
        };
    }

    private List<TQuestAction> GetAssistMenuActions() {
        var actions = new List<TQuestAction>();

        actions.Add(new TQuestAction{
            text = "We need war situation intel. (5)",
            cond = () => _gameState.alienCurrency[Faction.Phaa] >= 5,
            next = BuyWarIntel,
        });

        actions.Add(new TQuestAction{
            text = "I want to recruit a vessel. (50)",
            cond = () => {
                if (GetStatus() != DiplomaticStatus.Alliance) {
                    return false;
                }
                if (_gameState.humanUnit.Get().fleet.Count == 4) {
                    return false;
                }
                return _gameState.alienCurrency[Faction.Phaa] >= 50;
            },
            next = BuyVessel,
        });

        actions.Add(new TQuestAction{
            text = "Nevermind.",
            next = GetFirstCard,
        });

        return actions;
    }

    private TQuestCard BuyWarIntel() {
        _gameState.alienCurrency[Faction.Phaa] -= 5;

        var krigiaBases = 0;
        StarBase krigiaWeakestBase = null;
        var wertuBases = 0;
        StarBase wertuWeakestBase = null;
        var draklidBases = 0;
        StarBase draklidWeakestBase = null;
        foreach (var starBase in _gameState.starBases.objects.Values) {
            if (starBase.owner == Faction.Krigia) {
                krigiaBases++;
                if (krigiaWeakestBase == null || krigiaWeakestBase.GarrisonCost() > starBase.GarrisonCost()) {
                    krigiaWeakestBase = starBase;
                }
            } else if (starBase.owner == Faction.Wertu) {
                wertuBases++;
                if (wertuWeakestBase == null || wertuWeakestBase.GarrisonCost() > starBase.GarrisonCost()) {
                    wertuWeakestBase = starBase;
                }
            } else if (starBase.owner == Faction.Draklid) {
                draklidBases++;
                if (draklidWeakestBase == null || draklidWeakestBase.GarrisonCost() > starBase.GarrisonCost()) {
                    draklidWeakestBase = starBase;
                }
            }
        }

        Func<Faction, int, StarBase, string> factionInfo = (Faction faction, int num, StarBase weakest) => {
            if (num == 0) {
                return ($@"
                    {faction.ToString()} has no systems under their control right now.
                ");
            }
            var suffix = num == 1 ? "" : "s";
            return ($@"
                {faction.ToString()} controls [u]{num}[/u] system{suffix}.
                [u]{weakest.system.Get().name}[/u] has the weakest garrison.
            ");
        };



        var text = ($@"
            We spy on our enemies in this region.

            {factionInfo(Faction.Krigia, krigiaBases, krigiaWeakestBase)}

            {factionInfo(Faction.Wertu, wertuBases, wertuWeakestBase)}

            {factionInfo(Faction.Draklid, draklidBases, draklidWeakestBase)}
        ");
        
        return new TQuestCard{
            text = text,
            actions = GetAssistMenuActions(),
        };
    }

    private TQuestCard BuyVessel() {
        _gameState.alienCurrency[Faction.Phaa] -= 50;
        
        var mantis = RpgGameState.instance.NewVessel(Faction.Earthling, VesselDesign.Find("Mantis"));
        mantis.pilotName = PilotNames.UniqPhaaName(RpgGameState.instance.usedNames);
        VesselFactory.Init(mantis, "Phaa Mantis");
        mantis.isMercenary = true;
        _gameState.humanUnit.Get().fleet.Add(mantis.GetRef());

        return new TQuestCard{
            text = ($@"
                One Mantis vessel will join your fleet.
            "),
            actions = GetAssistMenuActions(),
        };
    }

    private TQuestCard TryAlliance() {
        if (GetReputation() < 20) {
            return new TQuestCard{
                text = (@"
                    Alliance is possible, but only when our kinds will
                    share more friendly interactions.

                    We should focus on our trading business until then.
                "),
                actions = GetBusinessActions(),
            };
        }
        _gameState.diplomaticStatuses[Faction.Phaa] = DiplomaticStatus.Alliance;
        return new TQuestCard{
            text = (@"
                From now on, your fleet will be identified as allied.

                Phaa will join the Earthlings side.
            "),
            actions = GetBusinessActions(),
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
                text += $"> Received {reward.value} {GetCurrencyName()}\\n";
            } else if (reward.kind == Quest.RewardKind.GetRU) {
                text += $"> Received {reward.value} RU\\n";
            } else if (reward.kind == Quest.RewardKind.GetTechnology) {
                text += $"> Got {reward.value} technology\\n";
            }
        }
        text += "\n";
        return new TQuestCard{
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
                    next = () => {
                        RpgGameState.instance.reputations[Faction.Phaa] -= 2;
                        return DoBusiness("Your response is not satisfactory.");
                    },
                },
            },
        };
    }

    private TQuestCard HostileActions() {
        var actions = new List<TQuestAction>();

        if (GetStatus() != DiplomaticStatus.War) {
            actions.Add(new TQuestAction{
                text = "[Declare war]",
                next = () => {
                    RpgGameState.instance.reputations[Faction.Phaa] -= 5;
                    RpgGameState.instance.diplomaticStatuses[Faction.Phaa] = DiplomaticStatus.War;
                    return TQuestCard.exitQuestEnterMap;
                },
            });
        }

        actions.Add(new TQuestAction{
            text = "[Back]",
            next = DialogueRoot,
        });

        return new TQuestCard{
            actions = actions,
        };
    }

    private TQuestCard DialogueRoot() {
        return new TQuestCard {
            actions = GetRootActions(),
        };
    }
}
