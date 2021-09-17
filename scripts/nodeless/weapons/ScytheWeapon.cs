using Godot;

public class ScytheWeapon : IWeapon {
    public static WeaponDesign Design = new WeaponDesign {
        name = "Scythe",
        level = 1,
        description = "Mid-range spread gun",
        extraDescription = "Launches 2 projectiles at a time",
        targeting = "front directions (120°), projectiles",
        sellingPrice = 1000,
        cooldown = 0.75f,
        range = 300,
        damage = 8,
        burst = 2,
        damageKind = DamageKind.Kinetic,
        projectileSpeed = 320,
        botHintSnipe = 0.2f,
    };
    public WeaponDesign GetDesign() { return Design; }
    public void Ready() {}
    public void Charge(float delta) {}

    private float _cooldown;
    private Pilot _owner;

    private static AudioStream _scytheAudioStream = null;

    public ScytheWeapon(Pilot owner) {
        _owner = owner;
    }

    public bool CanFire(VesselState state, Vector2 cursor) {
        if (_cooldown != 0) {
            return false;
        }
        var dstRotation = cursor.AngleToPoint(_owner.Vessel.Position);
        return Mathf.Abs(QMath.RotationDiff(dstRotation, _owner.Vessel.Rotation)) <= 1;
    }

    public void Process(VesselState state, float delta) {
        _cooldown -= delta;
        if (_cooldown < 0) {
            _cooldown = 0;
        }
    }

    public void Fire(VesselState state, Vector2 cursor) {
        _cooldown += Design.cooldown;

        {
            var projectile = Projectile.New(Design, _owner);
            projectile.Position = _owner.Vessel.Position;
            projectile.Rotation = (cursor - _owner.Vessel.Position).Normalized().Angle() - 0.1f;
            _owner.Vessel.GetParent().AddChild(projectile);
        }

        {
            var projectile = Projectile.New(Design, _owner);
            projectile.Position = _owner.Vessel.Position;
            projectile.Rotation = (cursor - _owner.Vessel.Position).Normalized().Angle() + 0.1f;
            _owner.Vessel.GetParent().AddChild(projectile);
        }

        if (_scytheAudioStream == null) {
            _scytheAudioStream = GD.Load<AudioStream>("res://audio/weapon/Scythe.wav");
        }
        var sfx = SoundEffectNode.New(_scytheAudioStream, -6);
        _owner.Vessel.GetParent().AddChild(sfx);
    }
}
