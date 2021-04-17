using System.Collections.Generic;
using System;

public class ShieldDesign : AbstractItem {
    public static ShieldDesign[] list;

    public string name = "";
    public string description = "";
    public string extraDescription = "";

    public int level;
    public List<string> technologiesNeeded = new List<string>();
    
    public float activeEnergyDamageReceive = 1;
    public float activeKineticDamageReceive = 1;
    public float activeThermalDamageReceive = 1;

    public float duration = 0;
    public float cooldown = 0;
    public float energyCost = 0;

    public int sellingPrice = 0;

    public bool researchRequired = false;

    public bool visualAuraRotates = false;

    public override ItemKind Kind() { return ItemKind.Shield; }

    public override string RenderHelp() {
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
        if (activeEnergyDamageReceive != 1) {
            parts.Add("Energy damage reduction: " + (int)Math.Round(100 * (1.0 - activeEnergyDamageReceive)) + "%");
        }
        if (activeKineticDamageReceive != 1) {
            parts.Add("Kinetic damage reduction: " + (int)Math.Round(100 * (1.0 - activeKineticDamageReceive)) + "%");
        }
        if (activeThermalDamageReceive != 1) {
            parts.Add("Thermal damage reduction: " + (int)Math.Round(100 * (1.0 - activeThermalDamageReceive)) + "%");
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
            ReflectorShield.Design,
            LaserPerimeterShield.Design,
            LatticeShield.Design,

            PhaserShield.Design,
        };
        Array.Sort(list, (x, y) => x.sellingPrice.CompareTo(y.sellingPrice));
    }
}
