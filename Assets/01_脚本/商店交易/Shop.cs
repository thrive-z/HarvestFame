using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Shop : UIContainer
{
    public static Shop Instance { get; private set; }
    public Transform _shopLayout;
    public GameObject shopItem_prefab;
    private TMP_Text balance;
    private ScrollRect scroll;
    private bool openedOnce;
    private int lastClosed = -1;
    public bool ClosedThisFrame => lastClosed == Time.frameCount;

    private void Awake()
    {
        Instance = this;
        _shopLayout = GameObject.Find("ShopLayout").transform;
        shopItem_prefab = Resources.Load<GameObject>("Prefab/ShopItem");
    }

    private void Start()
    {
        foreach (var item in LoadManager.instance.itemDatas)
            Instantiate(shopItem_prefab, _shopLayout).GetComponent<ShopItem>().SetData(item);

        var window = (RectTransform)transform;
        window.sizeDelta = new Vector2(1400f, 815f);
        window.anchoredPosition = new Vector2(0f, 65f);
        scroll = GetComponentInChildren<ScrollRect>();
        var grid = _shopLayout.GetComponent<GridLayoutGroup>();
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 2;
        grid.cellSize = new Vector2(610f, 175f);
        grid.spacing = new Vector2(32f, 24f);
        var rows = Mathf.CeilToInt(_shopLayout.childCount / 2f);
        scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
            grid.padding.vertical + rows * grid.cellSize.y + Mathf.Max(0, rows - 1) * grid.spacing.y);
        FarmPanelText.Inset((RectTransform)scroll.transform, Vector2.zero, Vector2.one,
            new Vector2(10f, 26f), new Vector2(-10f, -148f));

        var header = FarmPanelText.Backdrop(transform, "Market header");
        FarmPanelText.Inset(header.rectTransform, Vector2.up, Vector2.one,
            new Vector2(18f, -138f), new Vector2(-18f, -12f));
        balance = FarmPanelText.Label(header.transform, "Market balance");
        FarmPanelText.Inset(balance.rectTransform, Vector2.zero, Vector2.one,
            new Vector2(15f, 7f), new Vector2(-15f, -7f));
        FarmPanelText.Style(balance, 42f, 47f, TextAlignmentOptions.Center);
        RefreshBalance();
    }

    private void Update()
    {
        if (AgricultureSystem.Instance?.HasOpenStation == true || FarmCampaign.Instance?.BlocksWorldInput == true
            || FarmCampaign.Instance?.IsDriving == true)
            return;

        if (Input.GetKeyDown(KeyCode.P))
            SwitchContainer();
        if (!isOpen)
            return;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseContainerAndCursor();
            lastClosed = Time.frameCount;
            return;
        }
        if (Input.GetKeyDown(KeyCode.X))
            FarmCampaign.Instance?.SellSelected();
        if (!openedOnce)
        {
            Canvas.ForceUpdateCanvases();
            scroll.StopMovement();
            scroll.verticalNormalizedPosition = 1f;
            openedOnce = true;
        }
        RefreshBalance();
    }

    private void RefreshBalance() => balance.text =
        $"<b>农场商店</b> · {FarmCampaign.Instance?.Coins ?? 0} 金币    X 出售    P / Esc 关闭";

    public void OpenMarket() => OpenContainerAndCursor();

    public bool Buy(ItemData item)
    {
        var campaign = FarmCampaign.Instance;
        var price = Mathf.CeilToInt(item.price);
        if (campaign != null && campaign.Coins < price)
        {
            campaign.Notify("金币不足。可以出售农产或采集材料。");
            return false;
        }

        if (!InventorySystem.Instance.TryAddItem(item, 1))
        {
            campaign?.Notify("背包已满。");
            return false;
        }

        if (campaign != null)
        {
            campaign.SpendCoins(price);
            if (item.id == 6)
                campaign.Report("axePurchase");
        }
        return true;
    }
}
