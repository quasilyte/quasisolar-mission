using Godot;

public class ShockwaveCasterWeapon : IWeapon {
    public static WeaponDesign Design = new WeaponDesign{
        name = "Shockwave Caster",
        level = 1,
        description = "TODO",
        extraDescription = "On hit, causes the knockback effect",
        targeting = "any direction, projectiles",
        sellingPrice = 5000,
        cooldown = 0.9f,
        energyCost = 7.0f,
        range = 270.0f,
        damage = 7.0f,
        damageKind = DamageKind.Kinetic,
        projectileSpeed = 450,
        botHintSnipe = 0.55f,
        isSpecial = true,
    };
    public WeaponDesign GetDesign() { return Design; }
    public void Ready() {}
    public void Charge(float delta) {}

    private float _cooldown;
    private Pilot _owner;

    public ShockwaveCasterWeapon(Pilot owner) {
        _owner = owner;
    }

    public bool CanFire(VesselState state, Vector2 cursor) {
        return _cooldown == 0 && state.CanConsumeEnergy(Design.energyCost);
    }

    public void Process(VesselState state, float delta) {
        _cooldown -= delta;
        if (_cooldown < 0) {
            _cooldown = 0;
        }
    }

    public void Fire(VesselState state, Vector2 cursor) {
        _cooldown += Design.cooldown;
        state.ConsumeEnergy(Design.energyCost);
        var projectile = Projectile.New(Design, _owner);
        projectile.Position = _owner.Vessel.Position;
        projectile.Rotation = (cursor - _owner.Vessel.Position).Normalized().Angle();
        _owner.Vessel.GetParent().AddChild(projectile);
    }
}
