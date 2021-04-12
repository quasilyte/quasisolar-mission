using Godot;
using System;

public class PlasmaAura : Node2D {
    private float _hp;

    private Sprite _sprite = null;
    private CollisionShape2D _mask = null;

    public Pilot FiredBy;

    private static PackedScene _scene = null;
    public static PlasmaAura New(WeaponDesign design, Pilot owner) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/PlasmaAura.tscn");
        }
        var o = (PlasmaAura)_scene.Instance();
        o.FiredBy = owner;
        return o;
    }

    public override void _Ready() {
        _hp = 1.0f;
        _sprite = GetNode<Sprite>("Sprite");
        _mask = GetNode<CollisionShape2D>("Area2D/CollisionShape2D");
    }

    public override void _Process(float delta) {
        _hp -= delta;
        if (_hp < 0) {
            QueueFree();
            return;
        }

        Rotation -= delta * 3;
        if (_sprite.Scale.x < 1.0) {
            _sprite.Scale = new Vector2(_sprite.Scale.x + delta, _sprite.Scale.y + delta);
            _mask.Scale = new Vector2(_mask.Scale.x + delta, _mask.Scale.y + delta);
        } else {
            var m = _sprite.Modulate;
            _sprite.Modulate = new Color(m.r, m.g, m.b, m.a - 0.07f);
        }
    }
}
