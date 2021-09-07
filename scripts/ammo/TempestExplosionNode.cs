using Godot;
using System;

public class TempestExplosionNode : Node2D {
    private static PackedScene _scene = null;
    public static TempestExplosionNode New() {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/ammo/TempestExplosionNode.tscn");
        }
        var o = (TempestExplosionNode)_scene.Instance();
        return o;
    }

    public override void _Ready() {
        GetNode<CPUParticles2D>("CPUParticles2D").Emitting = true;
        GetTree().CreateTimer(0.2f).Connect("timeout", this, nameof(OnTimeout));
    }

    private void OnTimeout() {
        QueueFree();
    }
}
