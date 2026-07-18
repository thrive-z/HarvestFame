using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 物品脚本
/// </summary>
public class InventoryItem : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public ItemData itemData; //模型数据

   
    public Image _icon; //icon
    public TMP_Text _text;  //text


    public InventorySlot bindSlot; //绑定的slot

    private void Awake()
    {
        _icon = transform.Find("Icon").GetComponent<Image>();
        _text = transform.Find("Text").GetComponent<TMP_Text>();
    }



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //设置数量
    public void SetItemAmount(int count)
    {
        //如果数量为0，直接销毁
        if (count == 0)
        {
            bindSlot.bindItem = null;

            Destroy(gameObject);
            return;
        }

        itemData.current_stack = count;

        //更新ui
        _text.text = itemData.current_stack.ToString();

    }

    //设置数据
    public void SetItem(ItemData itemData, int count)
    {
       this.itemData = itemData;
        SetItemAmount(count);//设置数量和ui
        _icon.sprite=itemData.icon;//设置图片ui

    }

    public void OnDrag(PointerEventData eventData)
    {
        
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("开始拖拽");

        //记录当前拖拽的格子
        InventorySystem.Instance.beforeSlot = this.bindSlot;

        //手
        InventorySlot palmSlot =InventorySystem.Instance.palm.slotList[0];

        palmSlot.PickUpItem(this);

       

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        var focusSlot = InventorySystem.Instance.focusSlot;

        //1.松开鼠标, 把手上的物品放到空格子 
        //有焦点格子, 可以放
        if (focusSlot != null)
        {
            //空格子
            if (focusSlot.bindItem == null)
            {
                PutItem(focusSlot);
            }
            else
            {
                ExchangeOrMergeForDrag();


                //有物品
                //...
                //2.同类 溢出或者堆叠

                //3.交换


            }
        }
        else
        {
            //无焦点格子

            InventorySystem.Instance.Discard(itemData);
            //删除手上物品
            SetItemAmount(0);

            //断开手上链接
            InventorySystem.Instance.palm.slotList[0].bindItem = null;
            //Destroy(gameObject);
            //beforeSlot置空
            InventorySystem.Instance.beforeSlot = null;

        }


    }

    //交换或者合并（拖拽）
    private void ExchangeOrMergeForDrag()
    {
        Debug.Log("ExchangeOrMergeForDrag");

        //手上插槽 （手上拿着的物品格子）
        var palmSlot = InventorySystem.Instance.palm.slotList[0];

        //焦点插槽 （鼠标当前指向的格子)
        var focusSlot = InventorySystem.Instance.focusSlot;

        //判断是否为同类型物品
        if (palmSlot.bindItem.itemData.id == focusSlot.bindItem.itemData.id)
        {
            //同类
            MergeItemForDrag();

        }
        else
        {
            //不是同类 交换
            //在初始格子上绑定焦点格子上的物品
            InventorySystem.Instance.beforeSlot.BindItem(focusSlot.bindItem);

            //焦点格子绑定手上的物品
            focusSlot.BindItem(palmSlot.bindItem);
          
            //手上链接断开
            palmSlot.bindItem = null;
        }

    }

    //合并物品（拖拽）
    private void MergeItemForDrag()
    {
        //手上插槽
        var palmSlot = InventorySystem.Instance.palm.slotList[0];

        //焦点插槽
        var focusSlot = InventorySystem.Instance.focusSlot;

        //初始格子上
        var beforeSlot = InventorySystem.Instance.beforeSlot;

        int overflow = palmSlot.bindItem.itemData.current_stack
                     + focusSlot.bindItem.itemData.current_stack
                     - focusSlot.bindItem.itemData.max_stack;

        //判断溢出值
        if (overflow >0)
        {
            //有溢出
            //放满焦点格子
            focusSlot.SetItemAmount(focusSlot.bindItem.itemData.max_stack);

            //手上的设置数量减去放下的数量
            palmSlot.SetItemAmount(overflow);

            //手上的物品放回原处
            beforeSlot.BindItem(palmSlot.bindItem);

            //断开手的链接
            palmSlot.bindItem=null;

        }
        else
        {
            //没有溢出 能放下

            //手上的能放到焦点格子里
            focusSlot.bindItem.SetItemAmount(  // 你手上拿着的物品
                focusSlot.bindItem.itemData.current_stack // 你拖过去的目标格子
                + palmSlot.bindItem.itemData.current_stack); //物品原来所在的格子

            //删除手上物品
            palmSlot.SetItemAmount(0);
        }
    }

    //放下物品
    private void PutItem(InventorySlot focusSlot)
    {
        Debug.Log("放下物品");
        //从手上放到焦点格子上

        focusSlot.BindItem(this);  //双向绑定

        //断开手的绑定
        var palmSlot = InventorySystem.Instance.palm.slotList[0]; //手的插槽
        palmSlot.bindItem = null;

    }
}



//物品数据模型
[Serializable]
public class ItemData
{

    public ItemData()
    {

    }

    public ItemData(ItemData itemData)
    {
        this.id = itemData.id;
        this.name = itemData.name;
        this.describe = itemData.describe;
        icon_name = itemData.icon_name;
        item_type = itemData.item_type;
        item_sub_type = itemData.item_sub_type;
        is_stackable = itemData.is_stackable;
        max_stack = itemData.max_stack;
        current_stack = itemData.current_stack;
        is_durable = itemData.is_durable;
        max_durable = itemData.max_durable;
        current_durable = itemData.current_durable;

        this.icon = itemData.icon;
    }

    public int id; //物品id  1
    public string name; //物品名称 鸡蛋 
    public string describe; //物品详细描述 可以吃
    public string icon_name; //图标名称
    public float price; //价格

    //堆叠相关
    public bool is_stackable; //是否可以堆叠
    public int max_stack; //最大可以堆叠数量
    public int current_stack; //当前的数量

    //耐久相关
    public bool is_durable; //是否有耐久
    public int max_durable; //最大耐久
    public int current_durable; //当前耐久度

    //图像
    [JsonIgnore]
    public Sprite icon;
    private ItemData itemData;

    public ItemType item_type; //物品所属类型
    public ItemSubType item_sub_type; //物品类型二级细分
    // public WeaponType ......
    // public AnimalType ......
    // public WeaponType ......
}


