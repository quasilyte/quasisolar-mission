using Godot;

public class EmptyWeapon : IWeapon {
    public static WeaponDesign Design = new WeaponDesign{
        name = "Empty",
    };
    public WeaponDesign GetDesign() { return Design; }
    public void Ready() {}
    public void Charge(float delta) {}

    public bool CanFire(VesselState state, Vector2 cursor) {
        return false;
    }

    public void Process(VesselState state, float delta) {}
    public void Fire(VesselState state, Vector2 cursor) {}
}
