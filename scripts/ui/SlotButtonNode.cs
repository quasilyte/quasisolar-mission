using Godot;
using System;

public class SlotButtonNode : TextureButton {
    private static Texture selectedTexture = null;
    private static Texture normalTexture = null;

    private static PackedScene _scene = null;
    public static SlotButtonNode New() {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/SlotButtonNode.tscn");
        }
        var o = (SlotButtonNode)_scene.Instance();
        return o;
    }

    public override void _Ready() {
        if (selectedTexture == null) {
            selectedTexture = GD.Load<Texture>("res://images/ui/item_slot2_selected.png");
            normalTexture = GD.Load<Texture>("res://images/ui/item_slot2_normal.png");
        }
    }

    public void SetSelected(bool selected) {
        if (selected) {
            TextureNormal = selectedTexture;
        } else {
            TextureNormal = normalTexture;
        }
    }
}
