using Godot;

public class PulseBladeWeapon : IWeapon {
    public static WeaponDesign Design = new WeaponDesign{
        name = "Pulse Blade",
        level = 2,
        description = "TODO",
        targeting = "forward-only, beam",
        sellingPrice = 6000,
        cooldown = 2.0f,
        energyCost = 20,
        range = 440,
        damage = 15,
        burst = 5,
        damageKind = DamageKind.Electromagnetic,
        projectileSpeed = 1000.0f,
        botHintSnipe = 0,
        botHintEffectiveAngle = 0.1f,
    };
    public WeaponDesign GetDesign() { return Design; }
    public void Charge(float delta) {}
    
    private float _cooldown;
    private Pilot _owner;

    private int _burst = 0;
    private float _burstCooldown = 0;

    private PulseBladeProjectileNode _projectile;
    private RayCast2D _raycast;

    public PulseBladeWeapon(Pilot owner) {
        _owner = owner;
    }

    public void Ready() {
        _raycast = new RayCast2D();
        _raycast.Enabled = false;
        _raycast.CollideWithAreas = true;
        _raycast.CollideWithBodies = false;
        _raycast.CollisionMask = 2;

        _raycast.AddException(_owner.Vessel.GetNode<Area2D>("Area2D"));
        foreach (var ally in _owner.Allies) {
            _raycast.AddException(ally.Vessel.GetNode<Area2D>("Area2D"));
        }

        _owner.Vessel.GetParent().AddChild(_raycast);
    }

    public bool CanFire(VesselState state, Vector2 cursor) {
        return _cooldown == 0 && state.CanConsumeEnergy(Design.energyCost);
    }

    public void OnVesselDestroyed() {
        if (_projectile != null) {
            RemoveProjectile();
        }
    }

    public void Process(VesselState state, float delta) {
        _cooldown -= delta;
        if (_cooldown < 0) {
            _cooldown = 0;
        }

        if (!Godot.Object.IsInstanceValid(_projectile)) {
            _projectile = null;
        }

        _burstCooldown -= delta;
        if (_burstCooldown < 0) {
            _burstCooldown = 0;
        }
        VesselNode collidingVessel = null;
        if (_projectile != null) {
            collidingVessel = AdjustProjectile();
        }
        if (_burst > 0 && _burstCooldown == 0) {
            _burst--;
            _burstCooldown += 0.15f;
            if (collidingVessel != null) {
                collidingVessel.EmitTargetedByZap();
                collidingVessel.ApplyDamage(Design.damage, Design.damageKind);
            }
            if (_burst == 0 && _projectile != null) {
                RemoveProjectile();
            }
        }
        
    }

    public void RemoveProjectile() {
        _projectile.QueueFree();
        _projectile = null;
    }

    public VesselNode AdjustProjectile() {
        VesselNode target = null;

        var from = _owner.Vessel.Position;
        var to = _owner.Vessel.Position + (_owner.Vessel.Transform.x * Design.range);

        _raycast.Position = from;
        _raycast.CastTo = _raycast.ToLocal(to);
        _raycast.ForceRaycastUpdate();
        var collider = _raycast.GetCollider();
        if (collider != null && collider is Area2D collidingArea) {
            to = _raycast.GetCollisionPoint();
            if (collidingArea.GetParent() is VesselNode collidingVessel) {
                target = collidingVessel;
            }
        }

        _projectile.from = from;
        _projectile.to = to;

        return target;
    }

    public void Fire(VesselState state, Vector2 cursor) {
        _cooldown += Design.cooldown;
        state.ConsumeEnergy(Design.energyCost);

        _burst = Design.burst;

        _projectile = PulseBladeProjectileNode.New();
        AdjustProjectile();
        _owner.Vessel.GetParent().AddChild(_projectile);

        var sfx = SoundEffectNode.New(GD.Load<AudioStream>("res://audio/weapon/Pulse_Blade.wav"), -3);
        _owner.Vessel.GetParent().AddChild(sfx);
    }
}
