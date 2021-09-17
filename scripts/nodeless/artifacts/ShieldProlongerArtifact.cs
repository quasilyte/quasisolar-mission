using Godot;

public class ShieldProlongerArtifact : IArtifact {
    public static ArtifactDesign Design = new ArtifactDesign{
        name = "Shield Prolonger",
        description = "TODO",
        effect = "+15% shield activation duration",
        sellingPrice = 4800,
    };

    public void Apply(VesselState state, float delta) {}

    public void Upgrade(VesselState state) {
        state.shieldDurationRate = 1.15f;
    }
}
