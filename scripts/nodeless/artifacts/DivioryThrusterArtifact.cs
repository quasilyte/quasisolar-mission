using Godot;

public class DivioryThrusterArtifact : IArtifact {
    public static ArtifactDesign Design = new ArtifactDesign{
        name = "Diviory Thruster",
        description = "TODO",
        effect = "+30% vessel rotation speed",
        sellingPrice = 3000,
    };

    public void Apply(VesselState state, float delta) {}

    public void Upgrade(VesselState state) {
        state.stats.rotationSpeed *= 1.30f;
    }
}
