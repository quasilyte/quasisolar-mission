using System;
using System.Collections.Generic;

public class EnergySource : IItem {
    public string name;
    public string description;

    public int sellingPrice;

    public float maxEnergy;
    public float maxBackupEnergy;
    public float energyRegen;

    public bool researchRequired = false;

    public static EnergySource Find(string name) {
        foreach (var battery in list) {
            if (battery.name == name) {
                return battery;
            }
        }
        throw new Exception($"can't find {name} energy source");
    }

    public ItemKind GetItemKind() { return ItemKind.EnergySource; }

    public string RenderHelp() {
        if (name == "None") {
            // A special case.
            return "An empty energy source slot";
        }

        var parts = new List<string>();
        parts.Add(name + " (" + sellingPrice.ToString() + ")");
        parts.Add("");
        parts.Add(description + ".");
        parts.Add("");
        parts.Add("Max energy: " + maxEnergy);
        parts.Add("Max backup energy: " + maxBackupEnergy);
        parts.Add("Energy regen: " + energyRegen);
        return string.Join("\n", parts);
    }

    public static EnergySource[] list = {
        new EnergySource{
            name = "None",
            description = "TODO",
            sellingPrice = 0,

            maxEnergy = 0,
            maxBackupEnergy = 0,
            energyRegen = 0,
        },

        new EnergySource{
            name = "Power Generator",
            description = "TODO",
            sellingPrice = 1300,

            maxEnergy = 20,
            maxBackupEnergy = 60,
            energyRegen = 2.5f,
        },

        new EnergySource{
            name = "Advanced Power Generator",
            description = "TODO",
            sellingPrice = 3000,

            maxEnergy = 25,
            maxBackupEnergy = 90,
            energyRegen = 3.0f,
        },

        new EnergySource{
            name = "Vortex Battery",
            description = "TODO",
            sellingPrice = 4500,
            researchRequired = true,

            maxEnergy = 30,
            maxBackupEnergy = 130,
            energyRegen = 4.0f,
        },

        new EnergySource{
            name = "Living Dynamo",
            description = "TODO",
            sellingPrice = 5600,
            researchRequired = true,

            maxEnergy = 20,
            maxBackupEnergy = 40,
            energyRegen = 8,
        },

        new EnergySource{
            name = "Radioisotope Generator",
            description = "TODO",
            sellingPrice = 6000,
            researchRequired = true,

            maxEnergy = 50,
            maxBackupEnergy = 0,
            energyRegen = 7,
        },

        new EnergySource{
            name = "Cryogenic Block",
            description = "TODO",
            sellingPrice = 6000,
            researchRequired = true,

            maxEnergy = 45,
            maxBackupEnergy = 150,
            energyRegen = 4.5f,
        },

        new EnergySource{
            name = "Graviton Generator",
            description = "TODO",
            sellingPrice = 12000,
            researchRequired = true,

            maxEnergy = 60,
            maxBackupEnergy = 200,
            energyRegen = 8.5f,
        },

        new EnergySource{
            name = "Singularial Reactor",
            description = "TODO",
            sellingPrice = 13500,
            researchRequired = true,

            maxEnergy = 70,
            maxBackupEnergy = 240,
            energyRegen = 9.5f,
        },
    };
}