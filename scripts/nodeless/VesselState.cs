using Godot;

// VesselState holds the current vessel status.
public class VesselState {
    public int vesselLevel;

    public float maxEnergy;
    public float maxBackupEnergy;
    public float energyRegen;

    public float maxHp;
    public float maxSpeed;
    public float acceleration;
    public float rotationSpeed;

    public float hp;
    public float energy;
    public float backupEnergy;

    public float rotationCrippledTime = 0;
    public float reactorDisabledTime = 0;
    
    public float phasingTime = 0;

    public float shieldCooldownRate = 1;
    public float shieldDurationRate = 1;

    public float speedPenalty = 0;
    public Vector2 velocity = Vector2.Zero;

    public Node2D draggedBy = null;
    public float dragTime = 0;

    public int debris;

    public VesselDesign.Size vesselSize;

    public VesselState(VesselDesign design, EnergySource battery) {
        vesselLevel = design.level;

        maxHp = design.maxHp;
        maxSpeed = design.maxSpeed;
        acceleration = design.acceleration;
        rotationSpeed = design.rotationSpeed;

        maxEnergy = 15 + battery.maxEnergy;
        maxBackupEnergy = battery.maxBackupEnergy;
        energyRegen = 0.5f + battery.energyRegen;

        hp = design.maxHp;
        energy = maxEnergy;
        backupEnergy = maxBackupEnergy;

        debris = design.debris;

        vesselSize = design.size;
    }

    public bool CanConsumeEnergy(float amount) {
        return energy >= amount || backupEnergy >= amount;
    }

    public bool ConsumeEnergy(float amount) {
        if (energy >= amount) {
            energy -= amount;
            return true;
        }
        if (backupEnergy >= amount) {
            backupEnergy -= amount;
            return true;
        }
        return false;
    }
}
