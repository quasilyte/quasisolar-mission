using Godot;

public interface IProjectile {
    Pilot FiredBy();
    WeaponDesign GetWeaponDesign();
    Node2D GetProjectileNode();
    void OnImpact();
}
