using Godot;
using System;

public class ItemSlotNode : Node2D {
    [Signal]
    public delegate void ItemApplied(ItemSlotNode fromSlot, DraggableItemNode item);

    private DraggableItemNode _itemNode = null;
    private bool _enabled = false;
    private int _index;
    private ItemKind _itemKind;

    private Vessel _vessel;

    private Func<int, DraggableItemNode, bool> _assignItem;

    private static PackedScene _scene = null;
    public static ItemSlotNode New(int index, ItemKind itemKind) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/ItemSlotNode.tscn");
        }
        var o = (ItemSlotNode)_scene.Instance();
        o._index = index;
        o._itemKind = itemKind;
        return o;
    }

    public void SetAssignItemCallback(Func<int, DraggableItemNode, bool> f) {
        _assignItem = f;
    }

    public void Reset(Vessel v, bool enabled) {
        _enabled = enabled;
        _vessel = v;
        if (_itemNode != null) {
            _itemNode.QueueFree();
            _itemNode = null;
        }
    }

    public void MakeEmpty() {
        if (_assignItem != null) {
            _assignItem(_index, null);
        }
        _itemNode = null;
    }

    public bool ApplyItem(ItemSlotNode fromSlot, DraggableItemNode itemNode) {
        if (_itemNode != null || !_enabled) {
            return false;
        }
        switch (_itemKind) {
            case ItemKind.Sell:
            case ItemKind.Storage:
                break; // OK
            case ItemKind.GarrisonVessel:
                if (itemNode.item.GetItemKind() != ItemKind.Vessel) {
                    return false;
                }
                break;
            case ItemKind.Shield:
                if (itemNode.item.GetItemKind() != ItemKind.Shield) {
                    return false;
                }
                if (_vessel != null && _vessel.Design().maxShieldLevel < ((ShieldDesign)itemNode.item).level) {
                    return false;
                }
                break;
            default:
                if (_itemKind != itemNode.item.GetItemKind()) {
                    return false;
                }
                break;
        }

        if (_assignItem != null) {
            _assignItem(_index, itemNode);
        }

        _itemNode = itemNode;
        if (fromSlot != null) {
            fromSlot.MakeEmpty();
        }
        itemNode.GlobalPosition = GlobalPosition;
        EmitSignal(nameof(ItemApplied), new object[]{fromSlot, itemNode});
        return true;
    }
}
