using System.Collections.Generic;

public class Skill {
    public string name;

    public int expCost;

    public string requires = "";

    public string effect = "";
    public string effect2 = "";

    public bool IsAvailable() {
        return requires == "" || RpgGameState.instance.skillsLearned.Contains(requires);
    }

    public bool IsLearned() {
        return RpgGameState.instance.skillsLearned.Contains(name);
    }

    public static Skill[] list = {
        new Skill{
            name = "Drone Control I",
            expCost = 50,
            effect = "can own up to 8 drones",
        },

        new Skill{
            name = "Drone Control II",
            expCost = 80,
            effect = "can own up to 11 drones",
            requires = "Drone Control I",
        },

        new Skill{
            name = "Drone Control III",
            expCost = 110,
            effect = "can own up to 15 drones",
            requires = "Drone Control II",
        },

        new Skill{
            name = "Scholar",
            expCost = 50,
            effect = "-10 research time to all technologies",
        },

        new Skill{
            name = "Navigation I",
            expCost = 25,
            effect = "15% faster map travel speed",
        },

        new Skill{
            name = "Navigation II",
            expCost = 60,
            requires = "Navigation I",
            effect = "25% faster map travel speed",
        },

        new Skill{
            name = "Navigation III",
            expCost = 100,
            requires = "Navigation II",
            effect = "30% faster map travel speed",
        },

        new Skill{
            name = "Fighter",
            expCost = 40,
            effect = "33% more exp gain from battles",
        },

        // new Skill{
        //     name = "Mentor",
        //     expCost = 40,
        //     effect = "50% more allied pilots exp gain from battles",
        // },

        new Skill{
            name = "Intimidation",
            expCost = 50,
            effect = "easier to intimidate your foes",
        },

        new Skill{
            name = "Speaking",
            expCost = 65,
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
            expCost = 65,
            requires = "Repair I",
            effect = "after a battle, recover 20% of damage taken",
            effect2 = "after a battle, recover 25% of energy used",
        },

        new Skill{
            name = "Escape Tactics",
            expCost = 45,
            effect = "retreating costs 2 times less fuel units"
        },

        new Skill{
            name = "Siege Mastery",
            expCost = 50,
            effect = "when in attack mode, do double damage against bases",
            effect2 = "inflict damage even if garrison is not empty",
        },

        new Skill{
            name = "Salvaging",
            expCost = 35,
            effect = "get extra resources after winning a battle",
        },
    };
}