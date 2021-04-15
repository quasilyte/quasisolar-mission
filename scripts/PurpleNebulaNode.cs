using Godot;
using System;

public class PurpleNebulaNode : EnvHazardNode {
    private static PackedScene _scene = null;
    public static PurpleNebulaNode New() {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/PurpleNebulaNode.tscn");
        }
        var o = (PurpleNebulaNode)_scene.Instance();
        return o;
    }

    public override void _Ready() {
        base._Ready();
        RandomizeScaling();
    }
}
