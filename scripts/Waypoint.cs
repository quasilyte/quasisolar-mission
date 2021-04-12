using Godot;
using System;

public class Waypoint : Node2D {
    public Area2D Area;

    public bool visible;
    private Color? _color;

    private Vector2 _pos;

    private static PackedScene _scene = null;
    public static Waypoint New(bool visible, Color? color) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/Waypoint.tscn");
        }
        var o = (Waypoint)_scene.Instance();
        o.visible = visible;
        o._color = color;
        return o;
    }

    public override void _Ready() {
        Area = GetNode<Area2D>("Area2D");
        var sprite = GetNode<Sprite>("Sprite");
        sprite.Visible = visible;
        if (_color != null) {
            sprite.Modulate = _color.Value;
        }
    }

    // public override void _Process(float delta) {
    //     if (_camera != null) {
    //         var offset = _camera.GetCameraScreenCenter() - _camera.GetViewportRect().Size / 2; 
    //         Position = _pos - offset;
    //     }
    // }
}
