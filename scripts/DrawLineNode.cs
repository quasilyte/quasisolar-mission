using Godot;
using System;

public class DrawLineNode : Node2D {
    private float _lifespan;
    private float _width;
    private Color _color;
    private Node2D _from;
    private Node2D _to;

    private float _drawDelay = 0;

    private static PackedScene _scene = null;
    public static DrawLineNode New(float lifespan, Node2D from, Node2D to, Color color, float width) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/DrawLineNode.tscn");
        }
        var o = (DrawLineNode)_scene.Instance();
        o._lifespan = lifespan;
        o._from = from;
        o._to = to;
        o._color = color;
        o._width = width;
        return o;
    }

    public override void _Ready() {}

    public override void _Process(float delta) {
        _lifespan -= delta;
        if (_lifespan <= 0) {
            QueueFree();
            return;
        }
        if (!IsInstanceValid(_from) || !IsInstanceValid(_to)) {
            QueueFree();
            return;
        }
        _drawDelay -= delta;
        if (_drawDelay <= 0) {
            Update();
            _drawDelay = 0.05f;
        }
    }

    public override void _Draw() {
        if (IsInstanceValid(_from) && IsInstanceValid(_to)) {
            DrawLine(_from.Position, _to.Position, _color, _width);
        }
    }
}
