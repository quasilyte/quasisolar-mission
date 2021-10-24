using Godot;

public class MortarWeapon : IWeapon {
    public static WeaponDesign Design = new WeaponDesign {
        name = "Mortar",
        level = 3,
        description = "TODO",
        targeting = "selected location, projectiles",
        sellingPrice = 8500,
        cooldown = 2f,
        energyCost = 18.0f,
        range = 650,
        minRange = 200,
        damage = 25,
        damageKind = DamageKind.Thermal,
        projectileSpeed = 300,
        botHintSnipe = 0.9f,
        isSpecial = true,
        ignoresAsteroids = true,
    };
    public WeaponDesign GetDesign() { return Design; }
    public void Ready() {}
    public void Charge(float delta) {}

    private float _cooldown;
    private Pilot _owner;

    private MortarProjectile _activeProjectile = null;

    public MortarWeapon(Pilot owner) {
        _owner = owner;
    }

    public bool CanFire(VesselState state, Vector2 cursor) {
        if (!Godot.Object.IsInstanceValid(_activeProjectile)) {
            _activeProjectile = null;
        }
        if (_activeProjectile != null || _cooldown != 0) {
            return false;
        }
        if (!state.CanConsumeEnergy(Design.energyCost)) {
            return false;
        }
        var dist = _owner.Vessel.Position.DistanceTo(cursor);
        return dist >= Design.minRange;
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

        var projectile = MortarProjectile.New(_owner, cursor);
        _activeProjectile = projectile;
        projectile.Position = _owner.Vessel.Position;
        projectile.Rotation = (cursor - _owner.Vessel.Position).Normalized().Angle();
        _owner.Vessel.GetParent().AddChild(projectile);
    }
}
