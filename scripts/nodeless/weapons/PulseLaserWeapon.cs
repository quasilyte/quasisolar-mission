using Godot;

public class PulseLaserWeapon : IWeapon {
    public static WeaponDesign Design = new WeaponDesign{
        name = "Pulse Laser",
        level = 1,
        description = "A fierce weapon for a close-ranged combat",
        targeting = "any direction, projectiles",
        sellingPrice = 2400,
        technologiesNeeded = {"Pulse Laser"},
        cooldown = 0.15f,
        energyCost = 5.0f,
        range = 200.0f,
        damage = 8.0f,
        damageKind = DamageKind.Energy,
        projectileSpeed = 300.0f,
        botHintSnipe = 0.15f,
    };
    public WeaponDesign GetDesign() { return Design; }
    public void Ready() {}
    
    private float _cooldown;
    private Pilot _owner;

    public PulseLaserWeapon(Pilot owner) {
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
        var target = QMath.RandomizedLocation(cursor, 8);
        projectile.Rotation = (target - _owner.Vessel.Position).Normalized().Angle();
        _owner.Vessel.GetParent().AddChild(projectile);
    }
}
