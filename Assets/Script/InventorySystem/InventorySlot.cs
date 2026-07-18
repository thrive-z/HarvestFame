using NUnit.Framework.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 插槽
/// </summary>
public class InventorySlot : MonoBehaviour,IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public InventoryContainer container;

    public InventoryItem bindItem; //插槽上绑定的物品

   
    public void SetContainer(InventoryContainer inventoryContainer)
    {
        this.container = inventoryContainer;
    }

     //设置库存数量
    public void SetItemAmount(int count)
    {
        bindItem.SetItemAmount(count);


    }

    //在空格子上添加item
    public ItemData AddItem(ItemData itemData, int count)
    {
        //创建对象
        GameObject go = Instantiate(InventorySystem.Instance.item_prefab, transform);
        InventoryItem item = go.GetComponent<InventoryItem>();

        //双向绑定
        item.bindSlot = this;
        bindItem = item;

        //设置数据
        item.SetItem(itemData, count);

        return itemData;
    }

    //拿起物品
    public void PickUpItem(InventoryItem item)
    {
        //之前的slot
        item.bindSlot.bindItem = null;

        item.bindSlot = this;
        this.bindItem = item;

        //ui移动
        item.transform.SetParent(this.transform);


    }



   
    //鼠标移入
    public void OnPointerEnter(PointerEventData eventData)
    {
        //设置焦点格子
        InventorySystem.Instance.focusSlot = this;

        if (bindItem != null)
        {
            //放大特效
            StartCoroutine(ScaleOverTime(1.2f, 0.2f));

            //显示详情
            InventorySystem.Instance.tip.SetUI(bindItem.itemData);
            InventorySystem.Instance.tip.OpenContainer(interactable:false);
        }

    }

    //鼠标移出
    public void OnPointerExit(PointerEventData eventData)
    {
        InventorySystem.Instance.focusSlot = null;
        if (bindItem != null)
        {
            //放大特效
            StartCoroutine(ScaleOverTime(1f, 0.2f));

            InventorySystem.Instance.tip.CloseContainer();
        }
    }

    //缩放特效
    public IEnumerator ScaleOverTime(float targetScale,float time)
    {
        float scaleTimer = 0;

        var _rt = bindItem._icon. GetComponent<RectTransform>();
        //保存初始状态
        Vector3 originScale = GetComponent<RectTransform>().localScale;
        Vector3 targetScaleV3 = Vector3.one * targetScale;

        while (scaleTimer < time)
        {
            _rt.localScale = Vector3.Lerp(
                originScale, targetScaleV3, scaleTimer / time
                );

            scaleTimer += Time.deltaTime;
            yield return null;
        }

        _rt.localScale = targetScaleV3;

    }


    //鼠标点击
    public void OnPointerClick(PointerEventData eventData)
    {
        //如果是左键
        if (eventData.button==PointerEventData.InputButton.Left)
        {
            LeftMouseClick();
        }
        else if(eventData.button == PointerEventData.InputButton.Right)
        {
            RightMouseClick();
        }
        else if (eventData.button == PointerEventData.InputButton.Middle)
        {
            MiddleMouseClick();
        }
    }

    //鼠标中键点击处理
    private void MiddleMouseClick()
    {
        Debug.Log("按下了鼠标中键");
    }

    //右键点击处理
    private void RightMouseClick()
    {
        Debug.Log("按下了鼠标右键");

        //判断手上是否有东西
        if (InventorySystem.Instance.isPicked())
        {
            //有东西
            PutOne();

        }
        else
        {
            //手上没东西
            //当前格子为空
            if (bindItem == null)
            {
                Debug.Log("无意义");
                return;
            }

            //当前格子不为空，拆分一半
            SplitStack();


        }

    }

    //右键放下一个物品
    private void PutOne()
    {
        Debug.Log("PutOne方法");

        var palmSlot = InventorySystem.Instance.palm.slotList[0];
     
        //被点击的格子 是否为空
        if (bindItem==null)
        {
            //放下一个物品
            this.AddItem(new ItemData(palmSlot.bindItem.itemData), 1);
            //手上减去1个物品
            palmSlot.SetItemAmount(palmSlot.bindItem.itemData.current_stack - 1);
        }
        else if (bindItem.itemData.id != palmSlot.bindItem.itemData.id)
        {
            //被点击的格子不为空，不是同类物品
            Debug.Log("无意义");
        }
        else if (bindItem.itemData.id == palmSlot.bindItem.itemData.id)
        {
            //被点击的格子不为空，是同类物品

            //可以放的数量
            int availableStack = bindItem.itemData.max_stack - bindItem.itemData.current_stack;

            //最少可以放一个
            if (availableStack >0)
            {
                bindItem.SetItemAmount(bindItem.itemData.current_stack + 1);
                palmSlot.bindItem.SetItemAmount(palmSlot.bindItem.itemData.current_stack - 1);
            }
            else
            {
                //没有空位置
                Debug.Log("同类型，无空间");
            }
        }

    }

    //拆分一半到手上
    private void SplitStack()
    {
        Debug.Log("拆分一半物品");

        //计算要拿起来的物品 向上取整
        int pickupCount = (int)Math.Ceiling(bindItem.itemData.current_stack / 2.0);

        int surplusCount = bindItem.itemData.current_stack - pickupCount;

        //拿起来一半物品到手上
        var palmSlot = InventorySystem.Instance.palm.slotList[0];
        palmSlot.AddItem(new ItemData(bindItem.itemData), pickupCount);

        //设置剩余数量
        bindItem.SetItemAmount(surplusCount);


    }

    //左键点击处理
    private void LeftMouseClick()
    {
        Debug.Log("按下了鼠标左键");

        //判断手上是否有东西
        if (InventorySystem.Instance.isPicked())
        {
            //手上有东西,判断当前格子
            if (this.bindItem == null)
            {
                Debug.Log("手上有东西，点击空格子，直接放下");
                var palmSlot = InventorySystem.Instance.palm.slotList[0];
                this.BindItem(palmSlot.bindItem); //绑定手上的物品
                palmSlot.bindItem = null; //断开手的连接


            }
            else
            {
                Debug.Log("手上有东西，格子不为空，堆叠或者交换");
                ExchangeOrMergeForClick();

            }


        }
        else
        {
            //手上没东西，拿起物品
            if (this.bindItem != null)
            {
                Debug.Log("手上没东西,格子上有东西，直接拿起");
                InventorySystem.Instance.palm.PickUpItem(this);

                this.bindItem = null;
            }
            else
            {
                Debug.Log("手上没东西,格子上也没东西，无意义");
            }
        }

    }

    //交换或堆叠
    private void ExchangeOrMergeForClick()
    {
        Debug.Log("ExchangeOrMergeForClick");

        var palmSlot = InventorySystem.Instance.palm.slotList[0];

        //判断
        if (palmSlot.bindItem.itemData.id == bindItem.itemData.id)
        {
            //同类型物品
            MergeItemForClick();
        }
        else
        {
            //不是同类型物品 交换
            //保存手上的数据
            ItemData itemData = palmSlot.bindItem.itemData;

            //销毁手上的对象
            Destroy(palmSlot.bindItem.gameObject);
            //手上拿起
            palmSlot.BindItem(this.bindItem);
            //格子上生成手上的数据
            AddItem(itemData, itemData.current_stack);
        }

    }

    //合并
    private void MergeItemForClick()
    {
        var palmSlot = InventorySystem.Instance.palm.slotList[0];

        //溢出值
        var overflow = palmSlot.bindItem.itemData.current_stack
                     + bindItem.itemData.current_stack
                     - bindItem.itemData.max_stack;

        //是否有溢出
        if (overflow >0)
        {
            //目标格子放满
            bindItem.SetItemAmount(bindItem.itemData.max_stack);

            //设置手的数量为溢出值
            palmSlot.bindItem.SetItemAmount(overflow);
        }
        else
        {
            //没有溢出，直接相加
            bindItem.SetItemAmount(bindItem.itemData.current_stack + palmSlot.bindItem.itemData.current_stack);

            //删除手上的物品
            palmSlot.SetItemAmount(0);

        }
    }

    //绑定item
    public void BindItem(InventoryItem inventoryItem)
    {
        //双向绑定
        inventoryItem.bindSlot = this;
        this.bindItem = inventoryItem;

        //设置父级
        inventoryItem.transform.SetParent(this.transform);
        //保证位置在正中心
        (inventoryItem.transform as RectTransform).anchoredPosition = Vector2.zero;

    }
}
