using System.Collections.Generic;

public class Research {
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
        NewSentinel,
    }

    public string name;
    public Faction material = Faction.Neutral;
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

    public static Dictionary<string, Research> researchByName;

    public static Research Find(string name) {
        return researchByName[name];
    }

    public static void InitLists() {
        foreach (var artifact in ArtifactDesign.list) {
            list.Add(new Research{
                name = artifact.name,
                category = Research.Category.NewArtifact,
                researchTime = 40,
            });
        }

        researchByName = new Dictionary<string, Research>();
        foreach (var r in list) {
            researchByName.Add(r.name, r);
        }
    }

    public static List<Research> list = new List<Research>{
        // Science tech.

        new Research{
            name = "Alien Tech Lab",
            category = Category.Upgrade,
            researchTime = 70,
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
            name = "Gladiator",
            category = Category.NewVesselDesign,
            researchTime = 160,
            dependencies = {
                "Interceptor",
                "Alien Tech Lab",
            },
        },

        new Research{
            name = "Valkyrie",
            category = Category.NewVesselDesign,
            researchTime = 200,
            dependencies = {"Gladiator"},
        },

        new Research{
            name = "Ark Exodus",
            researchTime = 35,
            dependencies = {"Ark"},
        },

        // Sentinel tech.

        new Research{
            name = "Point-Defense Guard",
            category = Category.NewSentinel,
            researchTime = 60,
            dependencies = {"Point-Defense Laser"},
        },

        new Research{
            name = "Photon Fighter",
            material = Faction.Wertu,
            category = Category.NewSentinel,
            researchTime = 25,
            dependencies = {"Photon Burst Cannon"},
        },

        new Research{
            name = "Restructuring Guard",
            material = Faction.Wertu,
            category = Category.NewSentinel,
            researchTime = 90,
            dependencies = {"Restructuring Ray"},
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
            name = "Diffuser",
            category = Category.NewShield,
            researchTime = 100,
            dependencies = {"Level 3 Shields"},
        },

        new Research{
            name = "Phaser",
            category = Category.NewShield,
            researchTime = 110,
            dependencies = {"Level 3 Shields"},
        },

        new Research{
            name = "Aegis",
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
            name = "Rocket Launcher",
            category = Category.NewWeapon,
            researchTime = 70,
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
            name = "Jump Tracer Mk2",
            category = Category.Upgrade,
            researchTime = 50,
            effect = "+15% radar range",
        },

        new Research{
            name = "Jump Tracer Mk3",
            category = Category.Upgrade,
            researchTime = 85,
            dependencies = {"Jump Tracer Mk2"},
            effect = "+25% radar range",
        },

        // Zyth tech tree.

        new Research{
            name = "Zyth Weapons I",
            material = Faction.Zyth,
            researchTime = 75,
        },

        new Research{
            name = "Harpoon",
            material = Faction.Zyth,
            category = Category.NewSpecialWeapon,
            researchTime = 65,
            dependencies = {"Zyth Weapons I"},
        },

        new Research{
            name = "Zyth Weapons II",
            material = Faction.Zyth,
            researchTime = 90,
            dependencies = {"Zyth Weapons I"},
        },

        new Research{
            name = "Hellfire",
            material = Faction.Zyth,
            category = Category.NewWeapon,
            researchTime = 75,
            dependencies = {"Zyth Weapons II"},
        },

        new Research{
            name = "Zyth Weapons III",
            material = Faction.Zyth,
            researchTime = 100,
            dependencies = {"Zyth Weapons II"},
        },

        new Research{
            name = "Disk Thrower",
            material = Faction.Zyth,
            category = Category.NewWeapon,
            researchTime = 100,
            dependencies = {"Zyth Weapons III"},
        },

        // Draklid tech tree.

        new Research{
            name = "Draklid Weapons",
            material = Faction.Draklid,
            researchTime = 75,
        },

        new Research{
            name = "Disruptor",
            material = Faction.Draklid,
            category = Category.NewSpecialWeapon,
            researchTime = 90,
            dependencies = {"Draklid Weapons"},
        },

        new Research{
            name = "Assault Laser",
            material = Faction.Draklid,
            category = Category.NewWeapon,
            researchTime = 100,
            dependencies = {"Draklid Weapons"},
        },

        // Wertu tech tree.

        new Research{
            name = "Wertu Weapons I",
            material = Faction.Wertu,
            researchTime = 70,
        },

        new Research{
            name = "Photon Burst Cannon",
            material = Faction.Wertu,
            category = Category.NewWeapon,
            researchTime = 50,
            dependencies = {"Wertu Weapons I"},
        },

        new Research{
            name = "Wertu Weapons II",
            material = Faction.Wertu,
            researchTime = 110,
            dependencies = {"Wertu Weapons I"},
        },

        new Research{
            name = "Twin Photon Burst Cannon",
            material = Faction.Wertu,
            category = Category.NewWeapon,
            researchTime = 20,
            dependencies = {"Wertu Weapons II"},
        },

        new Research{
            name = "Shield Breaker",
            material = Faction.Wertu,
            category = Category.NewWeapon,
            researchTime = 65,
            dependencies = {"Wertu Weapons II"},
        },

        new Research{
            name = "Restructuring Ray",
            material = Faction.Wertu,
            category = Category.NewSpecialWeapon,
            researchTime = 40,
            dependencies = {"Wertu Weapons II"},
        },

        new Research{
            name = "Wertu Weapons III",
            material = Faction.Wertu,
            researchTime = 125,
            dependencies = {"Wertu Weapons II"},
        },

        new Research{
            name = "Photon Beam",
            material = Faction.Wertu,
            category = Category.NewSpecialWeapon,
            researchTime = 100,
            dependencies = {"Wertu Weapons III"},
        },

        new Research{
            name = "Plasma Emitter",
            material = Faction.Wertu,
            category = Category.NewWeapon,
            researchTime = 75,
            dependencies = {"Wertu Weapons III"},
        },

        // Vespion tech tree.

        new Research{
            name = "Vespion Weapons I",
            material = Faction.Vespion,
            researchTime = 100,
        },

        new Research{
            name = "Swarm Spawner",
            material = Faction.Vespion,
            category = Category.NewSpecialWeapon,
            researchTime = 60,
            dependencies = {"Vespion Weapons I"},
        },

        new Research{
            name = "Shockwave Caster",
            material = Faction.Vespion,
            category = Category.NewSpecialWeapon,
            researchTime = 75,
            dependencies = {"Vespion Weapons I"},
        },

        new Research{
            name = "Vespion Weapons II",
            material = Faction.Vespion,
            researchTime = 120,
            dependencies = {"Vespion Weapons I"},
        },

        new Research{
            name = "Hyper Cutter",
            material = Faction.Vespion,
            researchTime = 135,
            dependencies = {"Vespion Weapons II"},
        },

        // Rarilou tech tree.

        new Research{
            name = "Rarilou Weapons",
            material = Faction.Rarilou,
            researchTime = 125,
        },

        new Research{
            name = "Mjolnir",
            material = Faction.Rarilou,
            category = Category.NewSpecialWeapon,
            researchTime = 190,
            dependencies = {"Rarilou Weapons"},
        },

        // Phaa tech tree.

        new Research{
            name = "Phaa Weapons",
            material = Faction.Phaa,
            researchTime = 100,
        },

        new Research{
            name = "Bubble Gun",
            material = Faction.Phaa,
            category = Category.NewWeapon,
            researchTime = 85,
            dependencies = {"Phaa Weapons"},
        },

        // Krigia tech tree.

        new Research{
            name = "Krigia Weapons I",
            material = Faction.Krigia,
            researchTime = 100,
        },

        new Research{
            name = "Scythe",
            material = Faction.Krigia,
            category = Category.NewWeapon,
            researchTime = 40,
            dependencies = {"Krigia Weapons I"},
        },

        new Research{
            name = "Krigia Weapons II",
            material = Faction.Krigia,
            researchTime = 120,
            dependencies = {"Krigia Weapons I"},
        },

        new Research{
            name = "Great Scythe",
            material = Faction.Krigia,
            category = Category.NewWeapon,
            researchTime = 85,
            dependencies = {"Krigia Weapons II"},
        },

        new Research{
            name = "Hurricane",
            material = Faction.Krigia,
            category = Category.NewWeapon,
            researchTime = 110,
            dependencies = {
                "Krigia Weapons II",
                "Rocket Launcher",
            },
        },

        new Research{
            name = "Krigia Weapons III",
            material = Faction.Krigia,
            researchTime = 140,
            dependencies = {"Krigia Weapons II"},
        },

        new Research{
            name = "Lancer",
            material = Faction.Krigia,
            category = Category.NewWeapon,
            researchTime = 240,
            dependencies = {"Krigia Weapons III"},
        },

        new Research{
            name = "Torpedo Launcher",
            material = Faction.Krigia,
            category = Category.NewSpecialWeapon,
            researchTime = 130,
            dependencies = {"Krigia Weapons III", "Rocket Launcher"},
        },
    };
}
