using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 可交互物品的脚本
/// </summary>
public class CollectItem : MonoBehaviour, IInteractable
{
    public ItemData itemData;

    
    //拾取方法
    public void Collect()
    {
        //调用背包的添加物品
        if (!InventorySystem.Instance.TryAddItem(itemData, itemData.current_stack))
        {
            AgricultureSystem.Instance?.Notify("背包已满。");
            return;
        }

        //销毁自身
        Destroy(gameObject);
    }

    //设置数据
    public void SetData(ItemData itemData,int stack)
    {
        this.itemData = itemData;
        this.itemData.current_stack = stack;
    }

    //接口里的方法 显示文字提示
    public string GetInteractTip()
    {
        return "按F捡起 " + itemData.name;
    }

    //是否可以互动
    public bool CanInteract()
    {
        return true;
    }

    //互动方法
    public void Interact(GameObject interactor)
    {
        Collect(); //直接拾取
    }


}
