using Godot;

public abstract class AbstractWeapon : IWeapon {
    protected WeaponDesign _design;
    protected float _cooldown = 0;
    protected Pilot _owner;

    public AbstractWeapon(WeaponDesign design, Pilot owner) {
        _design = design;
        _owner = owner;
    }

    public WeaponDesign GetDesign() { return _design; }

    public void Ready() {}

    public virtual bool CanFire(VesselState state, Vector2 cursor) {
        if (_cooldown != 0) {
            return false;
        }
        if (_design.energyCost != 0 && !state.CanConsumeEnergy(_design.energyCost)) {
            return false;
        }
        return true;
    }

    public virtual void Process(VesselState state, float delta) {
        ProcessCooldown(delta);
    }

    public virtual void Fire(VesselState state, Vector2 cursor) {
        if (_design.energyCost != 0) {
            state.ConsumeEnergy(_design.energyCost);
        }

        if (_design.isSpecial) {
            _cooldown += state.hasAsyncReloader ? _design.cooldown * 0.9f : _design.cooldown;
        } else {
            _cooldown += _design.cooldown;
        }

        CreateProjectile(cursor);
    }

    public void ProcessCooldown(float delta) {
        _cooldown = QMath.ClampMin(_cooldown - delta, 0);
    }

    public virtual void Charge(float delta) {}

    protected abstract void CreateProjectile(Vector2 cursor);
}
