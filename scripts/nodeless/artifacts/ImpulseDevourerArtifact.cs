using Godot;

public class ImpulseDevourerArtifact : IArtifact {
    public static ArtifactDesign Design = new ArtifactDesign{
        name = "Impulse Devourer",
        description = "TODO",
        effect = "convert shield-blocked damage into energy",
        sellingPrice = 1200,
    };

    public void Apply(VesselState state, float delta) {}

    public void Upgrade(VesselState state) {
        state.hasImpulseDevourer = true;
    }
}
