using Godot;

public class PullingBeamerWeapon : IWeapon {
    public static WeaponDesign Design = new WeaponDesign{
        name = "Pulling Beamer",
        level = 2,
        description = "A tractor projector that can be used to close the distance",
        extraDescription = "Pulls victim towards the ship",
        targeting = "any direction, instant hit",
        sellingPrice = 3400,
        cooldown = 0.1f,
        energyCost = 4,
        range = 450f,
        energyDamage = 1,
        isSpecial = true,
    };
    public WeaponDesign GetDesign() { return Design; }
    public void Ready() {}
    public void Charge(float delta) {}

    private float _cooldown;
    private Pilot _owner;

    public PullingBeamerWeapon(Pilot owner) {
        _owner = owner;
    }

    public bool CanFire(VesselState state, Vector2 cursor) {
        if (_cooldown != 0 || !state.CanConsumeEnergy(Design.energyCost)) {
            return false;
        }
        var e = QMath.NearestEnemy(cursor, _owner);
        if (e != null && e.Vessel.Position.DistanceTo(_owner.Vessel.Position) <= Design.range) {
            return true;
        }
        return false;
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

        var target = QMath.NearestEnemy(cursor, _owner).Vessel;
        target.State.velocity += target.Position.DirectionTo(_owner.Vessel.Position) * 30;

        target.ApplyEnergyDamage(Design.energyDamage);

        var effect = PullingBeamerEffectNode.New(_owner.Vessel.Position, target.Position);
        _owner.Vessel.GetParent().AddChild(effect);
        
        var sfx = SoundEffectNode.New(GD.Load<AudioStream>("res://audio/weapon/Pulling_Beamer.wav"));
        _owner.Vessel.GetParent().AddChild(sfx);
    }
}
