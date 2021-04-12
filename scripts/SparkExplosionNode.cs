using Godot;
using System;

public class SparkExplosionNode : Node2D {
    private float _speed;

    private static PackedScene _scene = null;
    public static SparkExplosionNode New(float speed = 1.0f) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/SparkExplosionNode.tscn");
        }
        var o = (SparkExplosionNode)_scene.Instance();
        o._speed = speed;
        return o;
    }

    public override void _Ready() {
        var anim = GetNode<AnimationPlayer>("Explosion");
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
