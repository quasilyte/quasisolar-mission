using Godot;

public class CloakingDeviceArtifact : IArtifact {
    public static ArtifactDesign Design = new ArtifactDesign{
        name = "Cloaking Device",
        description = "TODO",
        effect = "rockets can't track the wielder",
        sellingPrice = 4000,
    };

    public void Apply(VesselState state, float delta) {}

    public void Upgrade(VesselState state) {}
}
