using Godot;
using System;

public class SlotButtonNode : TextureButton {
    private static PackedScene _scene = null;
    public static SlotButtonNode New() {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/SlotButtonNode.tscn");
        }
        var o = (SlotButtonNode)_scene.Instance();
        return o;
    }

    public override void _Ready() {
        GetNode<Sprite>("Outline").Visible = false;
    }

    public void SetOutlineVisibility(bool visible) {
        GetNode<Sprite>("Outline").Visible = visible;
    }
}
