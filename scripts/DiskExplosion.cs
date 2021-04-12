using Godot;
using System;

public class DiskExplosion : Node2D {
    private static PackedScene _scene = null;
    public static DiskExplosion New() {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/DiskExplosion.tscn");
        }
        var o = (DiskExplosion)_scene.Instance();
        return o;
    }

    public override void _Ready() {
        var anim = GetNode<AnimationPlayer>("AnimationPlayer");
        anim.Play("explosion");
        anim.Connect("animation_finished", this, "OnAnimationFinished");
    }

    private void OnAnimationFinished(string name) {
        QueueFree();
    }
}
