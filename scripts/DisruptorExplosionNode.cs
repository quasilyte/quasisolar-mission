using Godot;
using System;

public class DisruptorExplosionNode : Sprite {
    private static PackedScene _scene = null;
    public static DisruptorExplosionNode New() {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/DisruptorExplosionNode.tscn");
        }
        var o = (DisruptorExplosionNode)_scene.Instance();
        return o;
    }

    public override void _Ready() {
    }

    public override void _Process(float delta) {
        if (Scale.x < 0.2f) {
            QueueFree();
            return;
        }

        Rotation += delta * 4;
        Scale = new Vector2(Scale.x - delta, Scale.y - delta);
    }
}
