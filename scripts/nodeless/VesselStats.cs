// Calculated vessel stats.
// Depends on the vessel design, equipment and statuses.
public class VesselStats {
    public float maxEnergy;
    public float maxBackupEnergy;
    public float energyRegen;

    public float electromagneticDamageReceived = 0;
    public float kineticDamageReceived = 0;
    public float thermalDamageReceived = 0;
    public float starDamageReceived = 0;

    public float maxSpeed;
    public float acceleration;

    public float maxHp;

    public VesselStats(Vessel v) {
        var battery = v.GetEnergySource();
        maxEnergy = 15 + battery.maxEnergy;
        maxBackupEnergy = battery.maxBackupEnergy;
        energyRegen = 0.5f + battery.energyRegen;

        var design = v.Design();
        maxHp = design.maxHp;
        maxSpeed = design.maxSpeed;
        acceleration = design.acceleration;

        foreach (var statusName in v.statusList) {
            var p = VesselStatus.statusByName[statusName];
            maxHp += p.maxHp;
            maxSpeed += p.maxSpeed;
            acceleration += p.acceleration;
            maxBackupEnergy += p.maxBackupEnergy;
            electromagneticDamageReceived += p.electromagneticDamageReceived;
            kineticDamageReceived += p.kineticDamageReceived;
            thermalDamageReceived += p.thermalDamageReceived;
            starDamageReceived += p.starDamageReceived;
            energyRegen += p.energyRegen;
        }
    }
}
