using System;

public class ExplorationDrone {
    public string name;

    public int sellingPrice;

    public int maxTemp;
    public int explorationRate;
    public bool canExploreGasGiants = false;

    public bool needsResearch = false;

    public static ExplorationDrone Find(string name) {
        foreach (var drone in list) {
            if (drone.name == name) {
                return drone;
            }
        }
        throw new Exception($"can't find {name} exploration drone");
    }

    public static ExplorationDrone[] list = new ExplorationDrone[]{
        new ExplorationDrone{
            name = "Curiosity",
            sellingPrice = 800,
            maxTemp = 130,
            explorationRate = 5,
        },

        new ExplorationDrone{
            name = "Fog Rider",
            sellingPrice = 800,
            maxTemp = 100,
            explorationRate = 3,
            canExploreGasGiants = true,
        },

        new ExplorationDrone{
            name = "Flame Eater",
            sellingPrice = 1400,
            maxTemp = 270,
            explorationRate = 2,
            needsResearch = true,
        },

        new ExplorationDrone{
            name = "Seeker",
            sellingPrice = 1400,
            maxTemp = 190,
            explorationRate = 6,
            needsResearch = true,
        },

        new ExplorationDrone{
            name = "Fog Shark",
            sellingPrice = 2000,
            maxTemp = 210,
            explorationRate = 7,
            canExploreGasGiants = true,
            needsResearch = true,
        },

        new ExplorationDrone{
            name = "Stingray",
            sellingPrice = 2500,
            maxTemp = 260,
            explorationRate = 15,
            needsResearch = true,
        },

        new ExplorationDrone{
            name = "Ifrit",
            sellingPrice = 3000,
            maxTemp = 500,
            explorationRate = 6,
            needsResearch = true,
        },

        new ExplorationDrone{
            name = "Enigma",
            sellingPrice = 4000,
            maxTemp = 450,
            explorationRate = 10,
            canExploreGasGiants = true,
            needsResearch = true,
        },
    };
}
