using Godot;

public class WarpDeviceWeapon : IWeapon {
    public static WeaponDesign Design = new WeaponDesign {
        name = "Warp Device",
        level = 3,
        description = "A portable short-range teleporter",
        extraDescription = "Clears all waypoints when used",
        targeting = "selected location",
        sellingPrice = 6200,
        cooldown = 0.75f,
        energyCost = 20.0f,
        range = 400.0f,
        damageKind = DamageKind.None,
        isSpecial = true,
    };
    public WeaponDesign GetDesign() { return Design; }
    public void Ready() {}
    public void Charge(float delta) {}

    private float _cooldown;
    private Pilot _owner;

    public WarpDeviceWeapon(Pilot owner) {
        _owner = owner;
    }

    public bool CanFire(VesselState state, Vector2 cursor) {
        return _cooldown == 0 && state.CanConsumeEnergy(Design.energyCost) && _owner.Vessel.Position.DistanceTo(cursor) < Design.range;
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

        var sfx = SoundEffectNode.New(GD.Load<AudioStream>("res://audio/weapon/Warp_Device.wav"), -8);
        _owner.Vessel.GetParent().AddChild(sfx);

        var effect = WarpEffect.New();
        effect.Position = _owner.Vessel.Position;
        _owner.Vessel.GetParent().AddChild(effect);

        _owner.Vessel.ClearWaypoints();
        _owner.Vessel.Position = cursor;

        // var projectile = Projectile.New(Design, _owner);
        // projectile.Position = _owner.Vessel.Position;
        // projectile.Rotation = (cursor - _owner.Vessel.Position).Normalized().Angle();
        // _owner.Vessel.GetParent().AddChild(projectile);
    }
}
