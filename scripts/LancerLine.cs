using Godot;
using System;

public class LancerLine : Line2D {
    private bool _fading = false;

    private static PackedScene _scene = null;
    public static LancerLine New() {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/LancerLine.tscn");
        }
        var o = (LancerLine)_scene.Instance();
        return o;
    }

    public override void _Process(float delta) {
        if (Modulate.a <= 0) {
            QueueFree();
            return;
        }

        if (_fading) {
            var m = Modulate;
            m.a -= 0.06f;
            Modulate = m;
        }

        var newPos = Points[0].MoveToward(Points[1], 200 * delta);
        SetPointPosition(0, newPos);
    }

    public void StartFading() {
        _fading = true;
    }
}
