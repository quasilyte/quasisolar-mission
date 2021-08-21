using Godot;

public class PlasmaEmitterWeapon : IWeapon {
    public static WeaponDesign Design = new WeaponDesign {
        name = "Plasma Emitter",
        level = 3,
        description = "Creates a super-heated aura of destruction around the ship",
        extraDescription = "Every collided enemy receives damage only once",
        targeting = "none, circular wave",
        sellingPrice = 10000,
        cooldown = 5.0f,
        energyCost = 20.0f,
        range = 150.0f,
        damage = 38.0f,
        damageKind = DamageKind.Thermal,
        botHintRange = 175,
    };
    public WeaponDesign GetDesign() { return Design; }
    public void Ready() {}

    private float _cooldown;
    private Pilot _owner;

    public PlasmaEmitterWeapon(Pilot owner) {
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

        var aura = PlasmaAura.New(Design, _owner);
        _owner.Vessel.AddChild(aura);

        var sfx = SoundEffectNode.New(GD.Load<AudioStream>("res://audio/weapon/Plasma_Emitter.wav"));
        _owner.Vessel.GetParent().AddChild(sfx);
    }
}
