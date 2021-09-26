using Godot;

public class FlareWeapon : IWeapon {
    public static WeaponDesign Design = new WeaponDesign {
        name = "Flare",
        level = 2,
        description = "TODO",
        targeting = "forward-only, homing missles",
        sellingPrice = 3600,
        cooldown = 2.8f,
        range = 450.0f,
        damage = 16.0f,
        burst = 7,
        energyCost = 15,
        damageKind = DamageKind.Thermal,
        projectileSpeed = 220.0f,
        botHintEffectiveAngle = 1.3f,
    };
    public WeaponDesign GetDesign() { return Design; }
    public void Ready() {}
    public void Charge(float delta) {}

    private float _cooldown;
    private Pilot _owner;

    public FlareWeapon(Pilot owner) {
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

        var offset = 7.0;
        var angle = 0.35f;
        for (int i = -3; i < 4; i++) {
            var rocket = FlareProjectileNode.New(_owner);
            rocket.GlobalTransform = _owner.Vessel.GlobalTransform;
            rocket.Position += rocket.Transform.y * (float)(offset * i);
            rocket.Rotation += angle * (float)i;
            _owner.Vessel.GetParent().AddChild(rocket);
        }

        var sfx = SoundEffectNode.New(GD.Load<AudioStream>("res://audio/weapon/Flare.wav"));
        _owner.Vessel.GetParent().AddChild(sfx);
    }
}
