using Godot;
using System;

public class ListItemNode : GridContainer {
    private string _text;

    private static PackedScene _scene = null;
    public static ListItemNode New(string text) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/ui/ListItemNode.tscn");
        }
        var o = (ListItemNode)_scene.Instance();
        o._text = text;
        return o;
    }

    public override void _Ready() {
        GetNode<Label>("Label").Text = _text;
    }
}
