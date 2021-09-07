using Godot;

public class SentinelControllerArtifact : IArtifact {
    public static ArtifactDesign Design = new ArtifactDesign{
        name = "Sentinel Controller",
        description = "TODO",
        effect = "+50% sentinel max hp",
        sellingPrice = 3500,
    };

    public void Apply(VesselState state, float delta) {}
    public void Upgrade(VesselState state) {}
}
