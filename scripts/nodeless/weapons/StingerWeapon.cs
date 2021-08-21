using Godot;

public class StingerWeapon : IWeapon {
    public static WeaponDesign Design = new WeaponDesign{
        name = "Stinger",
        level = 2,
        description = "A weapon that is designed to slow down the target",
        special = "reduces target mobility for a short period of time",
        targeting = "any direction, projectiles",
        sellingPrice = 3500,
        cooldown = 0.6f,
        energyCost = 10.0f,
        range = 250.0f,
        damage = 6.0f,
        damageKind = DamageKind.Energy,
        projectileSpeed = 380.0f,
        botHintSnipe = 0.3f,
    };
    public WeaponDesign GetDesign() { return Design; }
    public void Ready() {}

    private float _cooldown;
    private Pilot _owner;

    public StingerWeapon(Pilot owner) {
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
