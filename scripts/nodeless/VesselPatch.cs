using System.Collections.Generic;
using System;

public class VesselPatch {
    public string name;

    public int expCost;

    // Implemented.
    public int maxHp = 0;
    public int maxSpeed = 0;
    public int repairCost = 0;
    public int maxBackupEnergy = 0;
    public int energyDamageReceived = 0;
    public int kineticDamageReceived = 0;
    public int thermalDamageReceived = 0;
    public int starDamageReceived = 0;
    public float cargoMultiplier = 1.0f;

    // TODO.
    public float energyRegen = 0;
    public int postBattleHpRecovery = 0;
    public int postBattleEnergyRecovery = 0;

    public static Dictionary<string, VesselPatch> patchByName;

    public static void InitLists() {
        Array.Sort(list, (x, y) => x.expCost.CompareTo(y.expCost));
        patchByName = new Dictionary<string, VesselPatch>();
        foreach (var patch in list) {
            patchByName[patch.name] = patch;
        }
    }

    public static VesselPatch[] list = new VesselPatch[] {
        new VesselPatch{
            expCost = 10,
            name = "Improved Cargo",
            cargoMultiplier = 1.1f,
        },

        new VesselPatch{
            expCost = 10,
            name = "Extra Storage Bay",
            cargoMultiplier = 1.25f,
            maxSpeed = -5,
        },

        new VesselPatch{
            expCost = 10,
            name = "Star Heat Resistor",
            starDamageReceived = -1,
        },

        // new VesselPatch{
        //     expCost = 10,
        //     name = "Alternative Cooling System",
        //     energyRegen = +0.75f,
        //     starDamageReceived = +4,
        // },

        // new VesselPatch{
        //     expCost = 30,
        //     name = "Auto-Repair System",
        //     postBattleHpRecovery = 20,
        // },

        // new VesselPatch{
        //     expCost = 25,
        //     name = "Advanced Energy Management",
        //     postBattleEnergyRecovery = 40,
        // },

        new VesselPatch{
            expCost = 20,
            name = "Reinforced Hull",
            maxHp = +20,
        },

        new VesselPatch{
            expCost = 40,
            name = "Extra Armor Layer",
            maxHp = +30,
            kineticDamageReceived = -1,
            repairCost = +2,
        },

        new VesselPatch{
            expCost = 15,
            name = "Anti-Impact Coating",
            kineticDamageReceived = -2,
            maxBackupEnergy = -10,
        },

        new VesselPatch{
            expCost = 15,
            name = "Anti-Laser Coating",
            energyDamageReceived = -2,
            maxBackupEnergy = -10,
        },

        new VesselPatch{
            expCost = 20,
            name = "Anti-Heat Coating",
            thermalDamageReceived = -3,
            maxBackupEnergy = -10,
        },

        new VesselPatch{
            expCost = 20,
            name = "Energy Deviator",
            energyDamageReceived = -3,
            kineticDamageReceived = +1,
        },

        // Unique patches.

        new VesselPatch{
            expCost = 0,
            name = "Organic Shell",
            maxHp = +65,
            thermalDamageReceived = +3,
            starDamageReceived = +2,
        },
    };
}
