using Godot;
using System;

public class ContrailNode : CPUParticles2D {
    private static PackedScene _scene = null;
    public static ContrailNode New() {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/ContrailNode.tscn");
        }
        var o = (ContrailNode)_scene.Instance();
        return o;
    }
}
