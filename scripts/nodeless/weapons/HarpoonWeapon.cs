using Godot;

public class HarpoonWeapon : IWeapon {
    public static WeaponDesign Design = new WeaponDesign{
        name = "Harpoon",
        level = 1,
        description = "TODO",
        extraDescription = "On hit, pulls victim towards the ship",
        targeting = "any direction, projectiles",
        sellingPrice = 2000,
        cooldown = 1.2f,
        energyCost = 7.0f,
        range = 310.0f,
        damage = 11.0f,
        damageKind = DamageKind.Energy,
        projectileSpeed = 350.0f,
        botHintSnipe = 0.5f,
        botHintScatter = 0.5f,
        isSpecial = true,
    };
    public WeaponDesign GetDesign() { return Design; }
    public void Ready() {}
    public void Charge(float delta) {}

    private float _cooldown;
    private Pilot _owner;

    public HarpoonWeapon(Pilot owner) {
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
        var projectile = HarpoonProjectile.New(_owner);
        projectile.Position = _owner.Vessel.Position;
        projectile.Rotation = (cursor - _owner.Vessel.Position).Normalized().Angle();
        _owner.Vessel.GetParent().AddChild(projectile);
    }
}
