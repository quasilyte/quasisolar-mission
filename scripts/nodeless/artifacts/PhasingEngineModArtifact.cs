using Godot;

public class PhasingEngineModArtifact : IArtifact {
    public static ArtifactDesign Design = new ArtifactDesign{
        name = "Phasing Engine Mod",
        description = "Very rare engine modification that allows to overcome its constraints",
        effect = "instant acceleration",
        sellingPrice = 3000,
    };

    public void Apply(VesselState state, float delta) {}

    public void Upgrade(VesselState state) {
        state.acceleration = 9999;
    }
}
