using Godot;

public class PhotoniumArtifact : IArtifact {
    public static ArtifactDesign Design = new ArtifactDesign{
        name = "Photonium",
        description = "TODO",
        effect = "+1 burst shot for photon cannons",
        sellingPrice = 3100,
    };

    public void Apply(VesselState state, float delta) {}

    public void Upgrade(VesselState state) {
        state.hasPhotonium = true;
    }
}
