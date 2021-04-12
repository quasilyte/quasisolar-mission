using Godot;

public class EngineBoosterArtifact : IArtifact {
    public static ArtifactDesign Design = new ArtifactDesign{
        name = "Engine Booster",
        description = "Extra engine components that make it possible to reach higher speeds",
        effect = "+15 max speed",
        sellingPrice = 1900,
    };

    public void Apply(VesselState state, float delta) {}

    public void Upgrade(VesselState state) {
        state.maxSpeed += 15.0f;
    }
}
