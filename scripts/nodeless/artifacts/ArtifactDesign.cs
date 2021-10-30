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

    public ItemKind GetItemKind() { return ItemKind.Artifact; }

    public static Dictionary<string, ArtifactDesign> artifactByName;

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
        return artifactByName[name];
    }

    public static void InitLists() {
        list = new ArtifactDesign[]{
            LaserAbsorberArtifact.Design,
            AsynchronousReloaderArtifact.Design,
            DivioryThrusterArtifact.Design,
            DroidArtifact.Design,
            EnergyConverterArtifact.Design,
            EngineBoosterArtifact.Design,
            MagneticNegatorArtifact.Design,
            MissileCoordinatorArtifact.Design,
            MissileTargeterArtifact.Design,
            ShivaRechargerArtifact.Design,
            ImpulseDevourerArtifact.Design,
            ShieldProlongerArtifact.Design,
            SentinelControllerArtifact.Design,
            SentinelLinkArtifact.Design,
            KineticAcceleratorArtifact.Design,
            PointDefenseSaturatorArtifact.Design,
            IonCannonSaturatorArtifact.Design,
            PhotoniumArtifact.Design,
            BeamAmplifierArtifact.Design,
        };
        Array.Sort(list, (x, y) => x.sellingPrice.CompareTo(y.sellingPrice));

        artifactByName = new Dictionary<string, ArtifactDesign>();
        foreach (var a in list) {
            artifactByName.Add(a.name, a);
        }
        artifactByName.Add("Empty", EmptyArtifact.Design);
    }
}
