using Godot;

public class FlakCannonWeapon : IWeapon {
    public static WeaponDesign Design = new WeaponDesign{
        name = "Flak Cannon",
        level = 1,
        description = "TODO",
        targeting = "any direction, projectiles",
        sellingPrice = 5700,
        technologiesNeeded = {"Flak Cannon"},
        cooldown = 0.2f,
        range = 460.0f,
        damage = 8.0f,
        damageKind = DamageKind.Kinetic,
        projectileSpeed = 320.0f,
        botHintSnipe = 0.3f,
    };
    public WeaponDesign GetDesign() { return Design; }
    public void Ready() {}

    private float _cooldown;
    private Pilot _owner;

    private float _extraCooldown;

    public FlakCannonWeapon(Pilot owner) {
        _owner = owner;
    }

    public bool CanFire(VesselState state, Vector2 cursor) {
        return _cooldown == 0;
    }

    public void Process(VesselState state, float delta) {
        _cooldown -= delta;
        if (_cooldown < 0) {
            _cooldown = 0;
        }
        _extraCooldown = QMath.ClampMin(_extraCooldown - (delta / 6), 0);
    }

    public void Fire(VesselState state, Vector2 cursor) {
        _cooldown += Design.cooldown + _extraCooldown;
        _extraCooldown += 0.1f;
        var projectile = Projectile.New(Design, _owner);
        projectile.Position = _owner.Vessel.Position;
        projectile.Rotation = (cursor - _owner.Vessel.Position).Normalized().Angle();
        projectile.Rotation += QRandom.FloatRange(-0.3f, 0.3f);
        _owner.Vessel.GetParent().AddChild(projectile);
    }
}
