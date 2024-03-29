using Godot;
using System;

public class Beam : Node2D {
    private Vector2 _from;
    private Vector2 _to;
    private Color _color;
    private float _width;
    private float _hp = 0.05f;

    public VesselNode target;
    public float damage;
    public DamageKind damageKind;

    private static PackedScene _scene = null;
    public static Beam New(Vector2 from, Vector2 to, Color color, float width) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/Beam.tscn");
        }
        var o = (Beam)_scene.Instance();
        o._from = from;
        o._to = to;
        o._color = color;
        o._width = width;
        return o;
    }

    public override void _Ready() {
        if (target != null) {
            target.ApplyDamage(damage, damageKind);
        }
    }

    public override void _Process(float delta) {
        _hp -= delta;
        if (_hp <= 0) {
            QueueFree();
        }
    }

    public override void _Draw() {
        DrawLine(_from, _to, _color, _width);
    }
}
