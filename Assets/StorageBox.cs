using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 储物箱UI
/// </summary>
public class StorageBox : InventoryContainer
{
    public static StorageBox Instance;

    
    private void Awake()
    {
        Instance = this;
    }

    //打开储物箱
    internal void OpenBox(List<ItemData> itemDataList)
    {
        SwitchContainer(); //解锁鼠标 并且显示ui

        //打开背包
        InventorySystem.Instance.backpack.SwitchContainer();

        //挂载所有物品

        for (int i = 0; i < itemDataList.Count; i++)
        {
            //有物品需要挂载
            if (itemDataList[i] != null)
            {
                Debug.Log("挂载物品"+ itemDataList[i].name);
                slotList[i].AddItem(itemDataList[i], itemDataList[i].current_stack);
            }
        }
    }

    //关闭储物箱
    public List<ItemData> CloseBox()
    {
        //关闭背包和储物箱ui
        InventorySystem.Instance.backpack.SwitchContainer();
        SwitchContainer(); //解锁鼠标 并且显示ui

        //返回新的列表
        List<ItemData> list = GetAllItemDataToList();

        //销毁物品ui
        ClearAllItem(); //销毁所有物品对象

        
        return list;
    }

    
}
