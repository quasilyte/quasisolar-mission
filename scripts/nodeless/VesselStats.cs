// Calculated vessel stats.
// Depends on the vessel design, equipment and patches.
public class VesselStats {
    public float maxEnergy;
    public float maxBackupEnergy;
    public float energyRegen;

    public float energyDamageReceived = 0;
    public float kineticDamageReceived = 0;
    public float thermalDamageReceived = 0;
    public float starDamageReceived = 0;

    public float maxSpeed;

    public float maxHp;

    public VesselStats(Vessel v) {
        var battery = v.GetEnergySource();
        maxEnergy = 15 + battery.maxEnergy;
        maxBackupEnergy = battery.maxBackupEnergy;
        energyRegen = 0.5f + battery.energyRegen;

        var design = v.Design();
        maxHp = design.maxHp;
        maxSpeed = design.maxSpeed;

        foreach (var patchName in v.patches) {
            var p = VesselPatch.patchByName[patchName];
            maxHp += p.maxHp;
            maxSpeed += p.maxSpeed;
            maxBackupEnergy += p.maxBackupEnergy;
            energyDamageReceived += p.energyDamageReceived;
            kineticDamageReceived += p.kineticDamageReceived;
            thermalDamageReceived += p.thermalDamageReceived;
            starDamageReceived += p.starDamageReceived;
        }
    }
}
