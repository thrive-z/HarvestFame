using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 手掌
/// </summary>
public class Palm : InventoryContainer
{
   
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // 手上有物品（加了防空列表判断）
        if (slotList.Count > 0 && slotList[0].bindItem != null)
        {
            FollowMouse();
        }
    }

    public float moveSpeed = 20f;
    private void FollowMouse()
    {
        //鼠标位置转为canvas的坐标
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            UIManager.Instance.canvas.GetComponent<RectTransform>(),
            Input.mousePosition,
            UIManager.Instance.uiCamera,
            out Vector2 canvasLocalPosition
        );

        //
        var item = slotList[0].bindItem.transform as RectTransform;

        item.anchoredPosition = Vector2.Lerp(
            item.anchoredPosition,
            canvasLocalPosition,
            Time.deltaTime * moveSpeed
            );

    }

    //拿起物品
    public void PickUpItem(InventorySlot slot)
    {
        slotList[0].PickUpItem(slot.bindItem);

        //断开原来的连接
        slot.bindItem = null;
    }

    //重新生成初始化
    public override List<InventorySlot>InitContainer(int slotCount)
    {
        var list = base.InitContainer(slotCount);
        //关闭手上的背景
        list[0].GetComponent<Image>().enabled = false;

        return list;
    }

}
