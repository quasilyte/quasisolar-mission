using Godot;

public class MissileCoordinatorArtifact : IArtifact {
    public static ArtifactDesign Design = new ArtifactDesign{
        name = "Missile Coordinator",
        description = "TODO",
        effect = "+30% missiles homing",
        sellingPrice = 3000,
    };

    public static float multiplier = 0.3f;

    public void Apply(VesselState state, float delta) {}

    public void Upgrade(VesselState state) {
        state.hasMissleCoordinator = true;
    }
}
