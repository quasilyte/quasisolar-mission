using Godot;

// Beam weapons so far:
// - Zap
// - Pulse Blade
// - Photon Beam

public class BeamAmplifierArtifact : IArtifact {
    public static ArtifactDesign Design = new ArtifactDesign{
        name = "Beam Amplifier",
        description = "TODO",
        effect = "+15% beam weapons range",
        effect2 = "-20% beam weapons energy cost",
        sellingPrice = 3200,
    };

    public void Apply(VesselState state, float delta) {}

    public void Upgrade(VesselState state) {
        state.hasBeamAmplifier = true;
    }
}
