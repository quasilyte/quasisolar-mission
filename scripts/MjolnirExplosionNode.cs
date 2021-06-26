using Godot;
using System;

public class MjolnirExplosionNode : Node2D {
    private static PackedScene _scene = null;
    public static MjolnirExplosionNode New() {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/MjolnirExplosionNode.tscn");
        }
        var o = (MjolnirExplosionNode)_scene.Instance();
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
