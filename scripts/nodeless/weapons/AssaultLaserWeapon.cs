using Godot;

public class AssaultLaserWeapon : IWeapon {
    public static WeaponDesign Design = new WeaponDesign{
        name = "Assault Laser",
        level = 2,
        description = "TODO",
        targeting = "any direction, projectiles",
        sellingPrice = 4000,
        cooldown = 0.65f,
        energyCost = 9.0f,
        range = 260,
        damage = 7.0f,
        damageKind = DamageKind.Energy,
        projectileSpeed = 340.0f,
        botHintSnipe = 0.25f,
    };
    public WeaponDesign GetDesign() { return Design; }
    public void Ready() {}
    public void Charge(float delta) {}
    
    private float _cooldown;
    private Pilot _owner;

    public AssaultLaserWeapon(Pilot owner) {
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

        var target = QMath.RandomizedLocation(cursor, 10);
        var rotation = (target - _owner.Vessel.Position).Normalized().Angle();
        var transform = _owner.Vessel.GlobalTransform;
        transform.Rotation = rotation;

        {
            var projectile = Projectile.New(Design, _owner);
            projectile.GlobalTransform = transform;
            projectile.Position += projectile.Transform.y * 4;
            projectile.Rotation = rotation;
            _owner.Vessel.GetParent().AddChild(projectile);
        }
        {
            var projectile = Projectile.New(Design, _owner);
            projectile.GlobalTransform = transform;
            projectile.Position += projectile.Transform.x * 8;
            projectile.Rotation = rotation;
            _owner.Vessel.GetParent().AddChild(projectile);
        }
        {
            var projectile = Projectile.New(Design, _owner);
            projectile.GlobalTransform = transform;
            projectile.Position -= projectile.Transform.y * 4;
            projectile.Rotation = rotation;
            _owner.Vessel.GetParent().AddChild(projectile);
        }

        var sfx = SoundEffectNode.New(GD.Load<AudioStream>("res://audio/weapon/Assault_Laser.wav"), -5);
        _owner.Vessel.GetParent().AddChild(sfx);
    }
}
