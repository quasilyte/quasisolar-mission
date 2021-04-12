using Godot;

public class MagneticNegatorArtifact : IArtifact {
    public static ArtifactDesign Design = new ArtifactDesign{
        name = "Magnetic Negator",
        description = "Surrounds the ship with distortion cloud",
        effect2 = "50% less energy loss when hit by Ion Cannon",
        effect = "50% less speed reduction when hit by Stinger",
        sellingPrice = 3900,
    };

    public void Apply(VesselState state, float delta) {}

    public void Upgrade(VesselState state) {}
}
