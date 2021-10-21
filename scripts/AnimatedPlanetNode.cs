using Godot;
using System;

public class AnimatedPlanetNode : Node2D {
    private static PackedScene _scene = null;
    public static AnimatedPlanetNode New() {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/AnimatedPlanetNode.tscn");
        }
        var o = (AnimatedPlanetNode)_scene.Instance();
        return o;
    }

    public override void _Ready() {
    }

    public void SetSprite(string name) {
        GetNode<Sprite>("Sprite").Texture = GD.Load<Texture>("res://images/planet/" + name + ".jpg");
    }
}
