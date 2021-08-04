using Godot;

public class HurricaneWeapon : IWeapon {
    public static WeaponDesign Design = new WeaponDesign {
        name = "Hurricane",
        level = 2,
        description = "Fires a swarm of rockets",
        extraDescription = "Every volley consists of 5 rockets",
        targeting = "forward-only, homing missles",
        sellingPrice = 4800,
        technologiesNeeded = {"Hurricane"},
        cooldown = 3.0f,
        range = 425.0f,
        damage = 8.0f,
        damageKind = DamageKind.Thermal,
        projectileSpeed = 250.0f,
        botHintEffectiveAngle = 1,
    };
    public WeaponDesign GetDesign() { return Design; }
    public void Ready() {}

    private float _cooldown;
    private Pilot _owner;

    private int _burst = 0;
    private float _burstCooldown = 0;
    private VesselNode _burstTarget = null;

    public HurricaneWeapon(Pilot owner) {
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
        _burstCooldown -= delta;
        if (_burstCooldown < 0) {
            _burstCooldown = 0;
        }
        if (_burst > 0 && _burstCooldown == 0) {
            var offset = 3 - _burst;
            float angle = 0.15f - (0.05f * (float)_burst);
            
            _burst--;
            _burstCooldown += 0.08f;

            var rocket = Rocket.New(_owner, Design);
            rocket.GlobalTransform = _owner.Vessel.GlobalTransform;
            rocket.Position += rocket.Transform.y * (float)(5 * offset);
            rocket.Rotation += angle;
            rocket.Start(_burstTarget);
            _owner.Vessel.GetParent().AddChild(rocket);
        }
    }

    public void Fire(VesselState state, Vector2 cursor) {
        _cooldown += Design.cooldown;
        var nearest = QMath.NearestEnemy(cursor, _owner);
        if (nearest != null) {
            _burstTarget = nearest.Vessel;
        } else {
            _burstTarget = null;
        }
        _burst = 5;
        var sfx = SoundEffectNode.New(GD.Load<AudioStream>("res://audio/weapon/Hurricane.wav"));
        _owner.Vessel.GetParent().AddChild(sfx);
    }
}
