using Godot;
using System;

public class ZapProjectileNode : Line2D {
    private Vector2 _from;
    private Vector2 _to;
    private float _hp = 0.05f;

    private static PackedScene _scene = null;
    public static ZapProjectileNode New(Vector2 from, Vector2 to) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/ZapProjectileNode.tscn");
        }
        var o = (ZapProjectileNode)_scene.Instance();
        o._from = from;
        o._to = to;
        return o;
    }

    public override void _Ready() {
        SetPointPosition(0, _from);
        SetPointPosition(1, _to);
    }

    public override void _Process(float delta) {
        _hp -= delta;
        if (_hp <= 0) {
            QueueFree();
        }
    }
}
