using System.Collections.Generic;
using UnityEngine;

public class StorageBox : InventoryContainer
{
    public static StorageBox Instance;
    private BoxEntity activeBox;

    private void Awake()
    {
        Instance = this;
        CloseContainer();
    }

    internal bool OpenBox(BoxEntity box)
    {
        if (activeBox != null)
            return false;

        activeBox = box;
        ClearAllItem();
        for (var i = 0; i < Mathf.Min(box.itemDataList.Count, slotList.Count); i++)
        {
            var item = box.itemDataList[i];
            if (item != null)
                slotList[i].AddItem(item, item.current_stack);
        }

        InventorySystem.Instance.backpack.OpenContainer();
        OpenContainerAndCursor();
        return true;
    }

    public void CloseActiveBox()
    {
        if (activeBox == null)
            return;

        var inventory = InventorySystem.Instance;
        var held = inventory.palm.slotList[0];
        if (inventory.beforeSlot != null && held.bindItem != null)
        {
            inventory.beforeSlot.BindItem(held.bindItem);
            held.bindItem = null;
        }
        inventory.beforeSlot = null;
        inventory.focusSlot = null;
        inventory.tip.CloseContainer();

        activeBox.itemDataList = GetAllItemDataToList();
        activeBox.isOpen = false;
        activeBox = null;
        ClearAllItem();
        inventory.backpack.CloseContainer();
        CloseContainerAndCursor();
    }
}
