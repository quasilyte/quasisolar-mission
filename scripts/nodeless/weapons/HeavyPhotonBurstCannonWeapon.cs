using Godot;

public class HeavyPhotonBurstCannonWeapon : IWeapon {
    public static WeaponDesign Design = new WeaponDesign{
        name = "Heavy Photon Burst Cannon",
        level = 2,
        description = "Mid-range burst cannon",
        extraDescription = "Fires 3 projectiles with delay between the shots",
        targeting = "any direction, projectiles",
        sellingPrice = 3500,
        cooldown = 1.8f,
        energyCost = 10f,
        range = 250f,
        damage = 12f,
        burst = 3,
        damageKind = DamageKind.Electromagnetic,
        projectileSpeed = 470.0f,
        botHintSnipe = 0.45f,
    };
    public WeaponDesign GetDesign() { return Design; }
    public void Ready() {}
    public void Charge(float delta) {}

    private float _cooldown;
    private Pilot _owner;

    private int _burst = 0;
    private float _burstCooldown = 0;
    private float _burstDirection;

    public HeavyPhotonBurstCannonWeapon(Pilot owner) {
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
            _burstCooldown += 0.3f;
            var projectile = Projectile.New(Design, _owner);
            projectile.Position = _owner.Vessel.Position;
            projectile.Rotation = _burstDirection;
            _owner.Vessel.GetParent().AddChild(projectile);

            var sfx = SoundEffectNode.New(GD.Load<AudioStream>("res://audio/weapon/Heavy_Photon_Burst_Cannon.wav"));
            _owner.Vessel.GetParent().AddChild(sfx);
        }
    }

    public void Fire(VesselState state, Vector2 cursor) {
        _cooldown += Design.cooldown;
        state.ConsumeEnergy(Design.energyCost);
        _burst = Design.burst;
        if (state.hasPhotonium) {
            _burst++;
        }
        _burstDirection = (cursor - _owner.Vessel.Position).Normalized().Angle();
    }
}
