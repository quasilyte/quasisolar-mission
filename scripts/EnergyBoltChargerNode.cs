using Godot;
using System;

public class EnergyBoltChargerNode : Node2D {
    private static PackedScene _scene = null;
    public static EnergyBoltChargerNode New() {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/EnergyBoltChargerNode.tscn");
        }
        var o = (EnergyBoltChargerNode)_scene.Instance();
        return o;
    }
}
