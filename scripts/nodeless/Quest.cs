using System.Collections.Generic;
using Godot;
using System;

public static class Quest {
    public class Template {
        public string name;

        public Func<bool> cond = () => true;

        public Func<Data, string> issueText;
        public Func<Data, string> logEntryText;
        public Func<Data, string> acceptResponse;
        public Func<CompletionData, string> completionPhrase;
        public Func<CompletionData, string> completionResponse;

        public Func<Template, Quest.Data> constructor;

        public Quest.Data CreateQuestData() {
            return constructor(this);
        }
    }

    public class CompletionData {
        public Data quest;
        public List<Action> requirementResolvers = new List<Action>();
        public Dictionary<RequirementKind, object> values = new Dictionary<RequirementKind, object>();
    }

    public class Data {
        public string name;

        public Faction faction;

        public List<Reward> rewards = new List<Reward>();
        public List<Requirement> requirements = new List<Requirement>();

        public string Req(RequirementKind kind) {
            foreach (var req in requirements) {
                if (req.kind == kind) {
                    return req.value.ToString();
                }
            }
            throw new Exception("can't find requirement " + kind.ToString());
        }

        public string Rew(RewardKind kind) {
            foreach (var rew in rewards) {
                if (rew.kind == kind) {
                    return rew.value.ToString();
                }
            }
            throw new Exception("can't find requirement " + kind.ToString());
        }
    }

    public enum RequirementKind {
        GiveMinerals,
        GiveOrganic,
        GivePower,
        GiveKrigiaMaterial,
        DestroyBase,
        FindGasGiant,
        FindVespionSystem,
        FindTwoWertuSystems,
        CompleteResearch,
    }

    public class Requirement {
        public RequirementKind kind;
        public object value;

        public Requirement(RequirementKind reqKind, object reqValue = null) {
            kind = reqKind;
            value = reqValue;
        }

        public int IntValue() {
            if (value is int intval) {
                return intval;
            }
            return (int)((Int64)value);
        }
    }

    public enum RewardKind {
        GetReputation,
        GetAlienCurrency,
        GetRU,
        GetTechnology,
    }

    public class Reward {
        public RewardKind kind;
        public object value;

        public Reward(RewardKind rewardKind, object rewardValue) {
            kind = rewardKind;
            value = rewardValue;
        }
    }

    public static CompletionData CheckRequirements(RpgGameState gameState, Data q) {
        var completion = new CompletionData();
        foreach (var req in q.requirements) {
            if (!CheckOneRequirement(gameState, completion, req)) {
                return null;
            }
        }
        completion.quest = q;
        return completion;
    }

    public static void Complete(RpgGameState gameState, CompletionData completion) {
        foreach (var resolve in completion.requirementResolvers) {
            resolve();
        }
        var q = completion.quest;
        foreach (var reward in q.rewards) {
            ApplyOneReward(gameState, q, reward);
        }
    }

    private static void ApplyOneReward(RpgGameState gameState, Data q, Reward reward) {
        if (reward.kind == RewardKind.GetReputation) {
            gameState.reputations[q.faction] += (int)reward.value;
        } else if (reward.kind == RewardKind.GetRU) {
            gameState.credits += (int)reward.value;
        } else if (reward.kind == RewardKind.GetAlienCurrency) {
            gameState.alienCurrency[q.faction] += (int)reward.value;
        } else if (reward.kind == RewardKind.GetTechnology) {
            gameState.technologiesResearched.Add((string)reward.value);
        } else {
            throw new Exception("can't apply reward of kind " + reward.kind.ToString());
        }
    }

    private static bool CheckOneRequirement(RpgGameState gameState, CompletionData completion, Requirement req) {
        var unit = gameState.humanUnit.Get();
        var cargo = unit.cargo;

        if (req.kind == RequirementKind.GiveMinerals) {
            completion.requirementResolvers.Add(() => unit.CargoAddMinerals(-(int)req.value));
            return cargo.minerals >= (int)req.value;
        }
        if (req.kind == RequirementKind.GiveOrganic) {
            completion.requirementResolvers.Add(() => unit.CargoAddOrganic(-req.IntValue()));
            return cargo.organic >= req.IntValue();
        }
        if (req.kind == RequirementKind.GivePower) {
            completion.requirementResolvers.Add(() => unit.CargoAddPower(-(int)req.value));
            return cargo.power >= (int)req.value;
        }

        if (req.kind == RequirementKind.GiveKrigiaMaterial) {
            completion.requirementResolvers.Add(() => gameState.researchMaterial.Add(-(int)req.value, Faction.Krigia));
            return gameState.researchMaterial.krigia >= (int)req.value;
        }

        if (req.kind == RequirementKind.FindVespionSystem) {
            return RpgGameState.vespionBase.system.Get().Visited();
        }
        if (req.kind == RequirementKind.FindTwoWertuSystems) {
            var num = 0;
            foreach (var sys in RpgGameState.starSystemList) {
                if (!sys.Visited()) {
                    continue;
                }
                if (sys.starBase.id != 0 && sys.starBase.Get().owner == Faction.Wertu) {
                    num++;
                }
            }
            return num >= 2;
        }

        if (req.kind == RequirementKind.DestroyBase) {
            var sys = RpgGameState.starSystemByName[(string)req.value];
            return sys.starBase.id == 0 || sys.starBase.Get().owner == Faction.Earthling;
        }

        if (req.kind == RequirementKind.CompleteResearch) {
            return gameState.technologiesResearched.Contains((string)req.value);
        }

        if (req.kind == RequirementKind.FindGasGiant) {
            string systemName = null;
            foreach (var sys in gameState.starSystems.objects.Values) {
                if (!sys.Visited()) {
                    continue;
                }
                foreach (var p in sys.resourcePlanets) {
                    if (p.gasGiant) {
                        systemName = sys.name;
                        break;
                    }
                }
            }
            completion.values[req.kind] = systemName;
            return systemName != null;
        }

        throw new System.Exception("unexpected requirement: " + req.kind.ToString());
    }
}
