using Godot;

public class CutterWeapon : AbstractWeapon {
    public static WeaponDesign Design = new WeaponDesign {
        name = "Cutter",
        level = 2,
        description = "TODO",
        targeting = "front directions (180°), projectiles",
        sellingPrice = 3900,
        cooldown = 1,
        energyCost = 13,
        range = 330,
        damage = 15,
        damageKind = DamageKind.Electromagnetic,
        projectileSpeed = 280,
        botHintSnipe = 0.2f,
        botHintEffectiveAngle = 1.6f,
    };

    public CutterWeapon(Pilot owner): base(Design, owner) {}

    protected override void CreateProjectile(Vector2 cursor) {
        var dstRotation = cursor.AngleToPoint(_owner.Vessel.Position);
        var projectileRotation = QMath.RotateTo(dstRotation, _owner.Vessel.Rotation, 1.6f);

        var projectile = CutterProjectile.New(_owner, Design);
        projectile.Position = _owner.Vessel.Position;
        projectile.Rotation = projectileRotation;
        _owner.Vessel.GetParent().AddChild(projectile);
    }
}
