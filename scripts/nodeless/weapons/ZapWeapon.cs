using Godot;

public class ZapWeapon : IWeapon {
    public static WeaponDesign Design = new WeaponDesign{
        name = "Zap",
        level = 2,
        description = "TODO",
        targeting = "any direction, projectiles",
        sellingPrice = 2800,
        technologiesNeeded = {"Zap"},
        cooldown = 1.1f,
        energyCost = 15,
        range = 220,
        damage = 3,
        damageKind = DamageKind.Energy,
        projectileSpeed = 1000.0f,
        botHintSnipe = 0,
    };
    public WeaponDesign GetDesign() { return Design; }
    
    private float _cooldown;
    private Pilot _owner;

    private int _burst = 0;
    private float _burstCooldown = 0;

    private RayCast2D _raycast;

    private Node2D _targetLock;
    private Vector2 _burstTarget;

    public ZapWeapon(Pilot owner) {
        _owner = owner;
    }

    public void SetTargetLock(Node2D target) {
        _targetLock = target;
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

    public void Process(VesselState state, float delta) {
        _cooldown -= delta;
        if (_cooldown < 0) {
            _cooldown = 0;
        }
        _burstCooldown -= delta;
        if (_burstCooldown < 0) {
            _burstCooldown = 0;
        }
        if (_burst > 0 && _burstCooldown == 0) {
            _burst--;
            _burstCooldown += 0.05f;
            CreateZap(FindTarget());
        }
    }

    private Vector2 FindTarget() {
        if (!Object.IsInstanceValid(_targetLock)) {
            _targetLock = null;
        }
        return _targetLock != null ? _targetLock.Position : _burstTarget;
    }

    private void CreateZap(Vector2 cursor) {
        var randomizedCursor = QMath.RandomizedLocation(cursor, 6);
        var maxRange = ZapWeapon.Design.range;
        var zapFrom = _owner.Vessel.Position;
        var zapTo = zapFrom + ((randomizedCursor - zapFrom).Normalized() * maxRange);

        _raycast.Position = zapFrom;
        _raycast.CastTo = _raycast.ToLocal(zapTo);
        _raycast.ForceRaycastUpdate();
        var collider = _raycast.GetCollider();
        if (collider != null && collider is Area2D collidingArea) {
            zapTo = _raycast.GetCollisionPoint();
            if (collidingArea.GetParent() is VesselNode collidingVessel) {
                collidingVessel.EmitTargetedByZap();
                collidingVessel.ApplyDamage(Design.damage, Design.damageKind);
            }
        }

        var projectile = ZapProjectileNode.New(zapFrom, zapTo);
        _owner.Vessel.GetParent().AddChild(projectile);

        var spark = SparkExplosionNode.New();
        spark.Position = QMath.RandomizedLocation(zapTo, 8);
        spark.Rotation = QRandom.FloatRange(0, 2);
        var sparkScale = QRandom.FloatRange(0.7f, 1.1f);
        spark.Scale = new Vector2(sparkScale, sparkScale);
        _owner.Vessel.GetParent().AddChild(spark);
    }

    public void Fire(VesselState state, Vector2 cursor) {
        _cooldown += Design.cooldown;
        state.ConsumeEnergy(Design.energyCost);

        _burst = 5;
        _burstTarget = cursor;

        CreateZap(cursor);

        var sfx = SoundEffectNode.New(GD.Load<AudioStream>("res://audio/weapon/Zap_Gun.wav"));
        _owner.Vessel.GetParent().AddChild(sfx);
    }
}
