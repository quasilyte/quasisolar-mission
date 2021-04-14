using Godot;
using System;

public class MapBadNotificationNode : MapNotificationNode {
    private static PackedScene _scene = null;
    public static new MapBadNotificationNode New(string text) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/MapBadNotificationNode.tscn");
        }
        var o = (MapBadNotificationNode)_scene.Instance();
        o._text = text;
        return o;
    }
}
