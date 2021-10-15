using Godot;

public class PointDefenseSaturatorArtifact : IArtifact {
    public static ArtifactDesign Design = new ArtifactDesign{
        name = "Point Defense Saturator",
        description = "TODO",
        effect = "-25% Point-Defense Laser cooldown",
        effect2 = "+5 Point-Defense Laser damage",
        sellingPrice = 4200,
    };

    public void Apply(VesselState state, float delta) {}

    public void Upgrade(VesselState state) {
        state.hasPointDefenseSaturator = true;
    }
}
