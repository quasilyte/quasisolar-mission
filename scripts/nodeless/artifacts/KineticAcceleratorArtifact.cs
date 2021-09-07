using Godot;

public class KineticAcceleratorArtifact : IArtifact {
    public static ArtifactDesign Design = new ArtifactDesign{
        name = "Kinetic Accelerator",
        description = "TODO",
        effect2 = "25% chance to do double damage with kinetic weapon",
        sellingPrice = 6500,
    };

    public static float chance = 0.25f;

    public void Apply(VesselState state, float delta) {}

    public void Upgrade(VesselState state) {
        state.hasKineticAccelerator = true;
    }
}
