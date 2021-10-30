using System.Collections.Generic;
using System;

public class VesselMod: IItem {
    public string name;

    public int level;

    public bool removable = true;
    public bool flagshipOnly = false;

    public string specialEffect = "";

    // Implemented.
    public int maxHp = 0;
    public int repairCost = 0;
    public float shieldDurationRate = 0;
    public int shieldEnergyCost = 0;
    public int maxSpeed = 0;
    public int maxBackupEnergy = 0;
    public int electromagneticDamageReceived = 0;
    public int kineticDamageReceived = 0;
    public int thermalDamageReceived = 0;
    public int asteroidDamageReceived = 0;
    public int starDamageReceived = 0;
    public float acceleration = 0;
    public float rotationSpeed = 0;
    public float cargoSpace = 0;
    public float energyRegen = 0;
    public float sentinelActionCooldown = 0;
    public int sentinelMaxHp = 0;
    public int luckModifier = 0;

    // TODO.
    public int postBattleHpRecovery = 0;
    public int postBattleEnergyRecovery = 0;

    public static Dictionary<string, VesselMod> modByName;

    public ItemKind GetItemKind() { return ItemKind.Mod; }

    private string FormatInt(int value) {
        return value > 0 ? "+" + value : value.ToString();
    }

    private string FormatFloat(float value) {
        return value > 0 ? "+" + value : value.ToString();
    }

    private string FormatFloatPercentage(float value) {
        var sign = value > 0 ? "+" : "";
        return sign + ((int)(value * 100)) + "%";
    }
    
    public List<string> GetEffects() {
        var effects = new List<string>();

        if (maxHp != 0) {
            effects.Add($"max hp {FormatInt(maxHp)}");
        }
        if (repairCost != 0) {
            effects.Add($"repair cost {FormatInt(repairCost)}/hp");
        }
        if (shieldDurationRate != 0) {
            effects.Add($"shield activation duration {FormatFloatPercentage(shieldDurationRate)}");
        }
        if (shieldEnergyCost != 0) {
            effects.Add($"shield activation cost {FormatInt(shieldEnergyCost)}");
        }
        if (maxSpeed != 0) {
            effects.Add($"max speed {FormatInt(maxSpeed)}");
        }
        if (maxBackupEnergy != 0) {
            effects.Add($"max backup energy {FormatInt(maxBackupEnergy)}");
        }
        if (energyRegen != 0) {
            effects.Add($"energy regen {FormatFloat(energyRegen)}");
        }
        if (kineticDamageReceived != 0) {
            effects.Add($"kinetic damage received {FormatInt(kineticDamageReceived)}");
        }
        if (electromagneticDamageReceived != 0) {
            effects.Add($"electromagnetic damage received {FormatInt(electromagneticDamageReceived)}");
        }
        if (thermalDamageReceived != 0) {
            effects.Add($"thermal damage received {FormatInt(thermalDamageReceived)}");
        }
        if (asteroidDamageReceived != 0) {
            effects.Add($"asteroid damage received {FormatInt(asteroidDamageReceived)}");
        }
        if (starDamageReceived != 0) {
            effects.Add($"star heat damage received {FormatInt(starDamageReceived)}");
        }
        if (acceleration != 0) {
            effects.Add($"acceleration {FormatFloat(acceleration)}");
        }
        if (rotationSpeed != 0) {
            effects.Add($"rotation speed {FormatFloat(rotationSpeed)}");
        }
        if (cargoSpace != 0) {
            effects.Add($"cargo space {FormatFloatPercentage(cargoSpace)}");
        }
        if (sentinelActionCooldown != 0) {
            effects.Add($"sentinel action cooldown {FormatFloatPercentage(sentinelActionCooldown)}");
        }
        if (sentinelMaxHp != 0) {
            effects.Add($"sentinel max hp {FormatInt(sentinelMaxHp)}");
        }
        if (luckModifier != 0) {
            effects.Add($"luck {FormatInt(luckModifier)}");
        }

        if (specialEffect != "") {
            effects.Add(specialEffect);
        }

        return effects;
    }

    public string RenderHelp() {
        var lines = new List<string>();

        lines.Add(name + "\n");

        lines.Add($"Level: {level}");

        foreach (var effect in GetEffects()) {
            lines.Add($"Effect: {effect}");
        }

        if (removable) {
            lines.Add("\nThis mod can be removed.");
        }
        if (flagshipOnly) {
            lines.Add("\nThis mod only works for a flagship.");
        }

        return String.Join("\n", lines);
    }

    public static void InitLists() {
        Array.Sort(list, (x, y) => x.level.CompareTo(y.level));
        modByName = new Dictionary<string, VesselMod>();
        foreach (var status in list) {
            modByName[status.name] = status;
        }
    }

    public static VesselMod[] list = new VesselMod[] {
        // Curse-like mods.

        new VesselMod{
            level = 0,
            name = "Mechanical Curse",
            electromagneticDamageReceived = +2,
            kineticDamageReceived = +2,
            thermalDamageReceived = +2,
            repairCost = -1,
            removable = false,
        },

        // Level 1 mods.

        new VesselMod{
            level = 1,
            name = "Reinforced Hull",
            maxHp = +35,
            maxSpeed = -10,
        },

        new VesselMod{
            level = 1,
            name = "Fortification",
            maxHp = +20,
            asteroidDamageReceived = -10,
        },

        new VesselMod{
            level = 1,
            name = "Extra Armor",
            maxHp = +25,
            cargoSpace = -0.2f,
        },

        new VesselMod{
            level = 1,
            name = "Shield Booster",
            shieldDurationRate = +0.1f,
            shieldEnergyCost = +15,
        },

        new VesselMod{
            level = 1,
            name = "Customized Power System",
            maxBackupEnergy = +15,
        },

        new VesselMod{
            level = 1,
            name = "Engine Throttler",
            maxBackupEnergy = +25,
            acceleration = -0.75f,
        },

        new VesselMod{
            level = 1,
            name = "Asteroid Danger",
            asteroidDamageReceived = +25,
            repairCost = -1,
            removable = false,
        },

        new VesselMod{
            level = 1,
            name = "Star Heat Resistor",
            starDamageReceived = -1,
        },

        new VesselMod{
            level = 1,
            name = "Extended Storage",
            cargoSpace = +0.3f,
            maxSpeed = -5,
        },

        new VesselMod{
            level = 1,
            name = "Alternative Cooling System",
            energyRegen = +0.75f,
            starDamageReceived = +4,
        },

        new VesselMod{
            level = 1,
            name = "Energy Deviator",
            electromagneticDamageReceived = -2,
            kineticDamageReceived = +2,
        },

        new VesselMod{
            level = 1,
            name = "Heat Deviator",
            thermalDamageReceived = -3,
            kineticDamageReceived = +2,
        },

        new VesselMod{
            level = 1,
            name = "Asteroid Blocker",
            asteroidDamageReceived = -30,
        },

        // Level 2 mods.

        new VesselMod{
            level = 2,
            name = "Unique Alloys",
            maxHp = +50,
            repairCost = +2,
        },

        new VesselMod{
            level = 2,
            name = "Anti-Electromagnetic Coating",
            electromagneticDamageReceived = -2,
            maxBackupEnergy = -15,
        },

        new VesselMod{
            level = 2,
            name = "Anti-Kinetic Coating",
            kineticDamageReceived = -2,
            maxBackupEnergy = -15,
        },

        new VesselMod{
            level = 2,
            name = "Anti-Thermal Coating",
            thermalDamageReceived = -3,
            maxBackupEnergy = -15,
        },

        new VesselMod{
            level = 2,
            name = "Battery Booster",
            maxBackupEnergy = +45,
        },

        new VesselMod{
            level = 2,
            name = "Sentinel Patch: Overclock",
            sentinelActionCooldown = -0.15f,
            energyRegen = -0.4f,
        },

        new VesselMod{
            level = 2,
            name = "Sentinel Patch: Berserk",
            sentinelActionCooldown = -0.25f,
            sentinelMaxHp = -10,
        },

        new VesselMod{
            level = 2,
            name = "Sentinel Patch: Bastion",
            sentinelMaxHp = +15,
            energyRegen = -0.1f,
        },

        new VesselMod{
            level = 2,
            name = "Turtle",
            maxHp = +40,
            kineticDamageReceived = -1,
            acceleration = -0.6f,
            rotationSpeed = -0.2f,
        },

        new VesselMod{
            level = 2,
            name = "Luck Charm",
            luckModifier = +1,
        },

        // Level 3 mods.

        new VesselMod{
            level = 3,
            name = "Dark Beacon",
            energyRegen = +2,
            specialEffect = "emits mysterious broadcasts into the space",
            removable = false,
        },

        new VesselMod{
            level = 3,
            name = "Lilium Sigil",
            maxHp = +15,
            kineticDamageReceived = -1,
            electromagneticDamageReceived = -1,
            thermalDamageReceived = -1,
            removable = false,
        },

        new VesselMod{
            level = 3,
            name = "Pirate Mark",
            maxSpeed = +5,
            acceleration = +0.5f,
            rotationSpeed = +0.25f,
            cargoSpace = +0.15f,
            removable = false,
        },

        new VesselMod{
            level = 3,
            name = "Leader Mark",
            maxHp = +50,
            energyRegen = +1.2f,
            removable = false,
            flagshipOnly = true,
        },
    };
}
