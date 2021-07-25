using Godot;

public class EnergyConverterArtifact : IArtifact {
    public static ArtifactDesign Design = new ArtifactDesign{
        name = "Energy Converter",
        description = "Redirects otherwise wasted produced energy into the backup batteries",
        effect = "+1 backup energy/sec when primary energy is full",
        sellingPrice = 2500,
    };

    public void Apply(VesselState state, float delta) {
        if (state.energy >= state.stats.maxEnergy) {
            state.backupEnergy += delta;
            if (state.backupEnergy >= state.stats.maxBackupEnergy) {
                state.backupEnergy = state.stats.maxBackupEnergy;
            }
        }
    }

    public void Upgrade(VesselState state) {}
}
