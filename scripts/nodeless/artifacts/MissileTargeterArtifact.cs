using Godot;

public class MissileTargeterArtifact : IArtifact {
    public static ArtifactDesign Design = new ArtifactDesign{
        name = "Missile Targeter",
        description = "TODO",
        effect = "+15% rocket weapon range",
        effect2 = "+10% rockets speed",
        sellingPrice = 4000,
    };

    public void Apply(VesselState state, float delta) {}

    public void Upgrade(VesselState state) {
        state.hasMissleTargeter = true;
    }
}
