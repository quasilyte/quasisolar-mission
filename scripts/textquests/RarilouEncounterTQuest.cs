using System.Collections.Generic;

public class RarilouEncounterTQuest : AbstractTQuest {
    public RarilouEncounterTQuest() {
        var gameState = RpgGameState.instance;
        DeclareValue("Rarilou reputation", () => gameState.reputations[Faction.Rarilou], true);
        DeclareValue("Status", () => DiplomaticStatusString(gameState.diplomaticStatuses[Faction.Rarilou]), true);
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
        var actions = new List<TQuestAction>();

        bool canAttack = RpgGameState.instance.diplomaticStatuses[Faction.Rarilou] == DiplomaticStatus.War ||
            RpgGameState.instance.diplomaticStatuses[Faction.Rarilou] == DiplomaticStatus.Unspecified;
        if (canAttack) {
            actions.Add(new TQuestAction{
                text = "Hostile actions.",
                next = HostileActions,
            });
        }

        actions.Add(new TQuestAction{
            text = "End conversation.",
            next = () => TQuestCard.exitQuestEnterMap,
        });

        return new TQuestCard {
            image = "Rarilou",
            text = ($@"
                Hehehe.
            "),
            actions = actions,
        };
    }

    private TQuestCard HostileActions() {
        return new TQuestCard{
            text = (@"
                Choose your action.
            "),
            actions = {
                new TQuestAction{
                    text = "Attack their fleet.",
                    next = () => {
                        RpgGameState.instance.reputations[Faction.Rarilou] -= 5;
                        RpgGameState.instance.diplomaticStatuses[Faction.Rarilou] = DiplomaticStatus.War;
                        return TQuestCard.exitQuestEnterArena;
                    },
                },
                new TQuestAction{
                    text = "Back.",
                    next = DialogueRoot,
                },
            },
        };
    }
}
