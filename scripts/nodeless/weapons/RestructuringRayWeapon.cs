using Godot;

public class RestructuringRayWeapon : IWeapon {
    public static WeaponDesign Design = new WeaponDesign {
        name = "Restructuring Ray",
        level = 2,
        description = "Repairs targeted ally ship, restoring its hp",
        extraDescription = "Repairs for 2 seconds, up to 6 hp is recovered",
        targeting = "any direction, instant hit (ally only)",
        sellingPrice = 6500,
        technologiesNeeded = {"Restructuring Ray"},
        cooldown = 5.0f,
        energyCost = 12.0f,
        damage = -0.15f,
        damageKind = DamageKind.None,
        range = 250.0f,
        botHintRange = 160,
        isSpecial = true,
    };
    public WeaponDesign GetDesign() { return Design; }
    public void Ready() {}

    private float _cooldown;
    private Pilot _owner;

    private RestructuringLine _line;
    private const int MAX_BURST = 10;
    private int _burst = 0;
    private float _burstCooldown = 0;
    private VesselNode _target = null;

    public RestructuringRayWeapon(Pilot owner) {
        _owner = owner;
    }

    public bool CanFire(VesselState state, Vector2 cursor) {
        if (_cooldown != 0 || !state.CanConsumeEnergy(Design.energyCost)) {
            return false;
        }
        var ally = QMath.NearestAlly(cursor, _owner);
        if (ally != null && ally.Vessel.Position.DistanceTo(_owner.Vessel.Position) <= Design.range) {
            return true;
        }
        return false;
    }

    public void Process(VesselState state, float delta) {
        _cooldown = QMath.ClampMin(_cooldown - delta, 0);
        _burstCooldown = QMath.ClampMin(_burstCooldown - delta, 0);
        if (_burst > 0 && _burstCooldown == 0) {
            _burst--;
            if (_burst == 0) {
                _line.StartFading();
            }
            _burstCooldown += 0.2f;

            if (!Godot.Object.IsInstanceValid(_target) || _target.Position.DistanceTo(_owner.Vessel.Position) > Design.range) {
                _target = null;
                _burst = 0;
                _line.StartFading();
                return;
            }

            _target.State.hp += 0.6f;
        }
    }

    public void Fire(VesselState state, Vector2 cursor) {
        _cooldown += Design.cooldown;
        state.ConsumeEnergy(Design.energyCost);
        _target = QMath.NearestAlly(cursor, _owner).Vessel;
        _burst = MAX_BURST;
        var sfx = SoundEffectNode.New(GD.Load<AudioStream>("res://audio/weapon/Restructuring_Ray.wav"));
        _owner.Vessel.GetParent().AddChild(sfx);

        _line = RestructuringLine.New(_owner.Vessel, _target);
        _owner.Vessel.GetParent().AddChild(_line);
    }
}
