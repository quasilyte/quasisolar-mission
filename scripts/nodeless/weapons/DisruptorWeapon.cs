using Godot;

public class DisruptorWeapon : AbstractWeapon {
    public static WeaponDesign Design = new WeaponDesign {
        name = "Disruptor",
        level = 1,
        description = "Launches an electric surge that messes with the enemy reactor",
        targeting = "any direction, homing projectiles",
        special = "disables energy regen on hit",
        sellingPrice = 3400,
        cooldown = 2,
        energyCost = 9.0f,
        range = 300.0f,
        damage = 12,
        damageKind = DamageKind.Electromagnetic,
        projectileSpeed = 280.0f,
        botHintSnipe = 0.45f,
        isSpecial = true,
    };

    public DisruptorWeapon(Pilot owner) : base(Design, owner) { }

    protected override void CreateProjectile(Vector2 cursor) {
        var nearest = QMath.NearestEnemy(cursor, _owner);
        Node2D target = nearest == null ? null : nearest.Vessel;

        var projectile = DisruptorProjectileNode.New(_owner, target);
        projectile.Position = _owner.Vessel.Position;
        projectile.Rotation = (cursor - _owner.Vessel.Position).Normalized().Angle();
        _owner.Vessel.GetParent().AddChild(projectile);
    }
}
