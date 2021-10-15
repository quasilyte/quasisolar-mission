using Godot;

public class PhotonBeamWeapon : IWeapon {
    public static WeaponDesign Design = new WeaponDesign{
        name = "Photon Beam",
        level = 3,
        description = "Fires concentrated photon-energy beam",
        extraDescription = "Weapon needs charging, so there is a pre-fire delay",
        targeting = "any direction, instant hit",
        sellingPrice = 7500,
        cooldown = 2.8f,
        energyCost = 25f,
        range = 320f,
        damage = 24f,
        damageKind = DamageKind.Electromagnetic,
        isSpecial = true,
    };
    public WeaponDesign GetDesign() { return Design; }
    public void Ready() {}
    public void Charge(float delta) {}

    private float _cooldown;
    private Pilot _owner;

    private float _fireDelay = 0;
    private VesselNode _target = null;

    public PhotonBeamWeapon(Pilot owner) {
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
        _fireDelay -= delta;
        if (_fireDelay < 0) {
            _fireDelay = 0;
        }
        if (_target != null && _fireDelay == 0) {
            var target = _target;
            _target = null;
            if (!Godot.Object.IsInstanceValid(target)) {
                return; // Target was destroyed
            }
            if (target.Position.DistanceTo(_owner.Vessel.Position) > Design.range) {
                return; // Target got too far, can't fire
            }
            var color = Color.Color8(0x0c, 0x9b, 0xc4);
            var beam = Beam.New(_owner.Vessel.Position, QMath.RandomizedLocation(target.Position, 8), color, 6);
            beam.damage = Design.damage;
            beam.damageKind = Design.damageKind;
            beam.target = target;
            _owner.Vessel.GetParent().AddChild(beam);
            var sfx = SoundEffectNode.New(GD.Load<AudioStream>("res://audio/weapon/Photon_Beam.wav"));
            _owner.Vessel.GetParent().AddChild(sfx);
        }
    }

    public void Fire(VesselState state, Vector2 cursor) {
        _cooldown += Design.cooldown;
        state.ConsumeEnergy(Design.energyCost);
        _target = QMath.NearestEnemy(cursor, _owner).Vessel;
        _fireDelay = 0.7f;
        var sfx = SoundEffectNode.New(GD.Load<AudioStream>("res://audio/weapon/Photon_Beam_Charge.wav"));
        _owner.Vessel.GetParent().AddChild(sfx);
    }
}
