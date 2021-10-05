public class ItemSlotController {
    public ItemSlotNode selected = null;

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
            if (itemSlot.ApplyItem(selected)) {
                selected.MakeUnselected();
                selected = null;
            }
        }
    }
}
