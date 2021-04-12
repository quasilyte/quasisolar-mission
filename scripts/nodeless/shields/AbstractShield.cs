using Godot;

public abstract class AbstractShield : IShield {
    public abstract ShieldDesign GetDesign();

    public AbstractShield(Pilot pilot) { _pilot = pilot; }

    private float _cooldown = 0;
    protected float _activation = 0;
    protected Pilot _pilot;

    protected string _audioName;
    protected string _textureName = "";

    private ShieldAuraNode _lastAura = null;

    public bool CanActivate(VesselState state) {
        return _cooldown <= 0 && state.CanConsumeEnergy(GetDesign().energyCost) && _activation == 0;
    }

    public void Deactivate() {
        _activation = 0;
        if (_lastAura != null && Godot.Object.IsInstanceValid(_lastAura)) {
            _lastAura.QueueFree();
        }
    }

    public virtual void Activate(VesselState state) {
        var design = GetDesign();
        _activation = design.duration * state.shieldDurationRate;
        _cooldown = design.cooldown * state.shieldCooldownRate;
        state.ConsumeEnergy(design.energyCost);

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

    public float ReduceDamage(float damage, DamageKind kind) {
        if (_activation <= 0) {
            return damage;
        }
        var design = GetDesign();
        if (kind == DamageKind.Energy) {
            return damage * design.activeEnergyDamageReceive;
        } else if (kind == DamageKind.Kinetic) {
            return damage * design.activeKineticDamageReceive;
        } else if (kind == DamageKind.Thermal) {
            return damage * design.activeThermalDamageReceive;
        }
        return damage;
    }
}