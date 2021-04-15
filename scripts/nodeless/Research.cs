using System.Collections.Generic;

public class Research {
    public enum Material {
        None,
        Krigia,
        Wertu,
        Zyth,
    }

    public enum Category {
        Dummy,
        Fundamental,
        Upgrade,
        NewWeapon,
        NewSpecialWeapon,
        NewEnergySource,
        NewShield,
        NewArtifact,
        NewVesselDesign,
    }

    public string name;
    public Material material = Material.None;
    public Category category = Category.Fundamental;
    public int researchTime;
    public string effect = "";
    public string effect2 = "";
    public List<string> dependencies = new List<string>();

    public static bool IsAvailable(HashSet<string> researched, List<string> requirements) {
        foreach (string tech in requirements) {
            if (!researched.Contains(tech)) {
                return false;
            }
        }
        return true;
    }

    public static Research[] list = {
        // Misc tech.

        new Research{
            name = "Long-range Scanners",
            category = Category.Upgrade,
            researchTime = 35,
            effect = "an ability to scan non-military vessels",
        },

        new Research{
            name = "Long-range Scanners II",
            category = Category.Upgrade,
            researchTime = 60,
            effect = "an ability to scan any vessel",
            dependencies = {"Long-range Scanners"},
        },

        // Science tech.

        new Research{
            name = "Alien Tech Lab",
            category = Category.Upgrade,
            researchTime = 120,
            effect = "+10% research rate for alien projects",
        },

        // Utility tech.

        new Research{
            name = "Utility Lab Branch",
            researchTime = 90,
        },

        new Research{
            name = "Recycling",
            category = Category.Upgrade,
            researchTime = 150,
            effect = "when in idle mode, fuel gain is doubled",
            dependencies = {"Utility Lab Branch"},
        },

        new Research{
            name = "Drone Capacity",
            category = Category.Upgrade,
            researchTime = 65,
            effect = "drones resource capacity increased by 25",
            dependencies = {"Utility Lab Branch"},
        },

        new Research{
            name = "Drone Capacity II",
            category = Category.Upgrade,
            researchTime = 85,
            effect = "drones resource capacity increased by 50",
            dependencies = {"Drone Capacity II"},
        },

        // Vessel tech.

        new Research{
            name = "Vessel Lab Branch",
            researchTime = 45,
        },

        new Research{
            name = "Ark",
            category = Category.NewVesselDesign,
            researchTime = 50,
            dependencies = {"Vessel Lab Branch"},
        },

        new Research{
            name = "Interceptor",
            category = Category.NewVesselDesign,
            researchTime = 90,
            dependencies = {"Vessel Lab Branch"},
        },

        new Research{
            name = "Ark Exodus",
            researchTime = 35,
            dependencies = {"Ark"},
        },

        // Shield tech.

        new Research{
            name = "Level 2 Shields",
            researchTime = 120,
        },

        new Research{
            name = "Dispersion Field",
            category = Category.NewShield,
            researchTime = 40,
            dependencies = {"Level 2 Shields"},
        },

        new Research{
            name = "Reflector",
            category = Category.NewShield,
            researchTime = 40,
            dependencies = {"Level 2 Shields"},
        },

        new Research{
            name = "Laser Perimeter",
            category = Category.NewShield,
            researchTime = 50,
            dependencies = {"Level 2 Shields"},
        },

        new Research{
            name = "Lattice",
            category = Category.NewShield,
            researchTime = 55,
            dependencies = {"Level 2 Shields"},
        },

        new Research{
            name = "Level 3 Shields",
            researchTime = 200,
            dependencies = {"Level 2 Shields"},
        },

        new Research{
            name = "Phaser",
            category = Category.NewShield,
            researchTime = 110,
            dependencies = {"Level 3 Shields"},
        },

        // Energy sources tech.

        new Research{
            name = "Vortex Battery",
            category = Category.NewEnergySource,
            researchTime = 85,
        },

        new Research{
            name = "Living Dynamo",
            category = Category.NewEnergySource,
            researchTime = 95,
            dependencies = {"Vortex Battery"},
        },

        new Research{
            name = "Radioisotope Generator",
            category = Category.NewEnergySource,
            researchTime = 100,
            dependencies = {"Vortex Battery"},
        },

        new Research{
            name = "Cryogenic Block",
            category = Category.NewEnergySource,
            researchTime = 100,
            dependencies = {"Vortex Battery"},
        },

        new Research{
            name = "High-Capacity Reactors",
            researchTime = 150,
            dependencies = {"Vortex Battery"},
        },

        new Research{
            name = "Graviton Generator",
            category = Category.NewEnergySource,
            researchTime = 190,
            dependencies = {"High-Capacity Reactors"},
        },

        new Research{
            name = "Singularial Reactor",
            category = Category.NewEnergySource,
            researchTime = 225,
            dependencies = {"Graviton Generator"},
        },

        // Basic weapons tech.

        new Research{
            name = "Laser Weapons",
            researchTime = 40,
        },

        new Research{
            name = "Pulse Laser",
            category = Category.NewWeapon,
            researchTime = 65,
            dependencies = {"Laser Weapons"},
        },

        new Research{
            name = "Assault Laser",
            category = Category.NewWeapon,
            researchTime = 70,
            dependencies = {"Pulse Laser"},
        },

        new Research{
            name = "Zap",
            category = Category.NewWeapon,
            researchTime = 50,
            dependencies = {"Laser Weapons"},
        },

        new Research{
            name = "Point-Defense Laser",
            category = Category.NewWeapon,
            researchTime = 40,
            dependencies = {"Laser Weapons"},
        },

        new Research{
            name = "Stinger",
            category = Category.NewWeapon,
            researchTime = 90,
        },

        new Research{
            name = "Cutter",
            category = Category.NewWeapon,
            researchTime = 100,
        },

        new Research{
            name = "Flak Weapons",
            researchTime = 65,
        },

        new Research{
            name = "Reaper Cannon",
            category = Category.NewSpecialWeapon,
            researchTime = 100,
            dependencies = {"Flak Weapons"},
        },

        new Research{
            name = "Mortar",
            category = Category.NewSpecialWeapon,
            researchTime = 140,
            dependencies = {"Reaper Cannon"},
        },

        new Research{
            name = "Torpedo Launcher",
            category = Category.NewSpecialWeapon,
            researchTime = 200,
            dependencies = {"Flak Weapons", "Rocket Launcher"},
        },

        new Research{
            name = "Disruptor",
            category = Category.NewSpecialWeapon,
            researchTime = 90,
        },

        new Research{
            name = "Rocket Launcher",
            category = Category.NewWeapon,
            researchTime = 70,
        },

        new Research{
            name = "Hurricane",
            category = Category.NewWeapon,
            researchTime = 120,
            dependencies = {"Rocket Launcher"},
        },

        new Research{
            name = "Warp Device",
            category = Category.NewSpecialWeapon,
            researchTime = 170,
        },

        // Upgrades.

        new Research{
            name = "Gauss Production",
            category = Category.Upgrade,
            researchTime = 25,
            effect = "-20% needle gun production cost"
        },

        new Research{
            name = "Improved Fuel Tanks",
            category = Category.Upgrade,
            researchTime = 50,
            effect = "+5% fuel tank capacity",
        },

        new Research{
            name = "Improved Fuel Tanks II",
            category = Category.Upgrade,
            researchTime = 75,
            dependencies = {"Improved Fuel Tanks"},
            effect = "+10% fuel tank capacity",
        },

        new Research{
            name = "Improved Fuel Tanks III",
            category = Category.Upgrade,
            researchTime = 250,
            dependencies = {"Improved Fuel Tanks II"},
            effect = "+20% fuel tank capacity",
        },

        new Research{
            name = "Fleet Identifier",
            category = Category.Upgrade,
            researchTime = 60,
            effect = "color-coding for space units on the map",
        },

        new Research{
            name = "Jump Tracer Mk2",
            category = Category.Upgrade,
            researchTime = 75,
            effect = "+15% radar range",
        },

        new Research{
            name = "Jump Tracer Mk3",
            category = Category.Upgrade,
            researchTime = 150,
            dependencies = {"Jump Tracer Mk2"},
            effect = "+25% radar range",
        },

        // Wertu tech tree.

        new Research{
            name = "Wertu Weapons I",
            material = Material.Wertu,
            researchTime = 70,
        },

        new Research{
            name = "Photon Burst Cannon",
            material = Material.Wertu,
            category = Category.NewWeapon,
            researchTime = 50,
            dependencies = {"Wertu Weapons I"},
        },

        new Research{
            name = "Wertu Weapons II",
            material = Material.Wertu,
            researchTime = 110,
            dependencies = {"Wertu Weapons I"},
        },

        new Research{
            name = "Twin Photon Burst Cannon",
            material = Material.Wertu,
            category = Category.NewWeapon,
            researchTime = 20,
            dependencies = {"Wertu Weapons II"},
        },

        new Research{
            name = "Twin Photon Burst Cannon",
            material = Material.Wertu,
            category = Category.NewWeapon,
            researchTime = 20,
            dependencies = {"Wertu Weapons II"},
        },

        new Research{
            name = "Shield Breaker",
            material = Material.Wertu,
            category = Category.NewWeapon,
            researchTime = 65,
            dependencies = {"Wertu Weapons II"},
        },

        new Research{
            name = "Restructuring Ray",
            material = Material.Wertu,
            category = Category.NewSpecialWeapon,
            researchTime = 40,
            dependencies = {"Wertu Weapons II"},
        },

        new Research{
            name = "Wertu Weapons III",
            material = Material.Wertu,
            researchTime = 125,
            dependencies = {"Wertu Weapons II"},
        },

        new Research{
            name = "Photon Beam",
            material = Material.Wertu,
            category = Category.NewSpecialWeapon,
            researchTime = 100,
            dependencies = {"Wertu Weapons III"},
        },

        new Research{
            name = "Plasma Emitter",
            material = Material.Wertu,
            category = Category.NewWeapon,
            researchTime = 75,
            dependencies = {"Wertu Weapons III"},
        },

        // Krigia tech tree.

        new Research{
            name = "Krigia Weapons I",
            material = Material.Krigia,
            researchTime = 100,
        },

        new Research{
            name = "Scythe",
            material = Material.Krigia,
            category = Category.NewWeapon,
            researchTime = 40,
            dependencies = {"Krigia Weapons I"},
        },

        new Research{
            name = "Krigia Weapons II",
            material = Material.Krigia,
            researchTime = 120,
            dependencies = {"Krigia Weapons I"},
        },

        new Research{
            name = "Great Scythe",
            material = Material.Krigia,
            category = Category.NewWeapon,
            researchTime = 150,
            dependencies = {"Krigia Weapons II"},
        },

        new Research{
            name = "Krigia Weapons III",
            material = Material.Krigia,
            researchTime = 140,
            dependencies = {"Krigia Weapons II"},
        },

        new Research{
            name = "Lancer",
            material = Material.Krigia,
            category = Category.NewWeapon,
            researchTime = 240,
            dependencies = {"Krigia Weapons III"},
        },
    };
}
