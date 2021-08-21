using Godot;

public class ReaperCannonWeapon : IWeapon {
    public static WeaponDesign Design = new WeaponDesign {
        name = "Reaper Cannon",
        level = 2,
        description = "A high-recoil cannon",
        extraDescription = "The recoil is so high it pulls the ship away",
        targeting = "front directions (60°), projectiles",
        sellingPrice = 7000,
        cooldown = 1.0f,
        range = 480,
        damage = 18,
        damageKind = DamageKind.Kinetic,
        projectileSpeed = 350,
        botHintSnipe = 0.45f,
        botHintScatter = 0.25f,
        isSpecial = true,
    };
    public WeaponDesign GetDesign() { return Design; }
    public void Ready() {}

    private float _cooldown;
    private Pilot _owner;

    public ReaperCannonWeapon(Pilot owner) {
        _owner = owner;
    }

    public bool CanFire(VesselState state, Vector2 cursor) {
        if (_cooldown != 0) {
            return false;
        }
        var dstRotation = cursor.AngleToPoint(_owner.Vessel.Position);
        return Mathf.Abs(QMath.RotationDiff(dstRotation, _owner.Vessel.Rotation)) <= 0.5;
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
        projectile.Position = _owner.Vessel.Position;
        projectile.Rotation = (cursor - _owner.Vessel.Position).Normalized().Angle();
        _owner.Vessel.GetParent().AddChild(projectile);

        _owner.Vessel.State.velocity += -projectile.Transform.x * 40;
    }
}
