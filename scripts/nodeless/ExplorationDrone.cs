using System;

public class ExplorationDrone {
    public string name;

    public int sellingPrice;

    public string description;

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
            description = "Can only explore low-temp rocky worlds, but it's fast and cheap.",
        },

        new ExplorationDrone{
            name = "Fog Rider",
            sellingPrice = 800,
            maxTemp = 100,
            explorationRate = 4,
            canExploreGasGiants = true,
            description = "A slower version of Curiosity that can explore gas giants.",
        },

        new ExplorationDrone{
            name = "Flame Eater",
            sellingPrice = 1400,
            maxTemp = 270,
            explorationRate = 2,
            needsResearch = true,
            description = "Very slow, but can tolerate high-temperature worlds.",
        },

        new ExplorationDrone{
            name = "Seeker",
            sellingPrice = 1400,
            maxTemp = 190,
            explorationRate = 6,
            needsResearch = true,
            description = "Improved Curiosity: faster and higher temperature tolerance.",
        },

        new ExplorationDrone{
            name = "Fog Shark",
            sellingPrice = 2000,
            maxTemp = 210,
            explorationRate = 7,
            canExploreGasGiants = true,
            needsResearch = true,
            description = "A Seeker upgrade that can explore gas giants.",
        },

        new ExplorationDrone{
            name = "Ifrit",
            sellingPrice = 3000,
            maxTemp = 500,
            explorationRate = 6,
            needsResearch = true,
            description = "This drone can explore the hottest planets.",
        },

        new ExplorationDrone{
            name = "Enigma",
            sellingPrice = 4000,
            maxTemp = 450,
            explorationRate = 10,
            canExploreGasGiants = true,
            needsResearch = true,
            description = "An all-around good drone that can't be replicated.",
        },
    };
}
