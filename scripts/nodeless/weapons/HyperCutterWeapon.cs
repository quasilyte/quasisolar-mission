using Godot;

public class HyperCutterWeapon : IWeapon {
    public static WeaponDesign Design = new WeaponDesign {
        name = "Hyper Cutter",
        level = 2,
        description = "TODO",
        targeting = "front directions (180°), projectiles",
        sellingPrice = 4500,
        technologiesNeeded = {"Hyper Cutter"},
        cooldown = 1.25f,
        energyCost = 18,
        range = 360,
        damage = 19,
        damageKind = DamageKind.Energy,
        projectileSpeed = 280,
        botHintSnipe = 0.3f,
        isSpecial = true,
    };
    public WeaponDesign GetDesign() { return Design; }
    public void Ready() {}

    private float _cooldown;
    private Pilot _owner;

    public HyperCutterWeapon(Pilot owner) {
        _owner = owner;
    }

    public bool CanFire(VesselState state, Vector2 cursor) {
        if (_cooldown != 0 || !state.CanConsumeEnergy(Design.energyCost)) {
            return false;
        }
        var dstRotation = cursor.AngleToPoint(_owner.Vessel.Position);
        return Mathf.Abs(QMath.RotationDiff(dstRotation, _owner.Vessel.Rotation)) <= 1.57;
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

        var projectile = CutterProjectile.New(_owner, Design);
        projectile.Position = _owner.Vessel.Position;
        projectile.Rotation = (cursor - _owner.Vessel.Position).Normalized().Angle();
        _owner.Vessel.GetParent().AddChild(projectile);
    }
}
