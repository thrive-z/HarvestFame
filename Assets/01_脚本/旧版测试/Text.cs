using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Text : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
        //尝试添加物品
        InventorySystem.Instance.TryAddItemById(1, 10);//添加蘑菇种子10个
        InventorySystem.Instance.TryAddItemById(21, 3);//添加南瓜种子3个

    }

    
}
