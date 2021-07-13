using Godot;
using System;

public class PlanetsMenuPopupNode : PopupNode {
    private static PackedScene _scene = null;
    public static PlanetsMenuPopupNode New() {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/PlanetsMenuPopupNode.tscn");
        }
        var o = (PlanetsMenuPopupNode)_scene.Instance();
        return o;
    }

    public override void _Ready() {
        GetNode<AnimatedPlanetNode>("PlanetModel").Visible = false;
    }
}
