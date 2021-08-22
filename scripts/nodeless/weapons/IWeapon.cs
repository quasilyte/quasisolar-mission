using Godot;

public interface IWeapon  {
    bool CanFire(VesselState state, Vector2 cursor);
    void Charge(float delta);
    void Fire(VesselState state, Vector2 cursor);
    void Process(VesselState state, float delta);
    void Ready();
    WeaponDesign GetDesign();
}
