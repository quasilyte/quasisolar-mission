using Godot;

public class DiskThrowerWeapon : IWeapon {
    public static WeaponDesign Design = new WeaponDesign{
        name = "Disk Thrower",
        level = 3,
        description = "Sends spinning bladed disks of death into the space",
        extraDescription = "Disks do not disappear if missed and serve as space mines",
        targeting = "selected location, projectiles",
        sellingPrice = 6800,
        cooldown = 1.10f,
        energyCost = 6.0f,
        range = 375.0f,
        damage = 15.0f,
        damageKind = DamageKind.Kinetic,
        projectileSpeed = 180.0f,
        botHintSnipe = 0.8f,
        botHintScatter = 0,
    };
    public WeaponDesign GetDesign() { return Design; }
    public void Ready() {}
    public void Charge(float delta) {}

    private float _cooldown;
    private Pilot _owner;

    public DiskThrowerWeapon(Pilot owner) {
        _owner = owner;
    }

    public bool CanFire(VesselState state, Vector2 cursor) {
        return _cooldown == 0 && _owner.Vessel.Position.DistanceTo(cursor) < Design.range;
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
        var target = QMath.RandomizedLocation(cursor, 24);
        var projectile = DiskProjectile.New(_owner, target);
        projectile.Position = _owner.Vessel.Position;
        projectile.Rotation = (target - _owner.Vessel.Position).Normalized().Angle();
        _owner.Vessel.GetParent().AddChild(projectile);
    }
}
