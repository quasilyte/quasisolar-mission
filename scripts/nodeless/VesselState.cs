using Godot;

// VesselState holds the current vessel status.
public class VesselState {
    public int vesselLevel;

    public VesselStats stats;

    public float rotationSpeed;

    public float initialHp;
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

    public bool insidePurpleNebula = false;
    public bool insideBlueNebula = false;
    public bool insideStarHazard = false;

    public int debris;

    public VesselDesign.Size vesselSize;

    public VesselState(VesselStats vesselStats, VesselDesign design, EnergySource battery) {
        vesselLevel = design.level;

        rotationSpeed = design.rotationSpeed;

        stats = vesselStats;

        hp = stats.maxHp;
        energy = stats.maxEnergy;
        backupEnergy = stats.maxBackupEnergy;

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
