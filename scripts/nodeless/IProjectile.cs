public interface IProjectile {
    Pilot FiredBy();
    WeaponDesign GetWeaponDesign();
    void OnImpact();
}
