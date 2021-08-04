using Godot;
using System;

public class Rocket : Node2D, IProjectile {
    private float _hp;
    private float _speed;
    private Vector2 _velocity;

    private Node2D _target;
    private Pilot _firedBy;
    public WeaponDesign weapon;
    private float _steer;

    private static AudioStream _destroyedAudioStream;

    public WeaponDesign GetWeaponDesign() { return weapon; }
    public Pilot FiredBy() { return _firedBy; }

    private static PackedScene _scene = null;
    public static Rocket New(Pilot owner, WeaponDesign weapon) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/Rocket.tscn");
        }
        var o = (Rocket)_scene.Instance();
        o._firedBy = owner;
        o.weapon = weapon;
        if (weapon == RocketLauncherWeapon.Design) {
            o._steer = 100;
        } else if (weapon == HurricaneWeapon.Design) {
            o._steer = 150;
        } else if (weapon == ShieldBreakerWeapon.Design) {
            o._steer = 200;
        }
        return o;
    }

    public override void _Ready() {
        AddToGroup("rockets");
        if (weapon == HurricaneWeapon.Design) {
            GetNode<Sprite>("Sprite").Texture = GD.Load<Texture>("res://images/ammo/hurricane_rocket.png");
        } else if (weapon == ShieldBreakerWeapon.Design) {
            GetNode<Sprite>("Sprite").Texture = GD.Load<Texture>("res://images/ammo/Shield_Breaker.png");
        }
    }

    public void Start(Node2D target) {
        _hp = weapon.range;
        _speed = weapon.projectileSpeed;
        if (_firedBy.Vessel.artifacts.Exists(x => x is MissileTargeterArtifact)) {
            _hp *= 1.15f;
            _speed *= 1.1f;
        }
        _target = target;
        _velocity = Transform.x * _speed;
    }

    private Vector2 seek() {
        var steer = Vector2.Zero;
        if (_target != null) {
            var dst = (_target.Position - Position).Normalized() * _speed;
            steer = (dst - _velocity).Normalized() * _steer;
        }
        return steer;
    }

    public override void _PhysicsProcess(float delta) {
        if (_target != null && !Godot.Object.IsInstanceValid(_target)) {
            // Target is lost (probably destroyed).
            _target = null;
        }
        var acceleration = Vector2.Zero;
        acceleration += seek();
        _velocity += acceleration * delta;
        _velocity = _velocity.Clamped(_speed);
        Rotation = _velocity.Angle();
        Position += _velocity * delta;
    }

    public override void _Process(float delta) {
        if (_hp < 0) {
            Explode();
            return;
        }

        float traveled = _speed * delta;
        _hp -= traveled;
    }

    public void OnImpact() {
        Explode();
        if (_destroyedAudioStream == null) {
            _destroyedAudioStream = GD.Load<AudioStream>("res://audio/rocket_impact.wav");
        }
        var sfx = SoundEffectNode.New(_destroyedAudioStream, -11);
        GetParent().AddChild(sfx);
    }

    public void Explode() {
        var explosion = Explosion.New();
        if (weapon == HurricaneWeapon.Design) {
            explosion.Modulate = Color.Color8(140, 140, 255);
        } else if (weapon == ShieldBreakerWeapon.Design) {
            explosion.Scale = new Vector2(1.2f, 1.2f);
            explosion.Modulate = Color.Color8(50, 120, 255);
        }
        explosion.Position = Position;
        GetParent().AddChild(explosion);
        QueueFree();
    }
}
