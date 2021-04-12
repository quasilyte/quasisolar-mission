using Godot;

public interface IArtifact {
    // Apply is called on every _Process() tick of the owner.
    // It's a chance for articats that affect the current state
    // to apply their effects (if any).
    void Apply(VesselState state, float delta);

    // Upgrade is used to apply the passive artifact
    // effects on vessel stats.
    void Upgrade(VesselState state);
}
