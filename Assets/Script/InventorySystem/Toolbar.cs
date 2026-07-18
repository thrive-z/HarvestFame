using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 工具栏
/// </summary>
public class Toolbar : InventoryContainer
{

    public int selectedSlot = 0; //当前选中的插槽索引
    public GameObject avtiveSlot_prefab; //当前选中的方框

    public List<KeyCode>KeyList = new List<KeyCode>(); //按键列表

    private void Awake()
    {
        KeyList.Add(KeyCode.Alpha1);
        KeyList.Add(KeyCode.Alpha2);
        KeyList.Add(KeyCode.Alpha3);
        KeyList.Add(KeyCode.Alpha4);
        KeyList.Add(KeyCode.Alpha5);
        KeyList.Add(KeyCode.Alpha6);
        KeyList.Add(KeyCode.Alpha7);
        KeyList.Add(KeyCode.Alpha8);
        KeyList.Add(KeyCode.Alpha9);
        KeyList.Add(KeyCode.Alpha0);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < KeyList.Count; i++)
        {
            if (Input.GetKeyDown(KeyList[i]))
            {
                SwitchSlot(i);
            }
        }
    }

    private void SwitchSlot(int i)
    {
        Debug.Log("选中了插槽索引" + i);

        selectedSlot = i; //记录激活的插槽

        //修改激活框的位置
        avtiveSlot_prefab.transform.position = slotList[selectedSlot].transform.position;

        //切换item 通知模块
        var item = slotList[selectedSlot].bindItem;
        if ( item == null)
        {
            SwitchItem(null);
        }
        else
        {
            SwitchItem(item.itemData);
        }
    }

    //切换物品
    private void SwitchItem(ItemData itemData)
    {
        //事件中心 发布事件
        EventBus.Publish(new ToolbarItemChangedEvent(itemData));

        ////工具系统
        //AgricultureSystem.Instance.toolModule.ChangeTool(itemData);
        ////武器系统
        //WeaponSystem.Instance.ChangeWeapon(itemData);

    }
}
