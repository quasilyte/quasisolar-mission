using Godot;
using System;

public class TorpedoNode : Node2D, IProjectile {
    private float _hp;
    private float _range;
    private float _speed;
    private Vector2 _velocity;

    private Node2D _target;
    private Pilot _firedBy;
    private float _steer;

    private static AudioStream _destroyedAudioStream;

    public WeaponDesign GetWeaponDesign() { return TorpedoLauncherWeapon.Design; }
    public Pilot FiredBy() { return _firedBy; }
    public Node2D GetProjectileNode() { return this; }

    private static PackedScene _scene = null;
    public static TorpedoNode New(Pilot owner) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/TorpedoNode.tscn");
        }
        var o = (TorpedoNode)_scene.Instance();
        o._firedBy = owner;
        o._steer = 175;
        return o;
    }

    public override void _Ready() {
        AddToGroup("rockets");
    }

    public void Start(Node2D target) {
        _hp = 6;
        _range = TorpedoLauncherWeapon.Design.range;
        _speed = TorpedoLauncherWeapon.Design.projectileSpeed;
        if (_firedBy.Vessel.artifacts.Exists(x => x is MissileTargeterArtifact)) {
            _range *= 1.15f;
            _speed *= 1.1f;
        }
        _target = target;
        _velocity = Transform.x * _speed;
    }

    public void ApplyDamage(float amount) {
        _hp -= amount;
        if (_hp <= 0) {
            Explode();
        }
    }

    private Vector2 seek() {
        var steer = Vector2.Zero;
        if (_target != null) {
            var dst = (_target.Position - Position).Normalized() * _speed;
            steer = (dst - _velocity).Normalized() * 100.0f;
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
        if (_range < 0) {
            Explode();
            return;
        }

        float traveled = _speed * delta;
        _range -= traveled;
    }

    public void OnImpact() {
        Explode();
        if (_destroyedAudioStream == null) {
            _destroyedAudioStream = GD.Load<AudioStream>("res://audio/rocket_impact.wav");
        }
        var sfx = SoundEffectNode.New(_destroyedAudioStream, -3);
        GetParent().AddChild(sfx);
    }

    public void Explode() {
        var explosion = Explosion.New();
        explosion.Scale = new Vector2(1.5f, 1.5f);
        explosion.Position = Position;
        GetParent().AddChild(explosion);
        QueueFree();
    }
}
