using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 商店逻辑
/// </summary>
public class Shop : UIContainer
{
    public Transform _shopLayout; //布局容器

    public GameObject shopItem_prefab; //商店条目预制体

    private void Awake()
    {
        _shopLayout = GameObject.Find("ShopLayout").transform;
        shopItem_prefab = Resources.Load<GameObject>("Prefab/ShopItem");
    }

    // Start is called before the first frame update
    void Start()
    {
        var itemDatas =LoadManager.instance.itemDatas;

        foreach (var itemData in itemDatas)
        {
            GameObject go = Instantiate(shopItem_prefab, _shopLayout);
            go.GetComponent<ShopItem>().SetData(itemData);

        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            SwitchContainer();
        }
    }
}
