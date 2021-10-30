using Godot;

public interface IShield  {
    bool CanActivate(VesselState state);
    bool IsActive();
    void Activate(VesselState state);
    float ActivationCost(VesselState state);
    void Deactivate();
    void Process(VesselState state, float delta);
    float ReduceDamage(float damage, DamageKind kind);
    ShieldDesign GetDesign();
}
