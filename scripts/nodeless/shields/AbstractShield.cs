using Godot;

public abstract class AbstractShield : IShield {
    public abstract ShieldDesign GetDesign();

    public AbstractShield(Pilot pilot) { _pilot = pilot; }

    private float _cooldown = 0;
    protected float _activation = 0;
    protected Pilot _pilot;

    protected string _audioName;
    protected string _textureName = "";

    protected Node _lastAura = null;

    public float ActivationCost(VesselState state) {
        return GetDesign().energyCost + state.stats.shieldExtraActivationCost;
    }

    public bool CanActivate(VesselState state) {
        return _cooldown <= 0 && state.CanConsumeEnergy(ActivationCost(state)) && _activation == 0;
    }

    public void Deactivate() {
        _activation = 0;
        if (_lastAura != null && Godot.Object.IsInstanceValid(_lastAura)) {
            _lastAura.QueueFree();
        }
    }

    public virtual void Activate(VesselState state) {
        var design = GetDesign();
        _activation = design.duration * state.stats.shieldDurationRate;
        _cooldown = design.cooldown * state.shieldCooldownRate;
        state.ConsumeEnergy(ActivationCost(state));

        var sfx = SoundEffectNode.New(GD.Load<AudioStream>(_audioName));
        _pilot.Vessel.GetParent().AddChild(sfx);

        if (_textureName != "") {
            _lastAura = ShieldAuraNode.New(_pilot.Vessel, GD.Load<Texture>(_textureName), design);
            _pilot.Vessel.GetParent().AddChild(_lastAura);
        }
    }

    public void Process(VesselState state, float delta) {
        _activation = QMath.ClampMin(_activation - delta, 0);
        _cooldown = QMath.ClampMin(_cooldown - delta, 0);
    }

    public bool IsActive() {
        return _activation > 0;
    }

    public static float CalculateReducedDamage(ShieldDesign design, float damage, DamageKind kind) {
        if (kind == DamageKind.Electromagnetic) {
            return damage * design.activeElectromagneticDamageReceive;
        } else if (kind == DamageKind.Kinetic) {
            return damage * design.activeKineticDamageReceive;
        } else if (kind == DamageKind.Thermal) {
            return damage * design.activeThermalDamageReceive;
        }
        return damage;
    }

    public float ReduceDamage(float damage, DamageKind kind) {
        if (_activation <= 0) {
            return damage;
        }
        var design = GetDesign();
        return CalculateReducedDamage(design, damage, kind);
    }
}