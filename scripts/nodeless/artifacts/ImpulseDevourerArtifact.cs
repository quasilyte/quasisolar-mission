using Godot;

public class ImpulseDevourerArtifact : IArtifact {
    public static ArtifactDesign Design = new ArtifactDesign{
        name = "Impulse Devourer",
        description = "TODO",
        effect2 = "convert shield-blocked energy damage into energy",
        sellingPrice = 1600,
    };

    public void Apply(VesselState state, float delta) {}

    public void Upgrade(VesselState state) {}
}
