public class EmptyShield : IShield {
    public static ShieldDesign Design = new ShieldDesign{
        name = "Empty",
    };
    public ShieldDesign GetDesign() { return Design; }

    public bool IsActive() { return false; }
    public bool CanActivate(VesselState state) { return false; }
    public void Activate(VesselState state) {}
    public void Deactivate() {}
    public void Process(VesselState state, float delta) {}
    public float ReduceDamage(float damage, DamageKind kind) { return damage; }
    public float ActivationCost(VesselState state) { return 0; }
}