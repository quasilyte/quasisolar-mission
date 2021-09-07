using Godot;
using System;

public class DiskProjectile : Node2D, IProjectile {
    private float _hp;

    private Sprite _sprite;

    private static Texture _texture = null;
    private static AudioStream _audioStream = null;

    private Vector2 _velocity;
    private Vector2 _target;

    private Pilot _firedBy;

    // FIXME: duplicated with rocket.
    private static AudioStream _destroyedAudioStream;

    public WeaponDesign GetWeaponDesign() { return DiskThrowerWeapon.Design; }
    public Pilot FiredBy() { return _firedBy; }
    public Node2D GetProjectileNode() { return this; }

    private static PackedScene _scene = null;
    public static DiskProjectile New(Pilot owner, Vector2 target) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/DiskProjectile.tscn");
        }
        var o = (DiskProjectile)_scene.Instance();
        o._firedBy = owner;
        o._target = target;
        return o;
    }

    public override void _Ready() {
        _velocity = Transform.x * (DiskThrowerWeapon.Design.projectileSpeed);
        _hp = 10;
        if (_texture == null) {
            _texture = GD.Load<Texture>("res://images/ammo/disk.png");
        }
        if (_audioStream == null) {
            _audioStream = GD.Load<AudioStream>("res://audio/disk.wav");
        }

        _sprite = GetNode<Sprite>("Sprite");
        _sprite.Texture = _texture;

        var sfx = SoundEffectNode.New(_audioStream, -6);
        GetParent().AddChild(sfx);
    }

    public override void _PhysicsProcess(float delta) {
        if (Position.DistanceTo(_target) < 8) {
            _velocity = Vector2.Zero;
        }
        if (_velocity != Vector2.Zero) {
            Position += _velocity * delta;
        }
    }

    public override void _Process(float delta) {
        if (_hp < 0) {
            Explode();
            QueueFree();
            return;
        }

        Rotation -= delta * 4;
        _hp -= delta;
    }

    public void OnImpact() {
        Explode();
        QueueFree();
        if (_destroyedAudioStream == null) {
            _destroyedAudioStream = GD.Load<AudioStream>("res://audio/rocket_impact.wav");
        }
        var sfx = SoundEffectNode.New(_destroyedAudioStream, -5);
        GetParent().AddChild(sfx);
    }

    private void Explode() {
        var explosion = DiskExplosion.New();
        explosion.Position = Position;
        explosion.Rotation = Rotation;
        GetParent().AddChild(explosion);
    }
}
