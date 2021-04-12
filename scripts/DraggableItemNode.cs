using Godot;
using System;

public class DraggableItemNode : Node2D {
    private static DraggableItemNode _dragging = null;
    private static ItemSlotNode _fallbackSlot = null;

    public IItem item;
    private ItemSlotNode _slot;

    private static PackedScene _scene = null;
    public static DraggableItemNode New(ItemSlotNode slot, IItem item) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/DraggableItemNode.tscn");
        }
        var o = (DraggableItemNode)_scene.Instance();
        o._slot = slot;
        o.item = item;
        return o;
    }

    public override void _Ready() {
        var sprite = GetNode<Sprite>("Sprite");
        sprite.Texture = ItemInfo.Texture(item);

        var area = GetNode<Area2D>("Area2D");
        area.Connect("area_entered", this, nameof(OnAreaEntered));
        area.Connect("area_exited", this, nameof(OnAreaExited));
    }

    public ItemSlotNode GetSlotNode() { return _slot; }

    public override void _Process(float delta) {
        if (_dragging == this) {
            GlobalPosition =  GetGlobalMousePosition();
        }

        if (_dragging == null && Input.IsActionJustPressed("leftMouseButton")) {
            var clickPos = GetGlobalMousePosition();
            var rect = new Rect2(GlobalPosition.x - 32, GlobalPosition.y - 32, 64, 64);
            if (rect.HasPoint(clickPos)) {
                _dragging = this;
                ZIndex++;
                var parent = _slot.GetParent();
                _fallbackSlot = _slot;
                _slot = null;
            }
        }

        if (_dragging == this && Input.IsActionJustReleased("leftMouseButton")) {
            _dragging = null;
            ZIndex--;
            if (_slot == null || !_slot.ApplyItem(_fallbackSlot, this)) {
                GlobalPosition = _fallbackSlot.GlobalPosition;
                _slot = _fallbackSlot;
            }
        }
    }

    private void OnAreaEntered(Area2D other) {
        if (other.GetParent() is ItemSlotNode slot) {
            _slot = slot;
        }
    }

    private void OnAreaExited(Area2D other) {
        if (other.GetParent() is ItemSlotNode slot) {
            if (slot == _slot) {
                _slot = null;
            }
        }
    }
}
