using Godot;
using System;

public class VesselUpgradeNode : MarginContainer {
    private static PackedScene _scene = null;
    public static VesselUpgradeNode New() {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/VesselUpgradeNode.tscn");
        }
        var o = (VesselUpgradeNode)_scene.Instance();
        return o;
    }

    public override void _Ready() {
    }
}
