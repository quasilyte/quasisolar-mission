using Godot;
using System;

public class PullingBeamerEffectNode : Node2D {
    private float _hp = 0.2f;
    private Vector2 _from;
    private Vector2 _to;

    private static PackedScene _scene = null;
    public static PullingBeamerEffectNode New(Vector2 from, Vector2 to) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/ammo/PullingBeamerEffectNode.tscn");
        }
        var o = (PullingBeamerEffectNode)_scene.Instance();
        o._from = from;
        o._to = to;
        return o;
    }

    public override void _Ready() {
        var particles = GetNode<CPUParticles2D>("Particles");
        particles.Position = (_from + _to) / 2;
        particles.Rotation = _to.AngleToPoint(_from);
        particles.EmissionRectExtents = new Vector2(_to.DistanceTo(_from) / 2, particles.EmissionRectExtents.y);
    }

    public override void _Process(float delta) {
        _hp -= delta;
        if (_hp <= 0) {
            QueueFree();
        }
    }
}
