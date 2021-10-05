using Godot;
using System;
using System.Collections.Generic;

// VesselDesign is a vessel blueprint that is used
// to initialize the initial vessel state.
// It's also used to infer the vessel capabilities.
//
// Max number of artifacts: 5.
// Max number of weapons: 2.
public class VesselDesign: IItem {
    public enum Size {
        Small,
        Normal,
        Large,
    };

    public enum ProductionAvailability {
        Never,
        ResearchRequired,
        Always,
    }

    public int level;
    public string name;
    public Faction affiliation;
    public string description;
    public string extraDescription = "";

    public bool canDestroyBase = false;

    public int sellingPrice;
    public int debris;
    public int productionTime;

    public ProductionAvailability availability = ProductionAvailability.Never;

    // Defense-related.
    public float maxHp;
    public int maxShieldLevel;

    // Mobility-related.
    public float maxSpeed;
    public float acceleration;
    public float rotationSpeed;

    // Slots.
    public bool specialSlot;
    public bool sentinelSlot;
    public int weaponSlots;
    public int artifactSlots;

    // Size-related.
    public int cargoSpace;
    public Size size;

    // Special properties.
    public bool fullArc = false;

    public Texture Texture() {
        return GD.Load<Texture>($"res://images/vessel/{affiliation}_{name}.png");
    }

    public static VesselDesign Find(string name) {
        if (designByName.ContainsKey(name)) {
            return designByName[name];
        }
        throw new Exception($"can't find {name} vessel design");
    }

    public ItemKind GetItemKind() { return ItemKind.VesselDesign; }

    public string RenderHelp(int price = 0) {
        if (price == 0) {
            price = sellingPrice;
        }
        var parts = new List<string>();
        parts.Add(affiliation + " " + name + " (" + price.ToString() + ")");
        parts.Add("");
        parts.Add(description + ".");
        if (extraDescription != "") {
            parts.Add(extraDescription + ".");
        }
        parts.Add("");
        parts.Add("Level: " + level.ToString());
        parts.Add("Max hp: " + maxHp);
        parts.Add("Max shield level: " + maxShieldLevel);
        parts.Add($"Speed: {maxSpeed} max, {acceleration} acceleration, {rotationSpeed} rotation");
        if (specialSlot) {
            parts.Add($"Weapon slots: {weaponSlots} + special");
        } else {
            parts.Add($"Weapon slots: {weaponSlots}");
        }
        parts.Add("Slots: " + SlotsText());
        parts.Add($"Size: {sizeText()} hull, {cargoSpace} cargo space");
        return string.Join("\n", parts);
    }

    private string SlotsText() {
        var parts = new List<string>();
        if (sentinelSlot) {
            parts.Add("sentinel");
        }
        if (artifactSlots != 0) {
            string suffix = artifactSlots == 1 ? "" : "s";
            parts.Add($"{artifactSlots} artifact" + suffix);
        }
        if (parts.Count == 0) {
            return "none";
        }
        return string.Join(", ", parts);
    }

    private string sizeText() {
        if (size == Size.Small) {
            return "small";
        }
        if (size == Size.Normal) {
            return "normal";
        }
        return "large";
    }

    public int PriceHintApprox() {
        double m = 1.0;

        if (weaponSlots == 0 && specialSlot) {
            m -= 0.3;
        } else if (weaponSlots == 1 && !specialSlot) {
            // no changes
        } else if (weaponSlots == 1 && specialSlot) {
            m += 0.9;
        } else if (weaponSlots == 2 && !specialSlot) {
            m += 0.85;
        } else if (weaponSlots == 2 && specialSlot) {
            m += 2.6;
        }

        if (maxShieldLevel == 0) {
            m -= 0.3;
        } else if (maxShieldLevel == 1) {
            // no changes
        } else if (maxShieldLevel == 2) {
            m += 0.3;
        } else if (maxShieldLevel == 3) {
            m += 0.75;
        }

        if (acceleration <= 1) {
            m -= 0.1;
        } else if (acceleration <= 2) {
            // no changes
        } else if (acceleration <= 3) {
            m += 0.08;
        } else if (acceleration <= 4) {
            m += 0.15;
        } else if (acceleration <= 5) {
            m += 0.22;
        } else {
            m += 0.25;
        }

        if (rotationSpeed <= 1) {
            m -= 0.3;
        } else if (rotationSpeed <= 1.5) {
            m -= 0.2;
        } else if (rotationSpeed <= 2) {
            // no chages
        } else if (rotationSpeed <= 2.5) {
            m += 0.1;
        } else if (rotationSpeed <= 3) {
            m += 0.17;
        } else if (rotationSpeed <= 3.5) {
            m += 0.23;
        } else if (rotationSpeed <= 4.0) {
            m += 0.29;
        } else if (rotationSpeed <= 4.5) {
            m += 0.35;
        } else {
            m += 0.41;
        }

        if (sentinelSlot) {
            m += 0.45;
        }

        if (artifactSlots == 0) {
            m -= 0.2;
        } else if (artifactSlots == 1) {
            m -= 0.05;
        } else if (artifactSlots == 2) {
            // no changes
        } else if (artifactSlots == 3) {
            m += 0.15;
        } else if (artifactSlots == 4) {
            m += 0.45;
        } else if (artifactSlots == 5) {
            m += 0.7;
        }

        if (maxSpeed <= 60) {
            m -= 0.1;
        } else if (maxSpeed <= 70) {
            // no changes
        } else if (maxSpeed <= 80) {
            m += 0.05;
        } else if (maxSpeed <= 85) {
            m += 0.1;
        } else if (maxSpeed <= 90) {
            m += 0.2;
        } else if (maxSpeed <= 95) {
            m += 0.3;
        } else if (maxSpeed <= 100) {
            m += 0.4;
        } else if (maxSpeed <= 105) {
            m += 0.55;
        } else if (maxSpeed <= 110) {
            m += 0.7;
        } else {
            m += 1;
        }

        m += ((double)cargoSpace / 125.0);

        if (size == Size.Small) {
            m += 0.1;
        } else if (size == Size.Normal) {
            m -= 0.05;
        } else if (size == Size.Large) {
            m -= 0.25;
        }

        if (productionTime <= 50) {
            m += 0.2;
        } else if (productionTime <= 100) {
            // no changes
        } else if (productionTime <= 150) {
            m -= 0.15;
        } else {
            m -= 0.4;
        }

        if (canDestroyBase) {
            m += 1.5;
        }

        double baseHpCost = 14 - (level * 0.5);
        return (int)((baseHpCost * m) * (double)maxHp);
    }

    private static Dictionary<string, VesselDesign> designByName;

    public static void InitLists() {
        Array.Sort(list, (x, y) => x.sellingPrice.CompareTo(y.sellingPrice));
        designByName = new Dictionary<string, VesselDesign>();
        foreach (var d in list) {
            designByName.Add(d.name, d);
        }
    }

    public static VesselDesign[] list = {
        // Earthling designs.

        new VesselDesign{
            level = 1,
            name = "Scout",
            affiliation = Faction.Earthling,
            description = "A highly maneuverable budget design",
            sellingPrice = 2500,
            debris = 30,
            productionTime = 20,
            availability = ProductionAvailability.Always,

            maxHp = 110,
            maxShieldLevel = 1,

            maxSpeed = 95,
            acceleration = 2.4f,
            rotationSpeed = 2.0f,

            sentinelSlot = false,
            specialSlot = false,
            weaponSlots = 1,
            artifactSlots = 1,

            cargoSpace = 75,
            size = Size.Small,
        },

        new VesselDesign{
            level = 3,
            name = "Explorer",
            affiliation = Faction.Earthling,
            description = "Simple, yet quite effective",
            sellingPrice = 5000,
            debris = 50,
            productionTime = 50,
            availability = ProductionAvailability.Always,

            maxHp = 140,
            maxShieldLevel = 1,

            maxSpeed = 90,
            acceleration = 2.2f,
            rotationSpeed = 2,

            sentinelSlot = true,
            specialSlot = true,
            weaponSlots = 1,
            artifactSlots = 3,

            cargoSpace = 120,
            size = Size.Normal,
        },

        new VesselDesign{
            level = 3,
            name = "Freighter",
            affiliation = Faction.Earthling,
            description = "Trade battle efficiency for a big cargo",
            sellingPrice = 4500,
            debris = 100,
            productionTime = 75,
            availability = ProductionAvailability.Always,

            maxHp = 260,
            maxShieldLevel = 1,

            maxSpeed = 65,
            acceleration = 1.5f,
            rotationSpeed = 1.3f,

            sentinelSlot = false,
            specialSlot = false,
            weaponSlots = 1,
            artifactSlots = 3,

            cargoSpace = 190,
            size = Size.Large,
        },

        new VesselDesign{
            level = 3,
            name = "Fighter",
            affiliation = Faction.Earthling,
            description = "Main Earthling military vessel design",
            sellingPrice = 5000,
            debris = 65,
            productionTime = 65,
            availability = ProductionAvailability.Always,

            maxHp = 175,
            maxShieldLevel = 2,

            maxSpeed = 80,
            acceleration = 2.0f,
            rotationSpeed = 1.8f,

            sentinelSlot = false,
            specialSlot = false,
            weaponSlots = 2,
            artifactSlots = 3,

            cargoSpace = 50,
            size = Size.Normal,
        },

        new VesselDesign{
            level = 4,
            name = "Bomber",
            affiliation = Faction.Earthling,
            description = "A siege vessel that can destroy a star base",
            sellingPrice = 9500,
            debris = 90,
            productionTime = 150,
            availability = ProductionAvailability.ResearchRequired,

            canDestroyBase = true,

            maxHp = 260,
            maxShieldLevel = 2,

            maxSpeed = 70,
            acceleration = 2,
            rotationSpeed = 1.6f,

            sentinelSlot = true,
            specialSlot = true,
            weaponSlots = 0,
            artifactSlots = 2,

            cargoSpace = 50,
            size = Size.Large,
        },

        new VesselDesign{
            level = 4,
            name = "Interceptor",
            affiliation = Faction.Earthling,
            description = "Elite Earthling military vessel design",
            sellingPrice = 10000,
            debris = 75,
            productionTime = 100,
            availability = ProductionAvailability.ResearchRequired,

            maxHp = 200,
            maxShieldLevel = 1,

            maxSpeed = 95,
            acceleration = 3.5f,
            rotationSpeed = 2.2f,

            sentinelSlot = false,
            specialSlot = true,
            weaponSlots = 2,
            artifactSlots = 3,

            cargoSpace = 70,
            size = Size.Normal,
        },

        new VesselDesign{
            level = 5,
            name = "Gladiator",
            affiliation = Faction.Earthling,
            description = "Alien-design inspired battle ship",
            sellingPrice = 12000,
            debris = 100,
            productionTime = 120,
            availability = ProductionAvailability.ResearchRequired,

            maxHp = 270,
            maxShieldLevel = 3,

            maxSpeed = 90,
            acceleration = 2.6f,
            rotationSpeed = 2.5f,

            sentinelSlot = true,
            specialSlot = true,
            weaponSlots = 1,
            artifactSlots = 4,

            cargoSpace = 110,
            size = Size.Normal,
        },

        // + HP
        // + Speed
        // + Sentinel
        // + Cargo space
        // - Increased production time
        // - Acceleration
        // - Weapon slot
        new VesselDesign{
            level = 6,
            name = "Valkyrie",
            affiliation = Faction.Earthling,
            description = "Wertu-based cruiser design",
            sellingPrice = 22000,
            debris = 160,
            productionTime = 210,
            availability = ProductionAvailability.ResearchRequired,

            maxHp = 430,
            maxShieldLevel = 3,

            maxSpeed = 85,
            acceleration = 2,
            rotationSpeed = 2,

            sentinelSlot = true,
            specialSlot = true,
            weaponSlots = 1,
            artifactSlots = 4,

            cargoSpace = 140,
            size = Size.Large,
        },

        // + Acceleration
        // + Rotation speed
        // + Faster Production time
        // - Increased price
        // - HP
        new VesselDesign{
            level = 6,
            name = "Einherjar",
            affiliation = Faction.Earthling,
            description = "Krigia-based cruiser design",
            sellingPrice = 24500,
            debris = 160,
            productionTime = 180,
            availability = ProductionAvailability.ResearchRequired,

            maxHp = 330,
            maxShieldLevel = 3,

            maxSpeed = 75,
            acceleration = 4.5f,
            rotationSpeed = 3.5f,

            sentinelSlot = false,
            specialSlot = true,
            weaponSlots = 2,
            artifactSlots = 4,

            cargoSpace = 80,
            size = Size.Large,
        },

        // + Artifact slot
        // - Shield level
        new VesselDesign{
            level = 6,
            name = "Valhalla",
            affiliation = Faction.Earthling,
            description = "Rarilou-based cruiser design",
            sellingPrice = 22000,
            debris = 160,
            productionTime = 190,
            availability = ProductionAvailability.ResearchRequired,

            maxHp = 360,
            maxShieldLevel = 2,

            maxSpeed = 75,
            acceleration = 3,
            rotationSpeed = 2,

            sentinelSlot = false,
            specialSlot = true,
            weaponSlots = 2,
            artifactSlots = 5,

            cargoSpace = 80,
            size = Size.Large,
        },

        new VesselDesign{
            level = 7,
            name = "Ragnarok",
            affiliation = Faction.Earthling,
            description = "An ultimate Earthling vessel design",
            sellingPrice = 35000,
            debris = 200,
            productionTime = 250,
            availability = ProductionAvailability.ResearchRequired,

            maxHp = 450,
            maxShieldLevel = 3,

            maxSpeed = 90,
            acceleration = 4.5f,
            rotationSpeed = 4,

            sentinelSlot = true,
            specialSlot = true,
            weaponSlots = 2,
            artifactSlots = 5,

            cargoSpace = 250,
            size = Size.Large,
        },

        new VesselDesign{
            level = 4,
            name = "Ark",
            affiliation = Faction.Earthling,
            description = "Transforms into a star base",
            sellingPrice = 10000,
            debris = 150,
            productionTime = 100,
            availability = ProductionAvailability.ResearchRequired,

            maxHp = 320,
            maxShieldLevel = 1,

            maxSpeed = 60,
            acceleration = 1.4f,
            rotationSpeed = 1.2f,

            sentinelSlot = true,
            specialSlot = true,
            weaponSlots = 0,
            artifactSlots = 1,

            cargoSpace = 50,
            size = Size.Large,
        },

        // Draklid designs.

        new VesselDesign{
            level = 2,
            name = "Raider",
            affiliation = Faction.Draklid,
            description = "TODO",
            sellingPrice = 3200,
            debris = 45,
            productionTime = 30,
            
            maxHp = 120,
            maxShieldLevel = 1,

            maxSpeed = 105,
            acceleration = 4,
            rotationSpeed = 2,

            sentinelSlot = false,
            specialSlot = false,
            weaponSlots = 1,
            artifactSlots = 1,

            cargoSpace = 70,
            size = Size.Small,
        },

        new VesselDesign{
            level = 3,
            name = "Marauder",
            affiliation = Faction.Draklid,
            description = "TODO",
            sellingPrice = 6000,
            debris = 65,
            productionTime = 45,

            maxHp = 145,
            maxShieldLevel = 1,

            maxSpeed = 100,
            acceleration = 3.5f,
            rotationSpeed = 3,

            sentinelSlot = true,
            specialSlot = true,
            weaponSlots = 1,
            artifactSlots = 1,

            cargoSpace = 100,
            size = Size.Normal,
        },

        new VesselDesign{
            level = 5,
            name = "Plunderer",
            affiliation = Faction.Draklid,
            description = "TODO",
            sellingPrice = 12500,
            debris = 130,
            productionTime = 120,

            maxHp = 290,
            maxShieldLevel = 2,

            maxSpeed = 85,
            acceleration = 3,
            rotationSpeed = 2,

            sentinelSlot = true,
            specialSlot = true,
            weaponSlots = 1,
            artifactSlots = 4,

            cargoSpace = 220,
            size = Size.Large,
        },

        // Krigia designs.

        new VesselDesign{
            level = 1,
            name = "Talons",
            affiliation = Faction.Krigia,
            description = "TODO",
            sellingPrice = 1500,
            debris = 45,
            productionTime = 20,

            maxHp = 95,
            maxShieldLevel = 1,

            maxSpeed = 100,
            acceleration = 3.7f,
            rotationSpeed = 2.8f,

            sentinelSlot = false,
            specialSlot = false,
            weaponSlots = 1,
            artifactSlots = 0,

            cargoSpace = 50,
            size = Size.Small,
        },

        new VesselDesign{
            level = 2,
            name = "Claws",
            affiliation = Faction.Krigia,
            description = "TODO",
            sellingPrice = 3500,
            debris = 65,
            productionTime = 70,

            maxHp = 145,
            maxShieldLevel = 1,

            maxSpeed = 80,
            acceleration = 2.7f,
            rotationSpeed = 2.2f,

            sentinelSlot = false,
            specialSlot = false,
            weaponSlots = 2,
            artifactSlots = 1,

            cargoSpace = 60,
            size = Size.Normal,
        },

        new VesselDesign{
            level = 4,
            name = "Fangs",
            affiliation = Faction.Krigia,
            description = "TODO",
            sellingPrice = 11500,
            debris = 110,
            productionTime = 115,

            maxHp = 195,
            maxShieldLevel = 2,

            maxSpeed = 100,
            acceleration = 3.2f,
            rotationSpeed = 2.7f,

            sentinelSlot = true,
            specialSlot = true,
            weaponSlots = 2,
            artifactSlots = 2,

            cargoSpace = 90,
            size = Size.Normal,
        },

        new VesselDesign{
            level = 5,
            name = "Destroyer",
            affiliation = Faction.Krigia,
            description = "TODO",
            sellingPrice = 17000,
            debris = 120,
            productionTime = 210,

            canDestroyBase = true,

            maxHp = 360,
            maxShieldLevel = 2,

            maxSpeed = 75,
            acceleration = 3,
            rotationSpeed = 2,

            sentinelSlot = false,
            specialSlot = true,
            weaponSlots = 1,
            artifactSlots = 3,

            cargoSpace = 100,
            size = Size.Large,
        },

        new VesselDesign{
            level = 6,
            name = "Tusks",
            affiliation = Faction.Krigia,
            description = "TODO",
            sellingPrice = 18500,
            debris = 190,
            productionTime = 200,

            maxHp = 450,
            maxShieldLevel = 2,

            maxSpeed = 70,
            acceleration = 2.7f,
            rotationSpeed = 1.9f,

            sentinelSlot = true,
            specialSlot = true,
            weaponSlots = 2,
            artifactSlots = 4,

            cargoSpace = 200,
            size = Size.Large,
        },

        new VesselDesign{
            level = 7,
            name = "Horns",
            affiliation = Faction.Krigia,
            description = "TODO",
            sellingPrice = 22000,
            debris = 220,
            productionTime = 180,

            maxHp = 320,
            maxShieldLevel = 3,

            maxSpeed = 105,
            acceleration = 7.0f,
            rotationSpeed = 2.5f,

            sentinelSlot = true,
            specialSlot = true,
            weaponSlots = 2,
            artifactSlots = 5,

            cargoSpace = 180,
            size = Size.Normal,
        },

        new VesselDesign{
            level = 10,
            name = "Ashes",
            affiliation = Faction.Krigia,
            description = "TODO",
            sellingPrice = 50000,
            debris = 500,
            productionTime = 900,

            maxHp = 1000,
            maxShieldLevel = 3,

            maxSpeed = 80,
            acceleration = 2.5f,
            rotationSpeed = 1.6f,

            sentinelSlot = true,
            specialSlot = true,
            weaponSlots = 2,
            artifactSlots = 5,

            cargoSpace = 300,
            size = Size.Large,
        },

        // Wertu designs.

        new VesselDesign{
            level = 1,
            name = "Probe",
            affiliation = Faction.Wertu,
            description = "TODO",
            sellingPrice = 3000,
            debris = 50,
            productionTime = 30,

            maxHp = 135,
            maxShieldLevel = 1,

            maxSpeed = 120,
            acceleration = 2.3f,
            rotationSpeed = 1.2f,

            sentinelSlot = false,
            specialSlot = false,
            weaponSlots = 1,
            artifactSlots = 2,

            cargoSpace = 40,
            size = Size.Small,
        },

        new VesselDesign{
            level = 4,
            name = "Transporter",
            affiliation = Faction.Wertu,
            description = "TODO",
            sellingPrice = 7500,
            debris = 130,
            productionTime = 70,

            maxHp = 280,
            maxShieldLevel = 2,

            maxSpeed = 80,
            acceleration = 1.2f,
            rotationSpeed = 1,

            sentinelSlot = false,
            specialSlot = false,
            weaponSlots = 1,
            artifactSlots = 4,

            cargoSpace = 250,
            size = Size.Normal,
        },

        new VesselDesign{
            level = 5,
            name = "Guardian",
            affiliation = Faction.Wertu,
            description = "TODO",
            sellingPrice = 13000,
            debris = 145,
            productionTime = 130,

            maxHp = 250,
            maxShieldLevel = 2,

            maxSpeed = 105,
            acceleration = 2.7f,
            rotationSpeed = 1.8f,

            sentinelSlot = false,
            specialSlot = true,
            weaponSlots = 2,
            artifactSlots = 3,

            cargoSpace = 120,
            size = Size.Normal,
        },

        new VesselDesign{
            level = 6,
            name = "Angel",
            affiliation = Faction.Wertu,
            description = "TODO",
            sellingPrice = 14500,
            debris = 165,
            productionTime = 140,

            maxHp = 285,
            maxShieldLevel = 1,

            maxSpeed = 110,
            acceleration = 3.5f,
            rotationSpeed = 2.5f,

            sentinelSlot = true,
            specialSlot = true,
            weaponSlots = 2,
            artifactSlots = 4,

            cargoSpace = 135,
            size = Size.Normal,
        },

        new VesselDesign{
            level = 7,
            name = "Dominator",
            affiliation = Faction.Wertu,
            description = "TODO",
            sellingPrice = 23000,
            debris = 265,
            productionTime = 190,
            
            canDestroyBase = true,

            maxHp = 550,
            maxShieldLevel = 3,

            maxSpeed = 95,
            acceleration = 2.5f,
            rotationSpeed = 1.5f,

            sentinelSlot = true,
            specialSlot = false,
            weaponSlots = 2,
            artifactSlots = 5,

            cargoSpace = 260,
            size = Size.Large,
        },
        
        // Zyth designs.

        new VesselDesign{
            level = 3,
            name = "Hunter",
            affiliation = Faction.Zyth,
            description = "TODO",
            sellingPrice = 8000,
            debris = 125,
            productionTime = 105,

            maxHp = 190,
            maxShieldLevel = 1,

            maxSpeed = 110,
            acceleration = 4.5f,
            rotationSpeed = 2.5f,

            sentinelSlot = false,
            specialSlot = true,
            weaponSlots = 1,
            artifactSlots = 4,

            cargoSpace = 80,
            size = Size.Normal,
        },

        new VesselDesign{
            level = 7,
            name = "Invader",
            affiliation = Faction.Zyth,
            description = "TODO",
            sellingPrice = 27000,
            debris = 240,
            productionTime = 160,

            maxHp = 390,
            maxShieldLevel = 3,

            maxSpeed = 100,
            acceleration = 5.0f,
            rotationSpeed = 2f,

            sentinelSlot = true,
            specialSlot = true,
            weaponSlots = 2,
            artifactSlots = 4,

            cargoSpace = 160,
            size = Size.Large,
        },

        // Rarilou designs.

        new VesselDesign{
            level = 4,
            name = "Leviathan",
            affiliation = Faction.Rarilou,
            description = "TODO",
            sellingPrice = 9500,
            debris = 90,
            productionTime = 130,

            maxHp = 250,
            maxShieldLevel = 1,

            maxSpeed = 95,
            acceleration = 4,
            rotationSpeed = 3.5f,

            sentinelSlot = false,
            specialSlot = true,
            weaponSlots = 1,
            artifactSlots = 4,

            cargoSpace = 130,
            size = Size.Normal,
        },

        // Phaa designs.
        // - No artifact slots.
        // - Small cargo capacity.
        // + Smaller hulls.
        // + Good shields.

        new VesselDesign{
            level = 3,
            name = "Spacehopper",
            affiliation = Faction.PhaaRebel,
            description = "TODO",
            sellingPrice = 4900,
            debris = 55,
            productionTime = 85,
            availability = ProductionAvailability.ResearchRequired,

            maxHp = 165,
            maxShieldLevel = 3,

            maxSpeed = 90,
            acceleration = 2.2f,
            rotationSpeed = 2,

            sentinelSlot = false,
            specialSlot = false,
            weaponSlots = 2,
            artifactSlots = 0,

            cargoSpace = 40,
            size = Size.Small,
        },

        new VesselDesign{
            level = 3,
            name = "Mantis",
            affiliation = Faction.Phaa,
            description = "TODO",
            sellingPrice = 4900,
            debris = 55,
            productionTime = 85,
            availability = ProductionAvailability.Never,

            maxHp = 165,
            maxShieldLevel = 3,

            maxSpeed = 90,
            acceleration = 2.2f,
            rotationSpeed = 2,

            sentinelSlot = false,
            specialSlot = false,
            weaponSlots = 2,
            artifactSlots = 0,

            cargoSpace = 40,
            size = Size.Small,
        },

        // Vespion designs.
        // - Low hp.
        // - Low firepower.
        // + Cheap, fast to produce.
        // + High rotation speed.
        // + High cargo capacity.
        // + Many artifact slots.

        new VesselDesign{
            level = 1,
            name = "Larva",
            affiliation = Faction.Vespion,
            description = "TODO",
            sellingPrice = 1750,
            debris = 20,
            productionTime = 15,

            maxHp = 75,
            maxShieldLevel = 1,

            maxSpeed = 100,
            acceleration = 2.5f,
            rotationSpeed = 4,

            sentinelSlot = true,
            specialSlot = true,
            weaponSlots = 0,
            artifactSlots = 1,

            cargoSpace = 90,
            size = Size.Small,
        },

        new VesselDesign{
            level = 4,
            name = "Hornet",
            affiliation = Faction.Vespion,
            description = "TODO",
            sellingPrice = 7500,
            debris = 60,
            productionTime = 50,

            maxHp = 145,
            maxShieldLevel = 2,

            maxSpeed = 110,
            acceleration = 3,
            rotationSpeed = 4,

            sentinelSlot = false,
            specialSlot = true,
            weaponSlots = 1,
            artifactSlots = 4,

            cargoSpace = 195,
            size = Size.Normal,
        },

        new VesselDesign{
            level = 6,
            name = "Queen",
            affiliation = Faction.Vespion,
            description = "TODO",
            sellingPrice = 16000,
            debris = 105,
            productionTime = 120,

            maxHp = 290,
            maxShieldLevel = 3,

            maxSpeed = 95,
            acceleration = 3.2f,
            rotationSpeed = 4,

            sentinelSlot = true,
            specialSlot = true,
            weaponSlots = 1,
            artifactSlots = 5,

            cargoSpace = 275,
            size = Size.Large,
        },

        // Neutral designs.

        new VesselDesign{
            level = 3,
            name = "Ravager",
            affiliation = Faction.Neutral,
            description = "",
            sellingPrice = 6500,
            debris = 90,
            productionTime = 70,

            maxHp = 225,
            maxShieldLevel = 0,

            maxSpeed = 75,
            acceleration = 5,
            rotationSpeed = 2,

            sentinelSlot = true,
            specialSlot = true,
            weaponSlots = 1,
            artifactSlots = 2,

            cargoSpace = 100,
            size = Size.Normal,
        },

        new VesselDesign{
            level = 4,
            name = "Nomad",
            affiliation = Faction.Neutral,
            description = "TODO",
            sellingPrice = 9500,
            debris = 160,
            productionTime = 85,
            availability = ProductionAvailability.ResearchRequired,

            maxHp = 180,
            maxShieldLevel = 2,

            maxSpeed = 108,
            acceleration = 2.5f,
            rotationSpeed = 2.0f,

            sentinelSlot = true,
            specialSlot = false,
            weaponSlots = 2,
            artifactSlots = 4,

            cargoSpace = 140,
            size = Size.Normal,
        },

        new VesselDesign{
            level = 5,
            name = "Avenger",
            affiliation = Faction.Neutral,
            description = "TODO",
            sellingPrice = 14500,
            debris = 200,
            productionTime = 100,
            availability = ProductionAvailability.ResearchRequired,

            maxHp = 235,
            maxShieldLevel = 1,

            maxSpeed = 105,
            acceleration = 2.9f,
            rotationSpeed = 2.9f,

            sentinelSlot = false,
            specialSlot = true,
            weaponSlots = 2,
            artifactSlots = 5,

            cargoSpace = 130,
            size = Size.Normal,
        },

        new VesselDesign{
            level = 10,
            name = "Slayer",
            affiliation = Faction.Neutral,
            description = "TODO",
            sellingPrice = 42000,
            debris = 420,
            productionTime = 210,

            maxHp = 520,
            maxShieldLevel = 3,

            maxSpeed = 100,
            acceleration = 3.2f,
            rotationSpeed = 2.3f,

            sentinelSlot = true,
            specialSlot = true,
            weaponSlots = 2,
            artifactSlots = 5,

            cargoSpace = 130,
            size = Size.Large,
        },

        // Unique designs.

        new VesselDesign{
            level = 6,
            name = "Spectre",
            affiliation = Faction.Neutral,
            description = "TODO",
            sellingPrice = 12500,
            debris = 270,
            productionTime = 170,

            maxHp = 190,
            maxShieldLevel = 3,

            maxSpeed = 110,
            acceleration = 2.5f,
            rotationSpeed = 3.0f,

            sentinelSlot = false,
            specialSlot = true,
            weaponSlots = 2,
            artifactSlots = 4,

            cargoSpace = 100,
            size = Size.Normal,
        },

        new VesselDesign{
            level = 8,
            name = "Visitor",
            affiliation = Faction.Neutral,
            description = "TODO",
            sellingPrice = 35000,
            debris = 400,
            productionTime = 500,

            maxHp = 780,
            maxShieldLevel = 3,

            maxSpeed = 0,
            acceleration = 0,
            rotationSpeed = 0,

            sentinelSlot = true,
            specialSlot = true,
            weaponSlots = 2,
            artifactSlots = 3,

            cargoSpace = 0,
            size = Size.Large,

            fullArc = true,
        },
    };
}
