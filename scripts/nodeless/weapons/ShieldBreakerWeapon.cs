using Godot;

public class ShieldBreakerWeapon : IWeapon {
    public static WeaponDesign Design = new WeaponDesign {
        name = "Shield Breaker",
        level = 2,
        description = "TODO",
        extraDescription = "Launches 2 missiles at a time",
        special = "deactivates target shield on impact",
        special2 = "missiles can't be targeted by the point-defense",
        targeting = "forward-only, homing missles",
        sellingPrice = 4600,
        technologiesNeeded = {"Shield Breaker"},
        cooldown = 2.8f,
        energyCost = 10,
        range = 475.0f,
        damage = 16.0f,
        damageKind = DamageKind.Energy,
        projectileSpeed = 225.0f,
        botHintEffectiveAngle = 1.2f,
    };
    public WeaponDesign GetDesign() { return Design; }
    public void Ready() {}

    private float _cooldown;
    private Pilot _owner;

    public ShieldBreakerWeapon(Pilot owner) {
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

        var offset = 10.0;
        var angle = 0.40f;
        for (int j = 0; j < 2; j++) {
            var i = j == 0 ? -1 : 1;
            var rocket = Rocket.New(_owner, Design);
            rocket.GlobalTransform = _owner.Vessel.GlobalTransform;
            rocket.Position += rocket.Transform.y * (float)(offset * i);
            rocket.Rotation += angle * (float)i;
            var nearest = QMath.NearestEnemy(cursor, _owner);
            if (nearest != null && !nearest.Vessel.artifacts.Exists(x => x is CloakingDeviceArtifact)) {
                rocket.Start(nearest.Vessel);
            } else {
                rocket.Start(null);
            }
            _owner.Vessel.GetParent().AddChild(rocket);
        }

        var sfx = SoundEffectNode.New(GD.Load<AudioStream>("res://audio/weapon/Shield_Breaker.wav"));
        _owner.Vessel.GetParent().AddChild(sfx);
    }
}
