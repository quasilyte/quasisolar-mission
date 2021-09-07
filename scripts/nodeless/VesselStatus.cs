using System.Collections.Generic;
using System;

public class VesselStatus {
    public enum Kind {
        Basic, // Always available (if the required level is reached)
        Rolled, // Can become available at the required level
        Unique, // Can only get it through the events
    }

    public string name;

    public int expCost;

    public Kind kind = Kind.Basic;
    public int level;

    // Implemented.
    public int maxHp = 0;
    public int maxSpeed = 0;
    public int repairCost = 0;
    public int maxBackupEnergy = 0;
    public int electromagneticDamageReceived = 0;
    public int kineticDamageReceived = 0;
    public int thermalDamageReceived = 0;
    public int starDamageReceived = 0;
    public float acceleration = 0;
    public float energyRegen = 0;
    public float cargoMultiplier = 1.0f;

    // TODO.
    public int postBattleHpRecovery = 0;
    public int postBattleEnergyRecovery = 0;

    public static Dictionary<string, VesselStatus> statusByName;

    public static void InitLists() {
        Array.Sort(list, (x, y) => x.level.CompareTo(y.level));
        statusByName = new Dictionary<string, VesselStatus>();
        rollableList = new List<VesselStatus>();
        foreach (var status in list) {
            if (status.kind == Kind.Rolled) {
                rollableList.Add(status);
            }
            statusByName[status.name] = status;
        }
    }

    public static List<VesselStatus> rollableList;

    public static VesselStatus[] list = new VesselStatus[] {
        // Level 0 upgrades.

        new VesselStatus{
            kind = Kind.Basic,
            level = 0,
            expCost = 10,
            name = "Improved Cargo",
            cargoMultiplier = 1.1f,
        },

        new VesselStatus{
            kind = Kind.Basic,
            level = 1,
            expCost = 10,
            name = "Star Heat Resistor",
            starDamageReceived = -1,
        },

        // Level 1 upgrades.

        new VesselStatus{
            kind = Kind.Basic,
            level = 1,
            expCost = 20,
            name = "Reinforced Hull",
            maxHp = +20,
        },

        new VesselStatus{
            kind = Kind.Rolled,
            level = 1,
            expCost = 15,
            name = "Extra Storage Bay",
            cargoMultiplier = 1.3f,
            maxSpeed = -5,
        },

        // Level 2 upgrades.

        new VesselStatus{
            kind = Kind.Basic,
            level = 2,
            expCost = 20,
            name = "Energy Deviator",
            electromagneticDamageReceived = -3,
            kineticDamageReceived = +1,
        },

        new VesselStatus{
            kind = Kind.Rolled,
            level = 2,
            expCost = 20,
            name = "Alternative Cooling System",
            energyRegen = +0.75f,
            starDamageReceived = +4,
        },

        // Level 3 upgrades.

        new VesselStatus{
            kind = Kind.Basic,
            level = 3,
            expCost = 40,
            name = "Extra Armor Layer",
            maxHp = +30,
            kineticDamageReceived = -1,
            repairCost = +2,
        },

        new VesselStatus{
            kind = Kind.Rolled,
            level = 3,
            expCost = 25,
            name = "Anti-Impact Coating",
            kineticDamageReceived = -2,
            maxBackupEnergy = -10,
        },

        new VesselStatus{
            kind = Kind.Rolled,
            level = 3,
            expCost = 25,
            name = "Anti-Laser Coating",
            electromagneticDamageReceived = -2,
            maxBackupEnergy = -10,
        },

        new VesselStatus{
            kind = Kind.Rolled,
            level = 3,
            expCost = 25,
            name = "Anti-Heat Coating",
            thermalDamageReceived = -3,
            maxBackupEnergy = -10,
        },

        // new VesselStatus{
        //     expCost = 30,
        //     name = "Auto-Repair System",
        //     postBattleHpRecovery = 20,
        // },

        // new VesselStatus{
        //     expCost = 25,
        //     name = "Advanced Energy Management",
        //     postBattleEnergyRecovery = 40,
        // },
    };
}
