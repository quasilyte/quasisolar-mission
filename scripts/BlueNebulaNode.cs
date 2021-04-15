using Godot;
using System;

public class BlueNebulaNode : EnvHazardNode {
    private static PackedScene _scene = null;
    public static BlueNebulaNode New() {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/BlueNebulaNode.tscn");
        }
        var o = (BlueNebulaNode)_scene.Instance();
        return o;
    }

    public override void _Ready() {
        base._Ready();
        RandomizeScaling();
    }
}
