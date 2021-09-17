using Godot;

public class CrystalCannonWeapon : AbstractWeapon {
    public static WeaponDesign Design = new WeaponDesign {
        name = "Crystal Cannon",
        level = 2,
        description = "TODO",
        targeting = "selected location, projectiles",
        sellingPrice = 4000,
        cooldown = 1.5f,
        energyCost = 10,
        range = 320,
        damage = 15,
        burst = 9,
        damageScoreMultiplier = 3,
        damageKind = DamageKind.Kinetic,
        projectileSpeed = 250,
        botHintSnipe = 0.5f,
    };

    public CrystalCannonWeapon(Pilot owner) : base(Design, owner) { }

    protected override void CreateProjectile(Vector2 cursor) {
        var projectile = CrystalProjectile.New(_owner, cursor);
        projectile.Position = _owner.Vessel.Position;
        projectile.Rotation = (cursor - _owner.Vessel.Position).Normalized().Angle();
        _owner.Vessel.GetParent().AddChild(projectile);
    }
}
