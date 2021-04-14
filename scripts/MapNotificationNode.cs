using Godot;
using System;

public class MapNotificationNode : Node2D {
    protected string _text;

    private static PackedScene _scene = null;
    public static MapNotificationNode New(string text) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/MapNotificationNode.tscn");
        }
        var o = (MapNotificationNode)_scene.Instance();
        o._text = text;
        return o;
    }

    public override void _Ready() {
        GetNode<Label>("Panel/Text").Text = _text;

        var timer = GetNode<Timer>("Timer");
        timer.Connect("timeout", this, nameof(OnTimeout));
        timer.WaitTime = 1.2f;
        timer.Start();
    }

    public override void _Process(float delta) {
        GetNode<Sprite>("Sprite").Rotation += delta * 2;
    }

    private void OnTimeout() {
        QueueFree();
    }
}
