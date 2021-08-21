using Godot;

public class SpreadGunWeapon : IWeapon {
    public static WeaponDesign Design = new WeaponDesign {
        name = "Spread Gun",
        level = 1,
        researchRequired = false,
        description = "TODO",
        targeting = "forward-only, projectiles",
        sellingPrice = 350,
        cooldown = 0.15f,
        range = 200.0f,
        damage = 3.0f,
        damageKind = DamageKind.Kinetic,
        projectileSpeed = 400.0f,
        botHintEffectiveAngle = 0.8f,
    };
    public WeaponDesign GetDesign() { return Design; }
    public void Ready() {}

    private float _cooldown;
    private Pilot _owner;

    public SpreadGunWeapon(Pilot owner) {
        _owner = owner;
    }

    public bool CanFire(VesselState state, Vector2 cursor) {
        return _cooldown == 0;
    }

    public void Process(VesselState state, float delta) {
        _cooldown -= delta;
        if (_cooldown < 0) {
            _cooldown = 0;
        }
    }

    public void Fire(VesselState state, Vector2 cursor) {
        _cooldown += Design.cooldown;
        
        var projectile = Projectile.New(Design, _owner);
        projectile.GlobalTransform = _owner.Vessel.GlobalTransform;
        projectile.Rotation += QRandom.FloatRange(-0.3f, 0.3f);
        _owner.Vessel.GetParent().AddChild(projectile);

        var sfx = SoundEffectNode.New(GD.Load<AudioStream>("res://audio/weapon/Spread_Gun.wav"), -5);
        _owner.Vessel.GetParent().AddChild(sfx);
    }
}
