using Godot;
using System;

public class SwarmNode : Node2D, IProjectile {
    private float _hp;
    private float _range;
    private float _speed;

    private int _numHits = 5;
    private bool _comingBack = false;

    private Node2D _target;
    private Pilot _firedBy;

    private static AudioStream _destroyedAudioStream;

    public WeaponDesign GetWeaponDesign() { return SwarmSpawnerWeapon.Design; }
    public Pilot FiredBy() { return _firedBy; }
    public Node2D GetProjectileNode() { return this; }

    private static PackedScene _scene = null;
    public static SwarmNode New(Pilot owner) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/SwarmNode.tscn");
        }
        var o = (SwarmNode)_scene.Instance();
        o._firedBy = owner;
        return o;
    }

    public override void _Ready() {
        AddToGroup("swarm");
    }

    public void Start(Node2D target) {
        _hp = 10;
        _range = SwarmSpawnerWeapon.Design.range;
        _speed = SwarmSpawnerWeapon.Design.projectileSpeed;
        _target = target;
    }

    public void ApplyDamage(float amount) {
        _hp -= amount;
        if (_hp <= 0) {
            Explode();
        }
    }

    public override void _Process(float delta) {
        if (!IsInstanceValid(_firedBy.Vessel)) {
            Explode();
            return;
        }

        if (!_comingBack && (_range < 0 || !IsInstanceValid(_target))) {
            StartComingBack();
        }

        if (_comingBack && !IsInstanceValid(_target)) {
            Explode();
            return;
        }

        if (_comingBack && GlobalPosition.DistanceTo(_target.Position) < 20) {
            QueueFree();
            return;
        }

        FlyTowards(_target.Position, delta);
    }

    private void StartComingBack() {
        _comingBack = true;
        _target = _firedBy.Vessel;
    }

    private void FlyTowards(Vector2 target, float delta) {
        var dstRotation = target.AngleToPoint(Position);
        var rotationDiff = QMath.RotationDiff(dstRotation, Rotation);
        var rotationAmount = 1.5f * delta;
        if (Math.Abs(rotationDiff) >= rotationAmount) {
            if (rotationDiff < 0) {
                Rotation += rotationAmount;
            } else {
                Rotation -= rotationAmount;
            }
        } else {
            Rotation = dstRotation;
        }

        float traveled = SwarmSpawnerWeapon.Design.projectileSpeed * delta;
        Position += Transform.x.Normalized() * traveled;
    }

    public void OnImpact() {
        _numHits--;
        if (_numHits <= 0) {
            StartComingBack();
        }
        // Explode();
        // if (_destroyedAudioStream == null) {
        //     _destroyedAudioStream = GD.Load<AudioStream>("res://audio/rocket_impact.wav");
        // }
        // var sfx = SoundEffectNode.New(_destroyedAudioStream, -3);
        // GetParent().AddChild(sfx);
    }

    public void Explode() {
        var explosion = Explosion.New();
        explosion.Scale = new Vector2(1.2f, 1.2f);
        explosion.Modulate = Color.Color8(50, 120, 255);
        explosion.Position = Position;
        GetParent().AddChild(explosion);
        QueueFree();
    }
}
