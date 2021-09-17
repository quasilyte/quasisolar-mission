using Godot;

public class GreatScytheWeapon : IWeapon {
    public static WeaponDesign Design = new WeaponDesign {
        name = "Great Scythe",
        level = 2,
        description = "Mid-range spread gun",
        extraDescription = "Launches 4 projectiles at a time",
        targeting = "front directions (120°), projectiles",
        sellingPrice = 2900,
        cooldown = 1.1f,
        range = 300,
        damage = 8,
        burst = 6,
        damageScoreMultiplier = 3,
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

    public GreatScytheWeapon(Pilot owner) {
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
            projectile.Rotation = (cursor - _owner.Vessel.Position).Normalized().Angle() - 0.25f;
            _owner.Vessel.GetParent().AddChild(projectile);
        }

        {
            var projectile = Projectile.New(Design, _owner);
            projectile.Position = _owner.Vessel.Position;
            projectile.Rotation = (cursor - _owner.Vessel.Position).Normalized().Angle() - 0.15f;
            _owner.Vessel.GetParent().AddChild(projectile);
        }

        {
            var projectile = Projectile.New(Design, _owner);
            projectile.Position = _owner.Vessel.Position;
            projectile.Rotation = (cursor - _owner.Vessel.Position).Normalized().Angle() - 0.05f;
            _owner.Vessel.GetParent().AddChild(projectile);
        }

        {
            var projectile = Projectile.New(Design, _owner);
            projectile.Position = _owner.Vessel.Position;
            projectile.Rotation = (cursor - _owner.Vessel.Position).Normalized().Angle() + 0.05f;
            _owner.Vessel.GetParent().AddChild(projectile);
        }

        {
            var projectile = Projectile.New(Design, _owner);
            projectile.Position = _owner.Vessel.Position;
            projectile.Rotation = (cursor - _owner.Vessel.Position).Normalized().Angle() + 0.15f;
            _owner.Vessel.GetParent().AddChild(projectile);
        }

        {
            var projectile = Projectile.New(Design, _owner);
            projectile.Position = _owner.Vessel.Position;
            projectile.Rotation = (cursor - _owner.Vessel.Position).Normalized().Angle() + 0.25f;
            _owner.Vessel.GetParent().AddChild(projectile);
        }

        if (_scytheAudioStream == null) {
            _scytheAudioStream = GD.Load<AudioStream>("res://audio/weapon/Scythe.wav");
        }
        var sfx = SoundEffectNode.New(_scytheAudioStream, -4);
        _owner.Vessel.GetParent().AddChild(sfx);
    }
}
