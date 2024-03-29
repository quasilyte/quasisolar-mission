using Godot;

public class NeedleGunWeapon : IWeapon {
    public static WeaponDesign TurretDesign = new WeaponDesign{
        damage = 20,
        range = 2000,
        projectileSpeed = 500,
        damageKind = DamageKind.Kinetic,
        ignoresAsteroids = true,
        maskScale = 2.5f,
    };

    public static WeaponDesign Design = new WeaponDesign{
        name = "Needle Gun",
        level = 1,
        researchRequired = false,
        description = "A rail gun that shots projectiles with high kinetic power",
        special = "penetrating projectiles",
        targeting = "any direction, projectiles",
        sellingPrice = 900,
        cooldown = 1.8f,
        range = 270.0f,
        damage = 11.0f,
        damageKind = DamageKind.Kinetic,
        energyCost = 10,
        projectileSpeed = 280.0f,
        botHintSnipe = 0.25f,
    };
    public WeaponDesign GetDesign() { return Design; }
    public void Ready() {}
    public void Charge(float delta) {}

    private float _cooldown;
    private Pilot _owner;

    public NeedleGunWeapon(Pilot owner) {
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
