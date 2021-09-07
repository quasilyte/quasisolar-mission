using Godot;

public class LancerWeapon : IWeapon {
    public static WeaponDesign Design = new WeaponDesign{
        name = "Lancer",
        level = 3,
        description = "TODO",
        targeting = "any direction, projectiles",
        sellingPrice = 20000,
        cooldown = 1.7f,
        energyCost = 19.0f,
        range = 380.0f,
        damage = 30.0f,
        damageKind = DamageKind.Energy,
        projectileSpeed = 550.0f,
        botHintSnipe = 0.7f,
    };
    public WeaponDesign GetDesign() { return Design; }
    public void Ready() {}
    public void Charge(float delta) {}
   
    private float _cooldown;
    private Pilot _owner;

    public LancerWeapon(Pilot owner) {
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

        var line = LancerLine.New();
        var projectile = LancerProjectile.New(line, _owner);
        projectile.Position = _owner.Vessel.Position;
        projectile.Rotation = (cursor - _owner.Vessel.Position).Normalized().Angle();
        _owner.Vessel.GetParent().AddChild(line);
        _owner.Vessel.GetParent().AddChild(projectile);

        var sfx = SoundEffectNode.New(GD.Load<AudioStream>("res://audio/weapon/Lancer.wav"));
        _owner.Vessel.GetParent().AddChild(sfx);
    }
}
