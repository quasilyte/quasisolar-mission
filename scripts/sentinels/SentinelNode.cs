using Godot;
using System;

public abstract class SentinelNode : Node2D {
    public Pilot pilot;

    protected VesselNode _vessel;
    protected SentinelDesign _design;
    protected Sprite _sprite;

    protected float _attackCooldown = 0;

    private float _hp;

    private static PackedScene _scene = null;
    public static SentinelNode New(VesselNode vessel, SentinelDesign design) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/SentinelNode.tscn");
        }
        var o = (SentinelNode)_scene.Instance();
        o._vessel = vessel;
        o._design = design;
        o.pilot = vessel.pilot;
        return o;
    }

    public override void _Ready() {
        _hp = _design.hp;

        var area = GetNode<Area2D>("Pivot/Sprite/Area2D");
        area.Connect("area_entered", this, nameof(OnCollision));
        
        _sprite = GetNode<Sprite>("Pivot/Sprite");
        
        GetNode<Node2D>("Pivot").Rotation = QRandom.FloatRange(-3, 3);
    }

    public override void _Process(float delta) {
        if (_vessel == null) {
            return;
        }
        if (!IsInstanceValid(_vessel)) {
            _vessel = null;
            Destroy(true);
            return;
        }

        ProcessFrame(delta);

        _attackCooldown = QMath.ClampMin(_attackCooldown - delta, 0);

        GlobalPosition = _vessel.GlobalPosition;
        GetNode<Node2D>("Pivot").Rotation += 1 * delta;
        _sprite.GlobalRotation = 0;

        if (_attackCooldown == 0) {
            ProcessAttack();
        }
    }

    private void OnCollision(Area2D other) {
        if (other.GetParent() is PlasmaAura plasmaAura) {
            if (plasmaAura.FiredBy.alliance == pilot.alliance) {
                return;
            }
            ApplyDamage(PlasmaEmitterWeapon.Design.damage, PlasmaEmitterWeapon.Design.damageKind);
            return;
        }

        if (other.GetParent() is IProjectile projectile) {
            var firedBy = projectile.FiredBy();
            if (firedBy.alliance == pilot.alliance) {
                return;
            }
            var design = projectile.GetWeaponDesign();
            projectile.OnImpact();
            ApplyDamage(design.damage, design.damageKind);

            return;
        }
    }

    public void ApplyDamage(float amount, DamageKind kind) {
        if (amount > 0) {
            Color color;
            var hpPercentage = _hp / _design.hp;
            if (hpPercentage >= 0.8) {
                color = Color.Color8(0x47, 0xe5, 0x3f);
            } else if (hpPercentage >= 0.5) {
                color = Color.Color8(0x8f, 0xcf, 0x43);
            } else if (hpPercentage >= 0.2) {
                color = Color.Color8(0xde, 0x7a, 0x31);
            } else {
                color = Color.Color8(0xff, 0x73, 0x47);
            }
            var score = DamageScoreNode.New((int)amount, color, false);
            GetParent().AddChild(score);
            score.GlobalPosition = QMath.RandomizedLocation(_sprite.GlobalPosition, 8);
        }

        _hp -= amount;
        if (_hp <= 0) {
            _hp = 0;
            Destroy();
        }
    }

    private void Destroy(bool quiet = false) {
        if (!quiet) {
            var sfx = SoundEffectNode.New(GD.Load<AudioStream>("res://audio/rocket_impact.wav"), -2);
            GetParent().AddChild(sfx);
        }

        OnDestroy();

        var e = Explosion.New(0.5f);
        GetParent().AddChild(e);
        e.GlobalPosition = _sprite.GlobalPosition;
        QueueFree();
    }

    protected virtual void OnDestroy() {}
    
    protected abstract void ProcessAttack();

    protected virtual void ProcessFrame(float delta) {}
}
