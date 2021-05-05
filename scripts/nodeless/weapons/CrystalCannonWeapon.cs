using Godot;

public class CrystalCannonWeapon : IWeapon {
    public static WeaponDesign Design = new WeaponDesign {
        name = "Crystal Cannon",
        level = 2,
        description = "TODO",
        targeting = "selected location, projectiles",
        sellingPrice = 4000,
        technologiesNeeded = {"Crystal Cannon"},
        cooldown = 1.5f,
        energyCost = 10,
        range = 320,
        damage = 15,
        damageKind = DamageKind.Kinetic,
        projectileSpeed = 250,
        botHintSnipe = 0.5f,
    };
    public WeaponDesign GetDesign() { return Design; }
    public void Ready() {}

    private float _cooldown;
    private Pilot _owner;

    public CrystalCannonWeapon(Pilot owner) {
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

        var projectile = CrystalProjectile.New(_owner, cursor);
        projectile.Position = _owner.Vessel.Position;
        projectile.Rotation = (cursor - _owner.Vessel.Position).Normalized().Angle();
        _owner.Vessel.GetParent().AddChild(projectile);
    }
}
