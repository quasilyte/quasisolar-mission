using Godot;

public class PhotonBurstCannonWeapon : IWeapon {
    public static WeaponDesign Design = new WeaponDesign{
        name = "Photon Burst Cannon",
        level = 1,
        description = "Low-range burst weapon",
        extraDescription = "Fires 3 projectiles with a slight delay between the shots",
        targeting = "any direction, projectiles",
        sellingPrice = 1000,
        cooldown = 0.8f,
        energyCost = 6f,
        range = 200f,
        damage = 5f,
        damageKind = DamageKind.Energy,
        projectileSpeed = 400.0f,
        botHintSnipe = 0.3f,
    };
    public WeaponDesign GetDesign() { return Design; }
    public void Ready() {}

    private float _cooldown;
    private Pilot _owner;

    private int _burst = 0;
    private float _burstCooldown = 0;
    private Vector2 _burstTarget;

    public PhotonBurstCannonWeapon(Pilot owner) {
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
            var projectile = Projectile.New(Design, _owner);
            projectile.Position = _owner.Vessel.Position;
            projectile.Rotation = (_burstTarget - _owner.Vessel.Position).Normalized().Angle();
            _owner.Vessel.GetParent().AddChild(projectile);

            var sfx = SoundEffectNode.New(GD.Load<AudioStream>("res://audio/weapon/Photon_Burst_Cannon.wav"), -4);
            _owner.Vessel.GetParent().AddChild(sfx);
        }
    }

    public void Fire(VesselState state, Vector2 cursor) {
        _cooldown += Design.cooldown;
        state.ConsumeEnergy(Design.energyCost);
        _burst = 3;
        _burstTarget = cursor;
    }
}
