using Godot;

public class CutterWeapon : IWeapon {
    public static WeaponDesign Design = new WeaponDesign {
        name = "Cutter",
        level = 2,
        description = "TODO",
        targeting = "front directions (180°), projectiles",
        sellingPrice = 3900,
        cooldown = 1,
        energyCost = 13,
        range = 330,
        damage = 15,
        damageKind = DamageKind.Energy,
        projectileSpeed = 280,
        botHintSnipe = 0.2f,
    };
    public WeaponDesign GetDesign() { return Design; }
    public void Ready() {}

    private float _cooldown;
    private Pilot _owner;

    public CutterWeapon(Pilot owner) {
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
