using Godot;
using System;

public class FireEffectNode : Node2D {
    private float _speed;

    private static PackedScene _scene = null;
    public static FireEffectNode New(float speed = 1.0f) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/FireEffectNode.tscn");
        }
        var o = (FireEffectNode)_scene.Instance();
        o._speed = speed;
        return o;
    }

    public override void _Ready() {
        var anim = GetNode<AnimationPlayer>("Animation");
        if (_speed != 1.0f) {
            anim.PlaybackSpeed = _speed;
        }
        anim.Play("explosion");
        anim.Connect("animation_finished", this, "OnAnimationFinished");
    }

    private void OnAnimationFinished(string name) {
        QueueFree();
    }
}
