using System.Collections.Generic;
using System;

public class ShieldDesign : IItem {
    public static ShieldDesign[] list;

    public string name = "";
    public string description = "";
    public string extraDescription = "";

    public int level;
    public List<string> technologiesNeeded = new List<string>();
    
    public float activeElectromagneticDamageReceive = 1;
    public float activeKineticDamageReceive = 1;
    public float activeThermalDamageReceive = 1;

    public float duration = 0;
    public float cooldown = 0;
    public float energyCost = 0;

    public int sellingPrice = 0;

    public int hpBonus = 0;

    public bool researchRequired = false;

    public bool visualAuraRotates = false;

    public ItemKind GetItemKind() { return ItemKind.Shield; }

    private static Dictionary<string, ShieldDesign> shieldByName;

    public static ShieldDesign Find(string name) {
        return shieldByName[name];
    }

    public string RenderHelp() {
        if (name == "Empty") {
            // A special case.
            return "An empty shield slot";
        }

        var parts = new List<string>();
        parts.Add(name + " (" + sellingPrice.ToString() + ")");
        parts.Add("");
        parts.Add(description + ".");
        if (extraDescription != "") {
            parts.Add(extraDescription + ".");
        }
        parts.Add("");
        parts.Add("Level: " + level.ToString());
        if (activeElectromagneticDamageReceive != 1) {
            parts.Add("Electromagnetic damage reduction: " + (int)Math.Round(100 * (1.0 - activeElectromagneticDamageReceive)) + "%");
        }
        if (activeKineticDamageReceive != 1) {
            parts.Add("Kinetic damage reduction: " + (int)Math.Round(100 * (1.0 - activeKineticDamageReceive)) + "%");
        }
        if (activeThermalDamageReceive != 1) {
            parts.Add("Thermal damage reduction: " + (int)Math.Round(100 * (1.0 - activeThermalDamageReceive)) + "%");
        }
        if (hpBonus != 0) {
            parts.Add("Max hp bonus: +" + hpBonus);
        }
        parts.Add("Cooldown: " + cooldown.ToString("0.0") + " seconds");
        parts.Add("Duration: " + duration.ToString("0.0") + " seconds");
        if (energyCost != 0) {
            parts.Add("Energy cost: " + energyCost.ToString());
        }
        return string.Join("\n", parts);
    }

    public static void InitLists() {
        list = new ShieldDesign[]{
            IonCurtainShield.Design,
            HeatScreenShield.Design,

            DispersionFieldShield.Design,
            DeceleratorShield.Design,
            DeflectorShield.Design,
            LaserPerimeterShield.Design,
            LatticeShield.Design,

            PhaserShield.Design,
            DiffuserShield.Design,
            AegisShield.Design,
        };
        Array.Sort(list, (x, y) => x.sellingPrice.CompareTo(y.sellingPrice));
        shieldByName = new Dictionary<string, ShieldDesign>();
        foreach (var shield in list) {
            shieldByName.Add(shield.name, shield);
        }
        shieldByName.Add("Empty", EmptyShield.Design);
    }
}
