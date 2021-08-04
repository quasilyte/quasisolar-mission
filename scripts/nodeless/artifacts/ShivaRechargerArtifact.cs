using Godot;

public class ShivaRechargerArtifact : IArtifact {
    public static ArtifactDesign Design = new ArtifactDesign{
        name = "Shiva Recharger",
        description = "A shield recharging module",
        effect = "-33% shield cooldown time",
        sellingPrice = 3100,
    };

    public void Apply(VesselState state, float delta) {}

    public void Upgrade(VesselState state) {
        state.shieldCooldownRate *= 0.66f;
    }
}
