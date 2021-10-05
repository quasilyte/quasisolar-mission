using Godot;
using System;

public class ButtonNode : Button {
    private static PackedScene _scene = null;
    public static ButtonNode New() {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/ui/ButtonNode.tscn");
        }
        var o = (ButtonNode)_scene.Instance();
        return o;
    }

    public override void _Ready() {

    }
}
