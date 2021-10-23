using Godot;

public class IonCannonSaturatorArtifact : IArtifact {
    public static ArtifactDesign Design = new ArtifactDesign{
        name = "Ion Cannon Saturator",
        description = "TODO",
        effect = "+4 Ion Cannon damage",
        sellingPrice = 2000,
    };

    public void Apply(VesselState state, float delta) {}

    public void Upgrade(VesselState state) {
        state.hasIonCannonSaturator = true;
    }
}
