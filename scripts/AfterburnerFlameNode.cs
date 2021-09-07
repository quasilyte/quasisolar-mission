using Godot;
using System;

public class AfterburnerFlameNode : Node2D, IProjectile {
    private float _hp;

    private Sprite _sprite;

    private static AudioStream _audioStream = null;

    private Vector2 _velocity;

    private Pilot _firedBy;

    public WeaponDesign GetWeaponDesign() { return AfterburnerWeapon.Design; }
    public Pilot FiredBy() { return _firedBy; }
    public Node2D GetProjectileNode() { return this; }

    private static PackedScene _scene = null;
    public static AfterburnerFlameNode New(Pilot owner) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/AfterburnerFlameNode.tscn");
        }
        var o = (AfterburnerFlameNode)_scene.Instance();
        o._firedBy = owner;
        return o;
    }

    public override void _Ready() {
        _hp = -AfterburnerWeapon.Design.range;
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
        // if (_destroyedAudioStream == null) {
        //     _destroyedAudioStream = GD.Load<AudioStream>("res://audio/rocket_impact.wav");
        // }
        // var sfx = SoundEffectNode.New(_destroyedAudioStream, -5);
        // GetParent().AddChild(sfx);
    }

    private void Explode() {
        // var explosion = DiskExplosion.New();
        // explosion.Position = Position;
        // explosion.Rotation = Rotation;
        // GetParent().AddChild(explosion);
    }
}
