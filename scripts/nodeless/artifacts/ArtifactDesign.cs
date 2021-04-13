using System.Collections.Generic;
using System;

public class ArtifactDesign : IItem {
    public string name = "";
    public string description = "";
    public string extraDescription = "";
    public string effect = "";
    public string effect2 = "";

    public int sellingPrice = 0;

    public static ArtifactDesign[] list;

    public ItemKind Kind() { return ItemKind.Artifact; }

    public string RenderHelp() {
        if (name == "Empty") {
            // A special case.
            return "An empty artifact slot";
        }

        var parts = new List<string>();
        parts.Add(name + " (" + sellingPrice.ToString() + ")");
        parts.Add("");
        parts.Add(description + ".");
        if (extraDescription != "") {
            parts.Add(extraDescription + ".");
        }
        parts.Add("");
        parts.Add("Effect: " + effect);
        if (effect2 != "") {
            parts.Add("Effect: " + effect2);
        }
        parts.Add("");
        parts.Add("Note: effects from identical artifacts do not stack.");
        return string.Join("\n", parts);
    }

    public static ArtifactDesign Find(string name) {
        foreach (var d in list) {
            if (d.name == name) {
                return d;
            }
        }
        throw new Exception($"can't find {name} artifact design");
    }

    public static void InitLists() {
        list = new ArtifactDesign[]{
            CloakingDeviceArtifact.Design,
            DivioryThrusterArtifact.Design,
            DroidArtifact.Design,
            EnergyConverterArtifact.Design,
            EngineBoosterArtifact.Design,
            MagneticNegatorArtifact.Design,
            MissileTargeterArtifact.Design,
            PhasingEngineModArtifact.Design,
            ShivaRechargerArtifact.Design,
            ImpulseDevourerArtifact.Design,
            ShieldProlongerArtifact.Design,
        };
        Array.Sort(list, (x, y) => x.sellingPrice.CompareTo(y.sellingPrice));
    }
}