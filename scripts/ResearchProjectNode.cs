using Godot;
using System;

public class ResearchProjectNode : MarginContainer {
    private static PackedScene _scene = null;
    public static ResearchProjectNode New() {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/ResearchProjectNode.tscn");
        }
        var o = (ResearchProjectNode)_scene.Instance();
        return o;
    }

    public override void _Ready() {
    }
}
