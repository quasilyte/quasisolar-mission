using Godot;

public class DiskThrowerWeapon : AbstractWeapon {
    public static WeaponDesign Design = new WeaponDesign {
        name = "Disk Thrower",
        level = 3,
        description = "Sends spinning bladed disks of death into the space",
        extraDescription = "Disks do not disappear if missed and serve as space mines",
        targeting = "selected location, projectiles",
        sellingPrice = 6800,
        cooldown = 1.10f,
        energyCost = 6.0f,
        range = 375.0f,
        damage = 17.0f,
        damageKind = DamageKind.Kinetic,
        projectileSpeed = 180.0f,
        botHintSnipe = 0.8f,
        botHintScatter = 0,
    };

    public DiskThrowerWeapon(Pilot owner) : base(Design, owner) { }

    protected override void CreateProjectile(Vector2 cursor) {
        var reachable = _owner.Vessel.Position.MoveToward(cursor, Design.range);
        var target = QMath.RandomizedLocation(reachable, 24);
        var projectile = DiskProjectile.New(_owner, target);
        projectile.Position = _owner.Vessel.Position;
        projectile.Rotation = (target - _owner.Vessel.Position).Normalized().Angle();
        _owner.Vessel.GetParent().AddChild(projectile);
    }
}
