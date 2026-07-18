using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 背包
/// </summary>
public class Backpack : InventoryContainer
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //如果按下了B
        if (Input.GetKeyDown(KeyCode.B))
        {
            SwitchContainer();
        }
    }
}
