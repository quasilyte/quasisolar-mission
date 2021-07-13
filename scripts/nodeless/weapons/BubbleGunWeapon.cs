using Godot;

public class BubbleGunWeapon : IWeapon {
    public static WeaponDesign Design = new WeaponDesign {
        name = "Bubble Gun",
        level = 1,
        description = "TODO",
        targeting = "backward-only, projectiles",
        sellingPrice = 3500,
        technologiesNeeded = {"Bubble Gun"},
        cooldown = 0.45f,
        energyCost = 7.0f,
        range = -6,
        damage = 9.0f,
        damageKind = DamageKind.Energy,
        projectileSpeed = 130.0f,
        botHintSnipe = 0,
        // botHintEffectiveAngle = -0.8f,
        botHintRange = 350,
    };
    public WeaponDesign GetDesign() { return Design; }
    public void Ready() {}

    private float _cooldown;
    private Pilot _owner;

    public BubbleGunWeapon(Pilot owner) {
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

        var sfx = SoundEffectNode.New(GD.Load<AudioStream>("res://audio/weapon/Bubble_Gun.wav"));
        _owner.Vessel.GetParent().AddChild(sfx);

        var projectile = BubbleNode.New(_owner);
        projectile.Position = _owner.Vessel.Position;
        _owner.Vessel.GetParent().AddChild(projectile);
    }
}
