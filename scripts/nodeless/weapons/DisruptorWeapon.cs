using Godot;

public class DisruptorWeapon : IWeapon {
    public static WeaponDesign Design = new WeaponDesign{
        name = "Disruptor",
        level = 1,
        description = "Launches an electric surges that mess with the enemy reactor.",
        extraDescription = "On hit, temporarily disables energy regen",
        targeting = "any direction, homing projectiles",
        sellingPrice = 3400,
        technologiesNeeded = {"Disruptor"},
        cooldown = 2,
        energyCost = 9.0f,
        range = 300.0f,
        damage = 12,
        damageKind = DamageKind.Energy,
        projectileSpeed = 280.0f,
        botHintSnipe = 0.45f,
        isSpecial = true,
    };
    public WeaponDesign GetDesign() { return Design; }
    public void Ready() {}
    
    private float _cooldown;
    private Pilot _owner;

    public DisruptorWeapon(Pilot owner) {
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
        var nearest = QMath.NearestEnemy(cursor, _owner);
        Node2D target = nearest == null ? null : nearest.Vessel;

        _cooldown += Design.cooldown;
        state.ConsumeEnergy(Design.energyCost);
        var projectile = DisruptorProjectileNode.New(_owner, target);
        projectile.Position = _owner.Vessel.Position;
        projectile.Rotation = (cursor - _owner.Vessel.Position).Normalized().Angle();
        _owner.Vessel.GetParent().AddChild(projectile);
    }
}
