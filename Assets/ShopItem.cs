using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{

    public ItemData itemData;

    public Button _buyButton; //购买按钮
    public TMP_Text _shopItemPrice; //价格
    public TMP_Text _shopItemName; //名称
    public Image _shopItemIcon; //图标


    private void Awake()
    {
        _buyButton = transform.Find("Button").GetComponent <Button>();

        _shopItemPrice = transform.Find("ShopItemPrice").GetComponent<TMP_Text>();
        _shopItemName = transform.Find("ShopItemName").GetComponent<TMP_Text>();
        _shopItemIcon = transform.Find("Icon").GetComponent<Image>();

        //按钮点击 添加物品
        _buyButton.onClick.AddListener(() =>
        {
            Debug.Log("购买" + itemData.name);
            InventorySystem.Instance.TryAddItem(itemData, 1);
        });
    }

    // Start is called before the first frame update
    void Start()
    {
        _shopItemPrice.text = itemData.price.ToString();
        _shopItemName.text = itemData.name;
        _shopItemIcon.sprite = itemData.icon;


    }

    //设置数据
    public void SetData(ItemData data)
    {
        this.itemData = data;
    }
}
