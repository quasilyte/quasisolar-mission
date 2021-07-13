using System;

public class StarBaseModule {
    public string name;
    
    public string effect;

    public int sellingPrice;

    public int buildTime;

    public string requires = ""; 

    public static StarBaseModule Find(string name) {
        foreach (var mod in list) {
            if (mod.name == name) {
                return mod;
            }
        }
        throw new Exception($"can't find {name} star base module");
    }

    public static StarBaseModule[] list = new StarBaseModule[]{
        new StarBaseModule{
            name = "Refuel Station",
            effect = "buy fuel at the cost of 1 RU instead of 3 at this base",
            sellingPrice = 2000,
            buildTime = 70,
        },

        new StarBaseModule{
            name = "Debris Rectifier",
            effect = "15% more RU when selling debris at this base",
            sellingPrice = 1500,
            buildTime = 60,
        },

        new StarBaseModule{
            name = "Minerals Collector",
            effect = "collect mineral resources from explored planets",
            sellingPrice = 2000,
            buildTime = 30,
        },

        new StarBaseModule{
            name = "Organic Collector",
            effect = "collect organic resources from explored planets",
            sellingPrice = 2500,
            buildTime = 30,
        },

        new StarBaseModule{
            name = "Power Collector",
            effect = "collect power resources from explored planets",
            sellingPrice = 5000,
            buildTime = 30,
        },

        new StarBaseModule{
            name = "Minerals Refinery",
            requires = "Minerals Collector",
            effect = "collected minerals are turned into RU",
            sellingPrice = 3000,
            buildTime = 60,
        },

        new StarBaseModule{
            name = "Organic Refinery",
            requires = "Organic Collector",
            effect = "collected organic is turned into RU",
            sellingPrice = 7000,
            buildTime = 80,
        },

        new StarBaseModule{
            name = "Power Refinery",
            requires = "Power Collector",
            effect = "collected power is turned into RU",
            sellingPrice = 8000,
            buildTime = 80,
        },

        new StarBaseModule{
            name = "Defensive Turrets",
            sellingPrice = 3000,
            buildTime = 20,
        },

        new StarBaseModule{
            name = "Warehouse",
            effect = "increases star base maximum resource capacity",
            sellingPrice = 4000,
            buildTime = 50,
        },

        new StarBaseModule{
            name = "Vessel Factory",
            effect = "increases vessel production speed",
            sellingPrice = 5000,
            buildTime = 90,
        },
    };
}
