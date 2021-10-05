using Godot;
using System;

public class CargoMenuPopupNode : PopupDialog {
    private static PackedScene _scene = null;
    public static CargoMenuPopupNode New() {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/ui/CargoMenuPopupNode.tscn");
        }
        var o = (CargoMenuPopupNode)_scene.Instance();
        return o;
    }
}
