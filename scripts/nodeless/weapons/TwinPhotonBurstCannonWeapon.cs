using Godot;

public class TwinPhotonBurstCannonWeapon : IWeapon {
    public static WeaponDesign Design = new WeaponDesign {
        name = "Twin Photon Burst Cannon",
        level = 2,
        description = "An improved version of the Photon Burst Cannon",
        extraDescription = "Fires two 2-projectile bursts with a slight delay between them",
        targeting = "any direction, projectiles",
        sellingPrice = 2100,
        cooldown = 0.8f,
        energyCost = 8f,
        range = 200f,
        damage = 5f,
        burst = 4,
        damageKind = DamageKind.Electromagnetic,
        projectileSpeed = 400.0f,
        botHintSnipe = 0.3f,
    };
    public WeaponDesign GetDesign() { return Design; }
    public void Ready() {}
    public void Charge(float delta) {}

    private float _cooldown;
    private Pilot _owner;

    private int _burst = 0;
    private float _burstCooldown = 0;
    private Vector2 _burstTarget;

    public TwinPhotonBurstCannonWeapon(Pilot owner) {
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
            _burstCooldown += 0.1f;

            var rotation = (_burstTarget - _owner.Vessel.Position).Normalized().Angle();

            var transform = _owner.Vessel.GlobalTransform;
            transform.Rotation = rotation;

            {
                var projectile = Projectile.New(Design, _owner);
                projectile.GlobalTransform = transform;
                projectile.Position += projectile.Transform.y * 8;
                projectile.Rotation = rotation;
                _owner.Vessel.GetParent().AddChild(projectile);
            }
            {
                var projectile = Projectile.New(Design, _owner);
                projectile.GlobalTransform = transform;
                projectile.Position -= projectile.Transform.y * 8;
                projectile.Rotation = rotation;
                _owner.Vessel.GetParent().AddChild(projectile);
            }

            var sfx = SoundEffectNode.New(GD.Load<AudioStream>("res://audio/weapon/Photon_Burst_Cannon.wav"), -4);
            _owner.Vessel.GetParent().AddChild(sfx);
        }
    }

    public void Fire(VesselState state, Vector2 cursor) {
        _cooldown += Design.cooldown;
        state.ConsumeEnergy(Design.energyCost);
        _burst = 2;
        _burstTarget = cursor;
    }
}
