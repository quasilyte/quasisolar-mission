using Godot;

public class AfterburnerWeapon : IWeapon {
    public static WeaponDesign Design = new WeaponDesign {
        name = "Afterburner",
        level = 2,
        description = "TODO",
        targeting = "n/a",
        sellingPrice = 5000,
        cooldown = 0.7f,
        energyCost = 10.0f,
        duration = 6,
        damage = 4,
        burst = 4,
        damageScoreMultiplier = 2,
        damageKind = DamageKind.Thermal,
        isSpecial = true,
    };
    public WeaponDesign GetDesign() { return Design; }
    public void Ready() {}
    public void Charge(float delta) {}

    private float _cooldown;
    private Pilot _owner;

    private int _burst = 0;
    private float _burstCooldown = 0;

    public AfterburnerWeapon(Pilot owner) {
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
            if (state.speedBonus < 20) {
                state.speedBonus += 5;
            }
            _owner.Vessel.State.velocity += _owner.Vessel.Transform.x * (10 + (_burst * 5));

            _burst--;
            _burstCooldown += 0.1f;
        
            var sfx = SoundEffectNode.New(GD.Load<AudioStream>("res://audio/weapon/Afterburner.wav"), -4);
            _owner.Vessel.GetParent().AddChild(sfx);

            var flame = AfterburnerFlameNode.New(_owner);
            flame.Position = _owner.Vessel.Position;
            flame.Position -= _owner.Vessel.Transform.x * 15;
            _owner.Vessel.GetParent().AddChild(flame);
        }
    }

    public void Fire(VesselState state, Vector2 cursor) {
        _cooldown += Design.cooldown;
        state.ConsumeEnergy(Design.energyCost);

        _burst = Design.burst;
    }
}
