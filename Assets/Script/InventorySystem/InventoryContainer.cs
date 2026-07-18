using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 容器父类
/// </summary>
public class InventoryContainer : UIContainer
{
    public Transform _layout; //布局 SlotLayout

    public List<InventorySlot> slotList; //插槽列表

    //初始化布局容器, 自动生成插槽
    public virtual List<InventorySlot> InitContainer(int slotCount)
    {


        _layout = transform.Find("SlotLayout").transform;

        //生成插槽
        for (int i = 0; i < slotCount; i++)
        {
            GameObject go = Instantiate(InventorySystem.Instance.slot_prefab, _layout);

            //slot脚本
            InventorySlot slot = go.AddComponent<InventorySlot>();
            slotList.Add(slot); //记录slot
            slot.SetContainer(this);
        }


        return slotList;
    }

    //查找一个可用的格子
    internal InventorySlot FindAvailSlot(int id)
    {
        //遍历插槽
        foreach (var slot in slotList)
        {
            //如果有绑定的物品
            if (slot.bindItem != null && //是否有物品
                slot.bindItem.itemData.id == id && //是否有插槽
                slot.bindItem.itemData.current_stack < slot.bindItem.itemData.max_stack //是否有剩余
                )
            {
                //当前找到了可用插槽
                return slot;
            }
        }

        //一个都没找到
        return null;
    }

    //在当前容器内查找一个空格子
    public InventorySlot FindEmptySlot()
    {
        //遍历插槽
        foreach (var slot in slotList)
        {
            //如果有绑定的物品
            if (slot.bindItem == null) //是否有物品
            {
                //当前找到了可用的插槽
                return slot;
            }
        }

        //一个都没找到
        return null;
    }

    //获取所有插槽中的物品数据 返回为列表
    public List<ItemData> GetAllItemDataToList()
    {
        List<ItemData> list = new List<ItemData>();

        foreach (InventorySlot slot in slotList)
        {
            if (slot.bindItem != null)
            {
                list.Add(new ItemData(slot.bindItem.itemData));
            }
            else
            {
                list.Add(null);
            }
        }

        return list;
    }

    //销毁所有物品
    public void ClearAllItem()
    {
        foreach (InventorySlot slot in slotList)
        {
            if (slot.bindItem != null)
            {
                Destroy(slot.bindItem.gameObject);
                slot.bindItem = null;
            }
            
        }
    }

}

