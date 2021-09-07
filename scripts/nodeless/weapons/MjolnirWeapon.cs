using Godot;

public class MjolnirWeapon : IWeapon {
    public static WeaponDesign Design = new WeaponDesign {
        name = "Mjolnir",
        level = 3,
        description = "TODO",
        targeting = "selected location, projectiles",
        sellingPrice = 10000,
        cooldown = 2.5f,
        energyCost = 17,
        range = 550,
        minRange = 250,
        damage = 20,
        energyDamage = 25,
        damageKind = DamageKind.Electromagnetic,
        projectileSpeed = 320,
        botHintSnipe = 0.8f,
        isSpecial = true,
        ignoresAsteroids = true,
    };
    public WeaponDesign GetDesign() { return Design; }
    public void Ready() {}
    public void Charge(float delta) {}

    private float _cooldown;
    private Pilot _owner;

    private MjolnirProjectile _activeProjectile = null;

    public MjolnirWeapon(Pilot owner) {
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

        var projectile = MjolnirProjectile.New(_owner, cursor);
        _activeProjectile = projectile;
        projectile.Position = _owner.Vessel.Position;
        projectile.Rotation = (cursor - _owner.Vessel.Position).Normalized().Angle();
        _owner.Vessel.GetParent().AddChild(projectile);
    }
}
