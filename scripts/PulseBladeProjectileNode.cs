using Godot;
using System;

public class PulseBladeProjectileNode : Line2D {
    public Vector2 from;
    public Vector2 to;

    private CPUParticles2D _particles;

    private static PackedScene _scene = null;
    public static PulseBladeProjectileNode New() {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/PulseBladeProjectileNode.tscn");
        }
        var o = (PulseBladeProjectileNode)_scene.Instance();
        return o;
    }

    public override void _Ready() {
        _particles = GetNode<CPUParticles2D>("Particles");
        Adjust();
    }

    public override void _Process(float delta) {
        Adjust();
    }

    private void Adjust() {
        _particles.Position = (to + from) / 2;
        _particles.Rotation = from.AngleToPoint(to);
        _particles.EmissionRectExtents = new Vector2(from.DistanceTo(to) / 2, _particles.EmissionRectExtents.y);
        SetPointPosition(0, from);
        SetPointPosition(1, to);
    }
}
