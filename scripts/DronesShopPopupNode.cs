using Godot;
using System;

public class DronesShopPopupNode : PopupNode {
    private static PackedScene _scene = null;
    public static DronesShopPopupNode New() {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/DronesShopPopupNode.tscn");
        }
        var o = (DronesShopPopupNode)_scene.Instance();
        return o;
    }
}
