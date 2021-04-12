using Godot;
using System;

public class CutterProjectile : Node2D, IProjectile {
    private float _hp;

    private Sprite _sprite;
    private CollisionShape2D _mask = null;

    private static AudioStream _audioStream;

    private Pilot _firedBy;

    public WeaponDesign GetWeaponDesign() { return CutterWeapon.Design; }
    public Pilot FiredBy() { return _firedBy; }

    private static PackedScene _scene = null;
    public static CutterProjectile New(Pilot owner) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/CutterProjectile.tscn");
        }
        var o = (CutterProjectile)_scene.Instance();
        o._firedBy = owner;
        return o;
    }

    public override void _Ready() {
        _hp = HarpoonWeapon.Design.range;
        if (_audioStream == null) {
            _audioStream = GD.Load<AudioStream>("res://audio/weapon/Cutter.wav");
        }

        _sprite = GetNode<Sprite>("Sprite");
        _mask = GetNode<CollisionShape2D>("Area2D/CollisionShape2D");

        GetParent().AddChild(SoundEffectNode.New(_audioStream, -5));
    }

    public override void _Process(float delta) {
        if (_hp < 0) {
            QueueFree();
            return;
        }

        if (_sprite.Scale.x < 1) {
            _sprite.Scale = new Vector2(_sprite.Scale.x + delta, _sprite.Scale.y + delta);
            _mask.Scale = new Vector2(_mask.Scale.x, _mask.Scale.y + delta);
        } else {
            var m = _sprite.Modulate;
            _sprite.Modulate = new Color(m.r, m.g, m.b, m.a - 0.04f);
        }

        float traveled = CutterWeapon.Design.projectileSpeed * delta;
        Position += Transform.x.Normalized() * traveled;
        _hp -= traveled;
    }

    public void OnImpact() {
    }
}
