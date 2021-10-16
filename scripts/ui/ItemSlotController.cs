using System.Collections.Generic;

public class ItemSlotController {
    public ItemSlotNode selected = null;

    public List<ItemSlotNode> slots = new List<ItemSlotNode>();

    public ItemSlotNode NewSlot(int index, ItemKind itemKind) {
        var item = ItemSlotNode.New(index, itemKind);
        slots.Add(item);
        return item;
    }

    public void AddSlot(ItemSlotNode slot) {
        slots.Add(slot);
    }

    public void Unselect() {
        foreach (var slot in slots) {
            slot.MakeUnselected();
        }
        selected = null;
    }

    public void ClearSelectedSlot() {
        selected.MakeEmpty();
        Unselect();
    }

    public void OnItemClicked(ItemSlotNode itemSlot) {
        if (selected == null && itemSlot.IsEmpty()) {
            // Clicking an empty slot without another slot selected is a no-op.
        } else if (selected == itemSlot) {
            // Clicked on the same item again.
            itemSlot.MakeUnselected();
            selected = null;
        } else if (selected == null) {
            // Nothing is selected, so any item slot click selects it.
            itemSlot.MakeSelected();
            selected = itemSlot;
        } else if (selected != null && !itemSlot.IsEmpty()) {
            // Pressing the other item while having an item selected selects a new item.
            selected.MakeUnselected();
            itemSlot.MakeSelected();
            selected = itemSlot;
        } else if (selected != null && itemSlot.IsEmpty()) {
            // Pressing an empty slot transfers an item to a new slot.
            if (selected.GetItemKind() != ItemKind.Shop && itemSlot.ApplyItem(selected)) {
                selected.MakeUnselected();
                selected = null;
            }
        }

        if (selected != null) {
            if (selected.GetItemKind() != ItemKind.Shop) {
                foreach (var slot in slots) {
                    if (slot == selected) {
                        continue;
                    }
                    if (slot.CanApplyItem(selected.GetItem())) {
                        slot.MakeHighlighted();
                    }
                }
            }
        } else {
            foreach (var slot in slots) {
                slot.MakeUnselected();
            }
        }
    }
}
