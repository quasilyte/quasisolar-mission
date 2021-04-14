using System.Collections.Generic;

public class Skill {
    public string name;

    public int expCost;

    public string requires = "";

    public string effect = "";
    public string effect2 = "";

    public bool IsAvailable() {
        return requires == "" || RpgGameState.skillsLearned.Contains(requires);
    }

    public bool IsLearned() {
        return RpgGameState.skillsLearned.Contains(name);
    }

    public static Skill[] list = {
        new Skill{
            name = "Navigation I",
            expCost = 25,
            effect = "15% faster map travel speed",
        },

        new Skill{
            name = "Navigation II",
            expCost = 50,
            requires = "Navigation I",
            effect = "25% faster map travel speed",
        },

        new Skill{
            name = "Navigation III",
            expCost = 75,
            requires = "Navigation II",
            effect = "30% faster map travel speed",
        },

        new Skill{
            name = "Fighter",
            expCost = 40,
            effect = "25% more exp gain pfrom battles",
        },

        new Skill{
            name = "Mentor",
            expCost = 40,
            effect = "50% more allied pilots exp gain from battles",
        },

        new Skill{
            name = "Diplomacy",
            expCost = 75,
            effect = "better deals in negotiations",
        },

        new Skill{
            name = "Luck",
            expCost = 40,
            effect = "improves chances of getting better random events",
            effect2 = "affects some rewards (in a good way)",
        },

        new Skill{
            name = "Repair I",
            expCost = 45,
            effect = "after a battle, recover 10% of damage taken",
        },

        new Skill{
            name = "Repair II",
            expCost = 60,
            requires = "Repair I",
            effect = "after a battle, recover 20% of damage taken",
        },

        new Skill{
            name = "Escape Tactics",
            expCost = 60,
            effect = "retreating is costs 2 times less fuel units"
        },

        new Skill{
            name = "Siege Mastery I",
            expCost = 40,
            effect = "when in attack mode, do double damage against bases",
        },

        new Skill{
            name = "Siege Mastery II",
            expCost = 70,
            requires = "Siege Mastery I",
            effect = "when in attack mode, do double damage against bases",
            effect2 = "do 1 point of base damage even if garrison is not empty",
        },

        new Skill{
            name = "Salvaging",
            expCost = 35,
            effect = "get extra resources after winning a battle",
        },
    };
}