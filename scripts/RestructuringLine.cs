using Godot;
using System;

public class RestructuringLine : Line2D {
    private bool _fading = false;
    private Node2D _from;
    private Node2D _to;

    private static PackedScene _scene = null;
    public static RestructuringLine New(Node2D from, Node2D to) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/RestructuringLine.tscn");
        }
        var o = (RestructuringLine)_scene.Instance();
        o._from = from;
        o._to = to;
        return o;
    }

    public override void _Ready() {
        base._Ready();
        AddPoint(_from.GlobalPosition);
        AddPoint(_to.GlobalPosition);
    }

    public override void _Process(float delta) {
        if (Modulate.a <= 0) {
            QueueFree();
            return;
        }
        if (!IsInstanceValid(_from) || !IsInstanceValid(_to)) {
            QueueFree();
            return;
        }

        SetPointPosition(0, _from.GlobalPosition);
        SetPointPosition(1, _to.GlobalPosition);

        if (_fading) {
            var m = Modulate;
            Modulate = new Color(m.r, m.g, m.b, m.a - 0.06f);
        }
    }

    public void StartFading() {
        _fading = true;
    }
}
