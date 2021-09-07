using Godot;

public class RocketLauncherWeapon : IWeapon {
    public static WeaponDesign TurretDesign = new WeaponDesign{
        damage = 16,
        range = 1800,
        projectileSpeed = 380,
        damageKind = DamageKind.Thermal,
        ignoresAsteroids = true,
        maskScale = 1.5f,
    };

    public static WeaponDesign Design = new WeaponDesign {
        name = "Rocket Launcher",
        level = 1,
        description = "An old-fashioned weapon for a long-ranged combat",
        extraDescription = "Launches 3 rockets at a time",
        targeting = "forward-only, homing missles",
        sellingPrice = 2650,
        cooldown = 1.4f,
        range = 450.0f,
        damage = 10.0f,
        damageKind = DamageKind.Thermal,
        projectileSpeed = 200.0f,
        botHintEffectiveAngle = 1,
    };
    public WeaponDesign GetDesign() { return Design; }
    public void Ready() {}
    public void Charge(float delta) {}

    private float _cooldown;
    private Pilot _owner;

    public RocketLauncherWeapon(Pilot owner) {
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
        var offset = 10.0;
        var angle = 0.25f;
        for (int i = -1; i < 2; i++) {
            var rocket = Rocket.New(_owner, Design);
            rocket.GlobalTransform = _owner.Vessel.GlobalTransform;
            rocket.Position += rocket.Transform.y * (float)(offset * i);
            rocket.Rotation += angle * (float)i;
            var nearest = QMath.NearestEnemy(cursor, _owner);
            if (nearest != null) {
                rocket.Start(nearest.Vessel);
            } else {
                rocket.Start(null);
            }
            _owner.Vessel.GetParent().AddChild(rocket);
        }

        var sfx = SoundEffectNode.New(GD.Load<AudioStream>("res://audio/rocket.wav"), -8);
        _owner.Vessel.GetParent().AddChild(sfx);
    }
}
