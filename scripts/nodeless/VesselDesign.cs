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

    public int level;
    public string name;
    public string affiliation;
    public string description;
    public string extraDescription = "";

    public int sellingPrice;
    public int debris;
    public int productionTime;

    // Defense-related.
    public float maxHp;
    public int maxShieldLevel;

    // Mobility-related.
    public float maxSpeed;
    public float acceleration;
    public float rotationSpeed;

    // Slots.
    public bool specialSlot;
    public int weaponSlots;
    public int artifactSlots;

    // Size-related.
    public int cargoSpace;
    public Size size;

    public Texture Texture() {
        return GD.Load<Texture>($"res://images/vessel/{affiliation}_{name}.png");
    }

    public static VesselDesign Find(string affiliation, string name) {
        foreach (var d in list) {
            if (d.name == name && d.affiliation == affiliation) {
                return d;
            }
        }
        throw new Exception($"can't find {name}/{affiliation} vessel design");
    }

    public ItemKind Kind() { return ItemKind.VesselDesign; }

    public string RenderHelp() {
        var parts = new List<string>();
        parts.Add(affiliation + " " + name + " (" + sellingPrice.ToString() + ")");
        parts.Add("");
        parts.Add(description + ".");
        if (extraDescription != "") {
            parts.Add(extraDescription + ".");
        }
        parts.Add("");
        parts.Add("Level: " + level.ToString());
        parts.Add("Max hp: " + maxHp);
        parts.Add("Max shield level: " + maxShieldLevel);
        parts.Add("Max speed: " + maxSpeed);
        parts.Add("Acceleration: " + acceleration);
        parts.Add("Rotation speed: " + rotationSpeed);
        parts.Add("Weapon slots: " + weaponSlots.ToString());
        parts.Add("Special weapon slots: " + (specialSlot ? "1" : "0"));
        parts.Add("Artifact slots: " + artifactSlots.ToString());
        parts.Add("Cargo space: " + cargoSpace.ToString());
        parts.Add("Size: " + sizeText());
        return string.Join("\n", parts);
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

    public static VesselDesign[] list = {
        // Earthling designs.

        new VesselDesign{
            level = 1,
            name = "Scout",
            affiliation = "Earthling",
            description = "A highly maneuverable budget design",
            sellingPrice = 2200,
            debris = 30,
            productionTime = 10,

            maxHp = 110,
            maxShieldLevel = 1,

            maxSpeed = 95,
            acceleration = 2.4f,
            rotationSpeed = 2.0f,

            specialSlot = false,
            weaponSlots = 1,
            artifactSlots = 1,

            cargoSpace = 75,
            size = Size.Small,
        },

        new VesselDesign{
            level = 2,
            name = "Explorer",
            affiliation = "Earthling",
            description = "Simple, yet quite effective",
            sellingPrice = 3900,
            debris = 50,
            productionTime = 20,

            maxHp = 140,
            maxShieldLevel = 1,

            maxSpeed = 90,
            acceleration = 2.2f,
            rotationSpeed = 2,

            specialSlot = true,
            weaponSlots = 1,
            artifactSlots = 3,

            cargoSpace = 120,
            size = Size.Normal,
        },

        new VesselDesign{
            level = 3,
            name = "Freighter",
            affiliation = "Earthling",
            description = "Trade battle efficiency for a big cargo",
            sellingPrice = 4500,
            debris = 100,
            productionTime = 35,

            maxHp = 260,
            maxShieldLevel = 1,

            maxSpeed = 65,
            acceleration = 1.5f,
            rotationSpeed = 1.3f,

            specialSlot = false,
            weaponSlots = 1,
            artifactSlots = 3,

            cargoSpace = 190,
            size = Size.Large,
        },

        new VesselDesign{
            level = 3,
            name = "Fighter",
            affiliation = "Earthling",
            description = "Main Earthling military vessel design",
            sellingPrice = 5000,
            debris = 65,
            productionTime = 30,

            maxHp = 175,
            maxShieldLevel = 2,

            maxSpeed = 80,
            acceleration = 2.0f,
            rotationSpeed = 1.8f,

            specialSlot = false,
            weaponSlots = 2,
            artifactSlots = 3,

            cargoSpace = 50,
            size = Size.Normal,
        },

        new VesselDesign{
            level = 4,
            name = "Interceptor",
            affiliation = "Earthling",
            description = "Elite Earthling military vessel design",
            sellingPrice = 10500,
            debris = 75,
            productionTime = 50,

            maxHp = 200,
            maxShieldLevel = 2,

            maxSpeed = 95,
            acceleration = 3.5f,
            rotationSpeed = 2.2f,

            specialSlot = true,
            weaponSlots = 2,
            artifactSlots = 3,

            cargoSpace = 70,
            size = Size.Normal,
        },

        new VesselDesign{
            level = 4,
            name = "Ark",
            affiliation = "Earthling",
            description = "Transforms into a star base",
            sellingPrice = 15000,
            debris = 150,
            productionTime = 60,

            maxHp = 320,
            maxShieldLevel = 1,

            maxSpeed = 60,
            acceleration = 1.4f,
            rotationSpeed = 1.2f,

            specialSlot = true,
            weaponSlots = 0,
            artifactSlots = 1,

            cargoSpace = 50,
            size = Size.Large,
        },

        // Scavenger designs.

        new VesselDesign{
            level = 2,
            name = "Raider",
            affiliation = "Scavenger",
            description = "TODO",
            sellingPrice = 3200,
            debris = 45,
            productionTime = 25,
            
            maxHp = 120,
            maxShieldLevel = 1,

            maxSpeed = 105,
            acceleration = 4,
            rotationSpeed = 2,

            specialSlot = false,
            weaponSlots = 1,
            artifactSlots = 1,

            cargoSpace = 70,
            size = Size.Small,
        },

        new VesselDesign{
            level = 3,
            name = "Marauder",
            affiliation = "Scavenger",
            description = "TODO",
            sellingPrice = 6000,
            debris = 65,
            productionTime = 35,

            maxHp = 145,
            maxShieldLevel = 1,

            maxSpeed = 100,
            acceleration = 3,
            rotationSpeed = 2.5f,

            specialSlot = true,
            weaponSlots = 1,
            artifactSlots = 1,

            cargoSpace = 100,
            size = Size.Normal,
        },

        // Krigia designs.

        new VesselDesign{
            level = 1,
            name = "Talons",
            affiliation = "Krigia",
            description = "TODO",
            sellingPrice = 2000,
            debris = 45,
            productionTime = 15,

            maxHp = 95,
            maxShieldLevel = 1,

            maxSpeed = 100,
            acceleration = 3.7f,
            rotationSpeed = 2.8f,

            specialSlot = false,
            weaponSlots = 1,
            artifactSlots = 0,

            cargoSpace = 50,
            size = Size.Small,
        },

        new VesselDesign{
            level = 2,
            name = "Claws",
            affiliation = "Krigia",
            description = "TODO",
            sellingPrice = 3500,
            debris = 65,
            productionTime = 35,

            maxHp = 145,
            maxShieldLevel = 1,

            maxSpeed = 80,
            acceleration = 2.7f,
            rotationSpeed = 2.2f,

            specialSlot = false,
            weaponSlots = 2,
            artifactSlots = 1,

            cargoSpace = 60,
            size = Size.Normal,
        },

        new VesselDesign{
            level = 4,
            name = "Fangs",
            affiliation = "Krigia",
            description = "TODO",
            sellingPrice = 14500,
            debris = 110,
            productionTime = 70,

            maxHp = 195,
            maxShieldLevel = 2,

            maxSpeed = 100,
            acceleration = 3.2f,
            rotationSpeed = 2.7f,

            specialSlot = true,
            weaponSlots = 2,
            artifactSlots = 2,

            cargoSpace = 90,
            size = Size.Normal,
        },

        new VesselDesign{
            level = 6,
            name = "Tusks",
            affiliation = "Krigia",
            description = "TODO",
            sellingPrice = 18500,
            debris = 190,
            productionTime = 100,

            maxHp = 420,
            maxShieldLevel = 3,

            maxSpeed = 70,
            acceleration = 2.7f,
            rotationSpeed = 1.9f,

            specialSlot = true,
            weaponSlots = 2,
            artifactSlots = 4,

            cargoSpace = 200,
            size = Size.Large,
        },

        new VesselDesign{
            level = 7,
            name = "Horns",
            affiliation = "Krigia",
            description = "TODO",
            sellingPrice = 23700,
            debris = 220,
            productionTime = 110,

            maxHp = 300,
            maxShieldLevel = 3,

            maxSpeed = 105,
            acceleration = 7.0f,
            rotationSpeed = 2.5f,

            specialSlot = true,
            weaponSlots = 2,
            artifactSlots = 5,

            cargoSpace = 180,
            size = Size.Large,
        },

        new VesselDesign{
            level = 10,
            name = "Ashes",
            affiliation = "Krigia",
            description = "TODO",
            sellingPrice = 50000,
            debris = 500,
            productionTime = 300,

            maxHp = 850,
            maxShieldLevel = 3,

            maxSpeed = 80,
            acceleration = 2.5f,
            rotationSpeed = 1.6f,

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
            affiliation = "Wertu",
            description = "TODO",
            sellingPrice = 2000,
            debris = 50,
            productionTime = 20,

            maxHp = 135,
            maxShieldLevel = 1,

            maxSpeed = 115,
            acceleration = 2.3f,
            rotationSpeed = 1.2f,

            specialSlot = false,
            weaponSlots = 1,
            artifactSlots = 2,

            cargoSpace = 40,
            size = Size.Small,
        },

        new VesselDesign{
            level = 4,
            name = "Transporter",
            affiliation = "Wertu",
            description = "TODO",
            sellingPrice = 6600,
            debris = 130,
            productionTime = 60,

            maxHp = 280,
            maxShieldLevel = 2,

            maxSpeed = 80,
            acceleration = 1.2f,
            rotationSpeed = 1,

            specialSlot = false,
            weaponSlots = 1,
            artifactSlots = 4,

            cargoSpace = 250,
            size = Size.Large,
        },

        new VesselDesign{
            level = 5,
            name = "Guardian",
            affiliation = "Wertu",
            description = "TODO",
            sellingPrice = 15600,
            debris = 145,
            productionTime = 85,

            maxHp = 250,
            maxShieldLevel = 2,

            maxSpeed = 105,
            acceleration = 2.7f,
            rotationSpeed = 1.8f,

            specialSlot = true,
            weaponSlots = 2,
            artifactSlots = 3,

            cargoSpace = 120,
            size = Size.Normal,
        },

        new VesselDesign{
            level = 6,
            name = "Angel",
            affiliation = "Wertu",
            description = "TODO",
            sellingPrice = 16000,
            debris = 165,
            productionTime = 90,

            maxHp = 285,
            maxShieldLevel = 1,

            maxSpeed = 110,
            acceleration = 3.5f,
            rotationSpeed = 2.5f,

            specialSlot = true,
            weaponSlots = 2,
            artifactSlots = 4,

            cargoSpace = 135,
            size = Size.Normal,
        },

        new VesselDesign{
            level = 7,
            name = "Dominator",
            affiliation = "Wertu",
            description = "TODO",
            sellingPrice = 23000,
            debris = 265,
            productionTime = 130,

            maxHp = 550,
            maxShieldLevel = 3,

            maxSpeed = 95,
            acceleration = 2.5f,
            rotationSpeed = 1.5f,

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
            affiliation = "Zyth",
            description = "TODO",
            sellingPrice = 8900,
            debris = 125,
            productionTime = 55,

            maxHp = 190,
            maxShieldLevel = 1,

            maxSpeed = 110,
            acceleration = 4.5f,
            rotationSpeed = 2.5f,

            specialSlot = true,
            weaponSlots = 1,
            artifactSlots = 4,

            cargoSpace = 80,
            size = Size.Normal,
        },

        // Neutral designs.

        new VesselDesign{
            level = 2,
            name = "Pirate",
            affiliation = "Neutral",
            description = "",
            sellingPrice = 3500,
            debris = 50,
            productionTime = 20,

            maxHp = 100,
            maxShieldLevel = 2,

            maxSpeed = 85,
            acceleration = 2,
            rotationSpeed = 1.6f,

            specialSlot = false,
            weaponSlots = 2,
            artifactSlots = 2,

            cargoSpace = 100,
            size = Size.Normal,
        },

        new VesselDesign{
            level = 4,
            name = "Nomad",
            affiliation = "Neutral",
            description = "TODO",
            sellingPrice = 12700,
            debris = 160,
            productionTime = 65,

            maxHp = 180,
            maxShieldLevel = 2,

            maxSpeed = 108,
            acceleration = 2.5f,
            rotationSpeed = 2.0f,

            specialSlot = false,
            weaponSlots = 2,
            artifactSlots = 4,

            cargoSpace = 140,
            size = Size.Normal,
        },

        new VesselDesign{
            level = 5,
            name = "Avenger",
            affiliation = "Neutral",
            description = "TODO",
            sellingPrice = 16500,
            debris = 200,
            productionTime = 75,

            maxHp = 235,
            maxShieldLevel = 1,

            maxSpeed = 105,
            acceleration = 2.9f,
            rotationSpeed = 2.9f,

            specialSlot = true,
            weaponSlots = 2,
            artifactSlots = 5,

            cargoSpace = 130,
            size = Size.Normal,
        },

        new VesselDesign{
            level = 10,
            name = "Slayer",
            affiliation = "Neutral",
            description = "TODO",
            sellingPrice = 42000,
            debris = 420,
            productionTime = 185,

            maxHp = 520,
            maxShieldLevel = 3,

            maxSpeed = 100,
            acceleration = 3.2f,
            rotationSpeed = 2.3f,

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
            affiliation = "Unique",
            description = "TODO",
            sellingPrice = 20000,
            debris = 270,
            productionTime = 135,

            maxHp = 190,
            maxShieldLevel = 3,

            maxSpeed = 110,
            acceleration = 2.5f,
            rotationSpeed = 3.0f,

            specialSlot = true,
            weaponSlots = 2,
            artifactSlots = 4,

            cargoSpace = 100,
            size = Size.Normal,
        },
    };
}