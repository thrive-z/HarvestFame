using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 背包系统
/// </summary>
public class InventorySystem : MonoBehaviour
{
    public GameObject slot_prefab; //插槽预制体
    public GameObject item_prefab; //item预制体

    public static InventorySystem Instance;
    
    public Toolbar toolbar; //工具栏
    public Backpack backpack; //背包
    public StorageBox storageBox; //储物箱
    public Palm palm;//手掌
    public Tip tip; //详情提示


    public InventorySlot focusSlot;//焦点格子
    public InventorySlot beforeSlot;//拖拽前的格子

    private void Awake()
    {
        Instance = this;

        slot_prefab = Resources.Load<GameObject>("Prefab/Slot");
        item_prefab = Resources.Load<GameObject>("Prefab/Item");

        //工具栏初始化
        toolbar = GameObject.Find("Toolbar").GetComponent<Toolbar>();
        toolbar.InitContainer(10);

        //背包初始化
        backpack = GameObject.Find("Backpack").GetComponent<Backpack>();
        backpack.InitContainer(32);

        //储物箱ui初始化
        storageBox = GameObject.Find("StorageBox").GetComponent<StorageBox>();
        storageBox.InitContainer(8);

        //手掌初始化
        palm = GameObject.Find("Palm").GetComponent<Palm>();
        palm.InitContainer(1);

        tip = GameObject.Find("Tip").GetComponent<Tip>();

    }

    //尝试添加物品ByID
    public bool TryAddItemById(int id,int count)
    {
        ItemData itemData = LoadManager.instance.itemDatasDict[id];
        return TryAddItem(itemData, count);
    }


    //尝试添加物品
    public bool TryAddItem(ItemData itemData, int count)
    {
        Debug.Log($"尝试添加物品{itemData.name} {count}个");

        int surplus = count; //剩余要处理的问题

        //1.查找工具栏 是否有同类物品 没有查找背包
        while (surplus > 0)
        {
            // Debug.Log("查找工具栏可用格子");
            //尝试获取一个可用的格子(同类型,且不满)
            InventorySlot slot = FindAvailSlot(toolbar, itemData.id);

            //如果找不到可用的插槽了
            if (slot == null)
            {
                //退出当前循环
                break;
            }

            //找到了一个就可以开始放了
            int overflow = slot.bindItem.itemData.current_stack +
                surplus - slot.bindItem.itemData.max_stack;
            //当前slot上, 已有5个, 要放下4个, 减去 最大值9 = 0
            //当前slot上, 已有5个, 要放下1个, 减去 最大值9 = -3  

            //当前slot上, 已有8个, 要放下3个, 减去 最大值9 = 2  

            if (overflow > 0)
            {
                //能放下的数量 
                int add = itemData.max_stack - slot.bindItem.itemData.current_stack;
                slot.SetItemAmount(itemData.max_stack);

                //扣除已经处理完的
                surplus -= add;


            }
            else
            {
                //能放下的情况: <= 0
                //直接修改库存数量
                slot.SetItemAmount(slot.bindItem.itemData.current_stack + surplus);
                surplus = 0;

                return true;
            }


        }

        //2.查找背包 是否有同类物品 没有查找工具栏空格子
        while (surplus > 0)
        {
            // Debug.Log("查找背包可用格子");
            //尝试获取一个可用的格子(同类型,且不满)
            InventorySlot slot = FindAvailSlot(backpack, itemData.id);

            //如果找不到可用的插槽了
            if (slot == null)
            {
                //退出当前循环
                break;
            }

            //找到了一个就可以开始放了
            int overflow = slot.bindItem.itemData.current_stack +
                surplus - slot.bindItem.itemData.max_stack;
            //当前slot上, 已有5个, 要放下4个, 减去 最大值9 = 0
            //当前slot上, 已有5个, 要放下1个, 减去 最大值9 = -3  

            //当前slot上, 已有8个, 要放下3个, 减去 最大值9 = 2  

            if (overflow > 0)
            {
                //能放下的数量 
                int add = itemData.max_stack - slot.bindItem.itemData.current_stack;
                slot.SetItemAmount(itemData.max_stack);

                //扣除已经处理完的
                surplus -= add;


            }
            else
            {
                //能放下的情况: <= 0
                //直接修改库存数量
                slot.SetItemAmount(slot.bindItem.itemData.current_stack + surplus);
                surplus = 0;

                return true;
            }


        }

        //3.查找工具栏的空格子
        while (surplus > 0)
        {
            // Debug.Log("查找工具栏的空格子");
            //拿到一个空格子
            InventorySlot empty_slot = FindEmptySlot(InventorySystem.Instance.toolbar);
            if (empty_slot == null)
            {
                break;
            }

            //溢出值
            int overflow = surplus - itemData.max_stack;
            // 当前待处理 10 , 这个空格子9 = 1
            // 当前待处理 5 , 这个空格子9 = -4

            if (overflow > 0)
            {
                //有溢出
                empty_slot.AddItem(new ItemData(itemData), itemData.max_stack);
                surplus -= itemData.max_stack; //扣除已处理的数量


            }
            else
            {
                //没有溢出
                empty_slot.AddItem(new ItemData(itemData), surplus);
                surplus = 0;
                return true;
            }


        }

        //4.查找背包的空格子
        while (surplus > 0)
        {
            // Debug.Log("查找背包的空格子");
            //拿到一个空格子
            InventorySlot empty_slot = FindEmptySlot(InventorySystem.Instance.backpack);
            if (empty_slot == null)
            {
                break;
            }

            //溢出值
            int overflow = surplus - itemData.max_stack;
            // 当前待处理 10 , 这个空格子9 = 1
            // 当前待处理 5 , 这个空格子9 = -4

            if (overflow > 0)
            {
                //有溢出
                empty_slot.AddItem(new ItemData(itemData), itemData.max_stack);
                surplus -= itemData.max_stack; //扣除已处理的数量


            }
            else
            {
                //没有溢出
                empty_slot.AddItem(new ItemData(itemData), surplus);
                surplus = 0;
                return true;
            }


        }

        //todo 5.生成在世界中(脚下


        return false;

    }

    //查找空格子
    private InventorySlot FindEmptySlot(InventoryContainer container)
    {
        return container.FindEmptySlot();
    }



    //查找一个可用的格子
    private InventorySlot FindAvailSlot(InventoryContainer container, int id)
    {
        return container.FindAvailSlot(id);
    }

    //判断手上是否拿着东西
    public bool isPicked()
    {
        return palm.slotList[0].bindItem != null;
    }

    //丢掉物品
    public void Discard(ItemData itemData)
    {
        CreateItemInWorld(Player.Instance.transform.position,itemData,itemData.current_stack);
    }

    //生成物品在世界上
    public void CreateItemInWorld(Vector3 pos,ItemData data,int stack)
    {
        //拿到预制体
        var collectItem_prefab = LoadManager.instance.GetPrefab(data.id);

        if (collectItem_prefab == null)
        {
            Debug.Log("没有该预制体");
                return;
        }

        //拿到预制体了，生成
        GameObject colletctGo = Instantiate(collectItem_prefab, pos, Quaternion.identity);
        colletctGo.GetComponent<CollectItem>().SetData(data, stack);


    }


}
