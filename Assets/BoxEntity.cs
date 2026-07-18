using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 储物箱实体
/// </summary>
public class BoxEntity : MonoBehaviour,IInteractable
{
    public List<ItemData> itemDataList = new(); //物品数据

    public bool isOpen = false; //当前储物箱是否打开

    private void Update()
    {
        //如果当前被打开，并且按了Esc
        if (isOpen &&Input.GetKeyDown(KeyCode.Escape))
        {
            //关闭储物箱
            itemDataList = StorageBox.Instance.CloseBox();
            isOpen = false;
        }
    }

    //打开储物箱
    public void OpenBox()
    {
        isOpen = true;
        //打开ui，传入数据
        StorageBox.Instance.OpenBox(itemDataList);
    }

    public string GetInteractTip()
    {
        return "按F打开储物箱";
    }

    public bool CanInteract()
    {
        return true;
    }

    public void Interact(GameObject interactor)
    {
        OpenBox();
    }

}
