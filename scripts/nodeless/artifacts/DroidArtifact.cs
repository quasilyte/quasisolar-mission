using Godot;

public class DroidArtifact : IArtifact {
    public static ArtifactDesign Design = new ArtifactDesign{
        name = "Droid",
        description = "A droid that repairs damaged hull",
        effect = "+0.5 hp/sec when hp<50%",
        sellingPrice = 4000,
    };

    public void Apply(VesselState state, float delta) {
        if (state.hp >= state.maxHp) {
            return;
        }
        if (state.hp < (state.maxHp / 2)) {
            state.hp += 0.5f * delta;
        }
    }

    public void Upgrade(VesselState state) {}
}
