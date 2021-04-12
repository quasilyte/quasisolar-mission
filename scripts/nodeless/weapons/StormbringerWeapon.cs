using Godot;

public class StormbringerWeapon : IWeapon {
    public static WeaponDesign Design = new WeaponDesign {
        name = "Stormbringer",
        level = 3,
        description = "TODO",
        targeting = "forward-only, homing projectiles",
        sellingPrice = 7000,
        technologiesNeeded = {"Stormbringer"},
        cooldown = 1.2f,
        energyCost = 12,
        range = 450.0f,
        damage = 16.0f,
        damageKind = DamageKind.Energy,
        projectileSpeed = 400.0f,
        botHintEffectiveAngle = 2,
        ignoresAsteroids = true,
    };
    public WeaponDesign GetDesign() { return Design; }
    public void Ready() {}

    private float _cooldown;
    private Pilot _owner;

    public StormbringerWeapon(Pilot owner) {
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

        var nearest = QMath.NearestEnemy(cursor, _owner);
        Node2D target = nearest == null ? null : nearest.Vessel;

        var lightningLine = LightningLine.New();
        var projectile = LightningProjectile.New(lightningLine, _owner, target);
        projectile.Position = _owner.Vessel.Position;
        projectile.Rotation = _owner.Vessel.Rotation + QRandom.FloatRange(-0.4f, 0.4f);
        _owner.Vessel.GetParent().AddChild(projectile);
        _owner.Vessel.GetParent().AddChild(lightningLine);
    
        var sfx = SoundEffectNode.New(GD.Load<AudioStream>("res://audio/weapon/Storm_Bringer.wav"), -6);
        _owner.Vessel.GetParent().AddChild(sfx);
    }
}
