using Godot;

public class HellfireWeapon : IWeapon {
    public static WeaponDesign Design = new WeaponDesign {
        name = "Hellfire",
        level = 2,
        description = "TODO",
        targeting = "forward-only, projectiles",
        sellingPrice = 3000,
        cooldown = 0.6f,
        energyCost = 10.0f,
        range = 200.0f,
        damage = 3.0f,
        damageKind = DamageKind.Thermal,
        projectileSpeed = 400.0f,
        botHintEffectiveAngle = 0.6f,
    };
    public WeaponDesign GetDesign() { return Design; }
    public void Ready() {}

    private float _cooldown;
    private Pilot _owner;

    private int _burst = 0;
    private float _burstCooldown = 0;

    public HellfireWeapon(Pilot owner) {
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
        _burstCooldown -= delta;
        if (_burstCooldown < 0) {
            _burstCooldown = 0;
        }
        if (_burst > 0 && _burstCooldown == 0) {
            _burst--;
            _burstCooldown += 0.05f;

            for (int i = 0; i < 3; i++) {
                var projectile = Projectile.New(Design, _owner);
                projectile.GlobalTransform = _owner.Vessel.GlobalTransform;
                projectile.Rotation += QRandom.FloatRange(-0.15f, 0.15f);
                _owner.Vessel.GetParent().AddChild(projectile);
            }
        }
    }

    public void Fire(VesselState state, Vector2 cursor) {
        _cooldown += Design.cooldown;
        state.ConsumeEnergy(Design.energyCost);
        _burst = 7;
        var sfx = SoundEffectNode.New(GD.Load<AudioStream>("res://audio/weapon/Hellfire.wav"));
        _owner.Vessel.GetParent().AddChild(sfx);
    }
}
