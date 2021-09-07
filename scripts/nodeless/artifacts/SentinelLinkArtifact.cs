using Godot;

public class SentinelLinkArtifact : IArtifact {
    public static ArtifactDesign Design = new ArtifactDesign{
        name = "Sentinel Link",
        description = "TODO",
        effect = "transfer 50% received damage to the sentinel",
        sellingPrice = 2500,
    };

    public void Apply(VesselState state, float delta) {}

    public void Upgrade(VesselState state) {
        state.hasSentinelLink = true;
    }
}
