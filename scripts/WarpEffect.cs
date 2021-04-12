using Godot;
using System;

public class WarpEffect : Node2D {
    private static PackedScene _scene = null;
    public static WarpEffect New() {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/WarpEffect.tscn");
        }
        var o = (WarpEffect)_scene.Instance();
        return o;
    }

    public override void _Ready() {
        var anim = GetNode<AnimationPlayer>("AnimationPlayer");
        anim.Play("effect");
        anim.Connect("animation_finished", this, "OnAnimationFinished");
    }

    private void OnAnimationFinished(string name) {
        QueueFree();
    }
}
