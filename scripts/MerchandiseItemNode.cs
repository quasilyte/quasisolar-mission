using Godot;
using System;

public class MerchandiseItemNode : Node2D {
    [Signal]
    public delegate void Clicked();

    public IItem item;

    public override void _Ready() {
        var sprite = GetNode<Sprite>("Sprite");
        sprite.Texture = ItemInfo.Texture(item);
    }

    private static PackedScene _scene = null;
    public static MerchandiseItemNode New(IItem item) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/MerchandiseItemNode.tscn");
        }
        var o = (MerchandiseItemNode)_scene.Instance();
        o.item = item;
        return o;
    }

    public override void _Input(InputEvent e) {
        if (e is InputEventMouseButton mouseEvent) {
            if (mouseEvent.ButtonIndex == (int)ButtonList.Left && mouseEvent.Pressed) {
                var clickPos = GetGlobalMousePosition();
                var rect = new Rect2(GlobalPosition.x - 32, GlobalPosition.y - 32, 64, 64);
                if (rect.HasPoint(clickPos)) {
                    EmitSignal(nameof(Clicked));
                    GetTree().SetInputAsHandled();
                }
            }
        }
    }
}
