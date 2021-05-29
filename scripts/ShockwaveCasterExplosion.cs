using Godot;
using System;

public class ShockwaveCasterExplosion : Node2D {
    private static PackedScene _scene = null;
    public static ShockwaveCasterExplosion New() {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/ShockwaveCasterExplosion.tscn");
        }
        var o = (ShockwaveCasterExplosion)_scene.Instance();
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
