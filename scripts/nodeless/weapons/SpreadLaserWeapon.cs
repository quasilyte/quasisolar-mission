using Godot;

public class SpreadLaserWeapon : AbstractWeapon {
    public static WeaponDesign Design = new WeaponDesign {
        name = "Spread Laser",
        level = 1,
        description = "TODO",
        targeting = "forward-only, projectiles",
        sellingPrice = 800,
        cooldown = 0.15f,
        energyCost = 3,
        range = 250.0f,
        damage = 6.0f,
        damageKind = DamageKind.Electromagnetic,
        projectileSpeed = 450.0f,
        botHintEffectiveAngle = 0.6f,
    };

    public SpreadLaserWeapon(Pilot owner) : base(Design, owner) {}

    protected override void CreateProjectile(Vector2 cursor) {
        var projectile = Projectile.New(Design, _owner);
        projectile.GlobalTransform = _owner.Vessel.GlobalTransform;
        projectile.Rotation += QRandom.FloatRange(-0.25f, 0.25f);
        _owner.Vessel.GetParent().AddChild(projectile);

        var sfx = SoundEffectNode.New(GD.Load<AudioStream>("res://audio/weapon/Spread_Laser.wav"), -5);
        _owner.Vessel.GetParent().AddChild(sfx);
    }
}
