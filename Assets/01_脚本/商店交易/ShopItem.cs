using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{
    public ItemData itemData;
    public Button _buyButton;
    public TMP_Text _shopItemPrice;
    public TMP_Text _shopItemName;
    public Image _shopItemIcon;

    private void Awake()
    {
        _buyButton = transform.Find("Button").GetComponent<Button>();
        _shopItemPrice = transform.Find("ShopItemPrice").GetComponent<TMP_Text>();
        _shopItemName = transform.Find("ShopItemName").GetComponent<TMP_Text>();
        _shopItemIcon = transform.Find("Icon").GetComponent<Image>();
        var background = transform.Find("Background").GetComponent<Image>();
        background.sprite = null;
        background.color = FarmPanelText.BackdropColor;
        background.raycastTarget = false;
        FarmPanelText.Inset(_shopItemIcon.rectTransform, Vector2.zero, Vector2.zero,
            new Vector2(19f, 45f), new Vector2(105f, 131f));
        _shopItemIcon.preserveAspect = true;
        FarmPanelText.Inset(_shopItemName.rectTransform, Vector2.zero, Vector2.one,
            new Vector2(129f, 84f), new Vector2(-15f, -8f));
        FarmPanelText.Style(_shopItemName, 42f, 54f, TextAlignmentOptions.MidlineLeft);
        FarmPanelText.Inset(_shopItemPrice.rectTransform, Vector2.zero, Vector2.one,
            new Vector2(129f, 10f), new Vector2(-185f, -90f));
        FarmPanelText.Style(_shopItemPrice, 38f, 48f, TextAlignmentOptions.MidlineLeft);
        _shopItemPrice.color = FarmPanelText.Accent;
        var buttonImage = _buyButton.GetComponent<Image>();
        buttonImage.sprite = null;
        buttonImage.color = new Color(0.46f, 0.23f, 0.09f);
        FarmPanelText.Inset(buttonImage.rectTransform, Vector2.right, Vector2.right,
            new Vector2(-163f, 10f), new Vector2(-14f, 79f));
        var buttonText = _buyButton.GetComponentInChildren<TMP_Text>();
        buttonText.font = _shopItemName.font;
        buttonText.text = "购买";
        FarmPanelText.Inset(buttonText.rectTransform, Vector2.zero, Vector2.one,
            new Vector2(4f, 3f), new Vector2(-4f, -3f));
        FarmPanelText.Style(buttonText, 39f, 47f, TextAlignmentOptions.Center);
        _buyButton.onClick.AddListener(() => Shop.Instance.Buy(itemData));
    }

    private void Start()
    {
        _shopItemPrice.text = $"{Mathf.CeilToInt(itemData.price)} 金币";
        _shopItemName.text = itemData.name;
        _shopItemIcon.sprite = itemData.icon;

        if (itemData.id is 56 or 57 or 100 or 200 or 201)
            FarmPanelText.Inset(_shopItemIcon.rectTransform, Vector2.zero, Vector2.zero,
                new Vector2(12f, 34f), new Vector2(120f, 142f));
        else if (itemData.id == 32)
            FarmPanelText.Inset(_shopItemIcon.rectTransform, Vector2.zero, Vector2.zero,
                new Vector2(5f, 31f), new Vector2(119f, 145f));
    }

    public void SetData(ItemData data) => itemData = data;
}
