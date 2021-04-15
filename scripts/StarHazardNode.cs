using Godot;
using System;

public class StarHazardNode : EnvHazardNode {
    private StarColor _color;

    private static PackedScene _scene = null;
    public static StarHazardNode New(StarColor color) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/StarHazardNode.tscn");
        }
        var o = (StarHazardNode)_scene.Instance();
        o._color = color;
        return o;
    }

    public override void _Ready() {
        base._Ready();

        if (_color == StarColor.Orange || _color == StarColor.Yellow || _color == StarColor.Red) {
            GetNode<Sprite>("Sprite").Texture = GD.Load<Texture>("res://images/arena/yellow_star.png");
        } else if (_color == StarColor.Blue || _color == StarColor.White) {
            GetNode<Sprite>("Sprite").Texture = GD.Load<Texture>("res://images/arena/blue_star.png");
        } else if (_color == StarColor.Green) {
            GetNode<Sprite>("Sprite").Texture = GD.Load<Texture>("res://images/arena/green_star.png");
        }
    }
}
