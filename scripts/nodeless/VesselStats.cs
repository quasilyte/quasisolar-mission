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
    public float asteroidDamageReceived = 0;

    public float shieldExtraActivationCost = 0;
    public float shieldDurationRate = 1;

    public float sentinelActionCooldownRate = 1;
    public float sentinelMaxHpRate = 1;
    public float sentinelMaxHpBonus = 0;

    public int luckModifier = 0;

    public float maxSpeed;
    public float acceleration;
    public float rotationSpeed;

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
        rotationSpeed = design.rotationSpeed;

        foreach (var modName in v.modList) {
            var mod = VesselMod.modByName[modName];
            if (mod.flagshipOnly && !v.isFlagship) {
                continue;
            }
            sentinelActionCooldownRate += mod.sentinelActionCooldown;
            sentinelMaxHpBonus += mod.sentinelMaxHp;
            maxHp += mod.maxHp;
            maxSpeed += mod.maxSpeed;
            acceleration += mod.acceleration;
            maxBackupEnergy += mod.maxBackupEnergy;
            electromagneticDamageReceived += mod.electromagneticDamageReceived;
            kineticDamageReceived += mod.kineticDamageReceived;
            thermalDamageReceived += mod.thermalDamageReceived;
            starDamageReceived += mod.starDamageReceived;
            asteroidDamageReceived += mod.asteroidDamageReceived;
            energyRegen += mod.energyRegen;
            shieldDurationRate += mod.shieldDurationRate;
            shieldExtraActivationCost += mod.shieldEnergyCost;
            rotationSpeed += mod.rotationSpeed;
            luckModifier += mod.luckModifier;
        }

        maxEnergy = QMath.ClampMin(maxEnergy, 15);
        maxBackupEnergy = QMath.ClampMin(maxBackupEnergy, 0);
        energyRegen = QMath.ClampMin(energyRegen, 0);
    }
}
