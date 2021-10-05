using Godot;
using System;

public class FlareProjectileNode : Node2D, IProjectile {
    private float _hp;
    private float _speed;
    private Vector2 _velocity;

    private Node2D _target;
    private Pilot _firedBy;
    private float _steer;

    private static AudioStream _destroyedAudioStream;

    public WeaponDesign GetWeaponDesign() { return FlareWeapon.Design; }
    public Pilot FiredBy() { return _firedBy; }
    public Node2D GetProjectileNode() { return this; }

    private static PackedScene _scene = null;
    public static FlareProjectileNode New(Pilot owner) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/ammo/FlareProjectileNode.tscn");
        }
        var o = (FlareProjectileNode)_scene.Instance();
        o._firedBy = owner;
        o._steer = 65;
        return o;
    }

    public override void _Ready() {
        AddToGroup("rockets");

        var nearest = QMath.NearestEnemy(GlobalPosition + Transform.x * 160, _firedBy);
        if (nearest != null) {
            Start(nearest.Vessel);
        } else {
            Start(null);
        }
    }

    public void Start(Node2D target) {
        _hp = GetWeaponDesign().range;
        _speed = GetWeaponDesign().projectileSpeed;
        if (IsInstanceValid(_firedBy.Vessel)) {
            if (_firedBy.Vessel.State.hasMissleTargeter) {
                _hp *= 1.15f;
                _speed *= 1.1f;
            }
            if (_firedBy.Vessel.State.hasMissleCoordinator) {
                _steer *= MissileCoordinatorArtifact.multiplier;
            }
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
        _velocity = _velocity.Rotated(QRandom.FloatRange(-0.04f, 0.04f));
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
            _destroyedAudioStream = GD.Load<AudioStream>("res://audio/weapon/Needle_Gun_Impact.wav");
        }
        var sfx = SoundEffectNode.New(_destroyedAudioStream, -11);
        GetParent().AddChild(sfx);
    }

    public void Explode() {
        QueueFree();
    }
}
