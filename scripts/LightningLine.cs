using Godot;
using System;

public class LightningLine : Line2D {
    private float _removeDelay;
    private bool _fading = false;

    private static PackedScene _scene = null;
    public static LightningLine New() {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/LightningLine.tscn");
        }
        var o = (LightningLine)_scene.Instance();
        return o;
    }

    public override void _Ready() {
        _removeDelay = 0.2f;
    }

    public override void _Process(float delta) {
        var numPoints = GetPointCount();
        if (numPoints == 0 || Modulate.a <= 0) {
            QueueFree();
            return;
        }

        if (_fading) {
            var m = Modulate;
            Modulate = new Color(m.r, m.g, m.b, m.a - 0.03f);
        }

        if (numPoints > 1) {
            var p0 = GetPointPosition(0);
            var p1 = GetPointPosition(1);
            SetPointPosition(0, p0.MoveToward(p1, 120 * delta));
        }

        _removeDelay -= delta;
        if (_removeDelay <= 0) {
            _removeDelay = 0.2f;
            RemovePoint(0);
        }
    }

    public void StartFading() {
        _fading = true;
    }
}
