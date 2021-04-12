using Godot;
using System;

public class Explosion : Node2D {
    private float _speed;
    private string _name;

    private static PackedScene _scene = null;
    public static Explosion New(float speed = 1.0f, string name = "MissileExplosion") {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/Explosion.tscn");
        }
        var o = (Explosion)_scene.Instance();
        o._speed = speed;
        o._name = name;
        return o;
    }

    public override void _Ready() {
        var anim = GetNode<AnimationPlayer>(_name);
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
