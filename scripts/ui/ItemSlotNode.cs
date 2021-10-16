using Godot;
using System;

public class ItemSlotNode : Control {
    [Export] public ItemKind presetItemKind = ItemKind.Unset;
    [Export] public int presetIndex = 0;

    [Signal]
    public delegate void Clicked();

    private static Texture selectedTexture = null;
    private static Texture normalTexture = null;
    private static Texture highlightedTexture = null;

    private bool _enabled = false;
    private int _index;
    private ItemKind _itemKind; // Item kinds that this slot can accept
    private IItem _item; // Item that is currently assigned to the slot

    private Sprite _itemSprite;
    private TextureButton _toggle;

    private Vector2 _buttonDownPos;

    private Vessel _vessel;
    private Action<int, IItem> _assignItem;

    private static PackedScene _scene = null;
    public static ItemSlotNode New(int index, ItemKind itemKind) {
        if (_scene == null) {
            _scene = GD.Load<PackedScene>("res://scenes/ui/ItemSlotNode.tscn");
        }
        var o = (ItemSlotNode)_scene.Instance();
        o._index = index;
        o._itemKind = itemKind;
        return o;
    }

    public override void _Ready() {
        if (normalTexture == null) {
            selectedTexture = GD.Load<Texture>("res://images/ui/item_slot_selected.png");
            normalTexture = GD.Load<Texture>("res://images/ui/item_slot_normal.png");
            highlightedTexture = GD.Load<Texture>("res://images/ui/item_slot_highlighted.png");
        }
        if (presetItemKind != ItemKind.Unset) {
            _itemKind = presetItemKind;
            _index = presetIndex;
        }

        _itemSprite = GetNode<Sprite>("ItemSprite");
        _itemSprite.Visible = false;
        _toggle = GetNode<TextureButton>("Toggle");

        _toggle.Connect("button_down", this, nameof(OnButtonDown));
        _toggle.Connect("button_up", this, nameof(OnButtonUp));
    }

    public bool IsEmpty() { return _item == null; }
    public bool IsSelected() { return _toggle.TextureNormal == selectedTexture; }

    public void MakeSelected() { _toggle.TextureNormal = selectedTexture; }
    public void MakeUnselected() { _toggle.TextureNormal = normalTexture; }
    public void MakeHighlighted() { _toggle.TextureNormal = highlightedTexture;}

    public IItem GetItem() { return _item; }
    public ItemKind GetItemKind() { return _itemKind; }

    public void SetAssignItemCallback(Action<int, IItem> f) {
        _assignItem = f;
    }

    public void Reset(Vessel v, bool enabled) {
        _enabled = enabled;
        _vessel = v;
        if (_item != null) {
            _itemSprite.Visible = false;
            _item = null;
        }
        _toggle.Disabled = !_enabled;
        MakeUnselected();
    }

    public void MakeEmpty() {
        if (_assignItem != null) {
            _assignItem(_index, null);
        }
        _itemSprite.Visible = false;
        _item = null;
    }

    public bool CanApplyItem(IItem item) {
        if (_item != null || !_enabled) {
            return false;
        }

        switch (_itemKind) {
            case ItemKind.Shop:
                return false;
            case ItemKind.Storage:
                break; // OK
            case ItemKind.GarrisonVessel:
                if (item.GetItemKind() != ItemKind.Vessel) {
                    return false;
                }
                break;
            case ItemKind.Shield:
                if (item.GetItemKind() != ItemKind.Shield) {
                    return false;
                }
                if (_vessel != null && _vessel.Design().maxShieldLevel < ((ShieldDesign)item).level) {
                    return false;
                }
                break;
            default:
                if (_itemKind != item.GetItemKind()) {
                    return false;
                }
                break;
        }
        return true;
    }

    public bool ApplyItem(IItem item) {
        if (!CanApplyItem(item)) {
            return false;
        }

        if (_assignItem != null) {
            _assignItem(_index, item);
        }

        AssignItem(item);
        return true;
    }

    public bool ApplyItem(ItemSlotNode fromSlot) {
        if (ApplyItem(fromSlot._item)) {
            fromSlot.MakeEmpty();
            return true;
        }
        return false;
    }

    public void AssignItem(IItem item) {
        _item = item;
        _itemSprite.Texture = ItemInfo.Texture(item);
        _itemSprite.Visible = true;
    }

    public void OnButtonDown() {
        _buttonDownPos = GetGlobalMousePosition();
    }

    public void OnButtonUp() {
        if (GetGlobalMousePosition().DistanceTo(_buttonDownPos) <= 32) {
            EmitSignal(nameof(Clicked));
        }
    }
}
