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
            InventorySystem.Instance.tip.SetUI(bindItem.itemData, (RectTransform)transform);
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
        InventorySystem.Instance.palm.slotList[0].TransferTo(this, 1);
    }

    //拆分一半到手上
    private void SplitStack()
    {
        var count = (bindItem.itemData.current_stack + 1) / 2;
        TransferTo(InventorySystem.Instance.palm.slotList[0], count);
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
        var palm = InventorySystem.Instance.palm.slotList[0];
        palm.TransferTo(this, palm.bindItem.itemData.current_stack);
    }

    //绑定item
    public void TransferTo(InventorySlot destination, int count)
    {
        if (bindItem == null || destination == this)
            return;

        var source = bindItem.itemData;
        var target = destination.bindItem?.itemData;
        if (target != null && target.id != source.id)
            return;

        var capacity = target == null ? source.max_stack : target.max_stack - target.current_stack;
        var moved = Mathf.Min(count, source.current_stack, capacity);
        if (moved <= 0)
            return;

        var remaining = source.current_stack - moved;
        if (target == null)
            destination.AddItem(source.CopyStackRange(remaining, moved), moved);
        else
        {
            target.AppendStackRange(source, remaining, moved);
            destination.SetItemAmount(target.current_stack + moved);
        }

        SetItemAmount(remaining);
    }

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
