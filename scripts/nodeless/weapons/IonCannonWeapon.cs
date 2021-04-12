using Godot;

public class IonCannonWeapon : IWeapon {
    public static WeaponDesign Design = new WeaponDesign{
        name = "Ion Cannon",
        level = 1,
        description = "A tactical weapon that can dry the target\nbattery up",
        targeting = "any direction, projectiles",
        sellingPrice = 1400,
        cooldown = 0.25f,
        energyCost = 4.0f,
        range = 250.0f,
        damage = 5.0f,
        damageKind = DamageKind.Energy,
        energyDamage = 12.0f,
        projectileSpeed = 320.0f,
        botHintSnipe = 0.3f,
    };
    public WeaponDesign GetDesign() { return Design; }
    public void Ready() {}

    private float _cooldown;
    private Pilot _owner;

    public IonCannonWeapon(Pilot owner) {
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
        var projectile = Projectile.New(Design, _owner);
        projectile.Position = _owner.Vessel.Position;
        projectile.Rotation = (cursor - _owner.Vessel.Position).Normalized().Angle();
        _owner.Vessel.GetParent().AddChild(projectile);
    }
}
