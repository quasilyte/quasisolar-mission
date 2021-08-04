using Godot;

public class LaserAbsorberArtifact : IArtifact {
    public static ArtifactDesign Design = new ArtifactDesign{
        name = "Laser Absorber",
        description = "TODO",
        effect2 = "15% chance to block incoming energy damage",
        sellingPrice = 7000,
    };

    public static float chance = 0.15f;

    public void Apply(VesselState state, float delta) {}

    public void Upgrade(VesselState state) {}
}
