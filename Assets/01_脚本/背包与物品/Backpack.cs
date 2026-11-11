using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Backpack : InventoryContainer
{
    private void Start()
    {
        var grid = GetComponentInChildren<GridLayoutGroup>();
        grid.padding.top = 122;
        grid.spacing = new Vector2(grid.spacing.x, 16f);
        var header = FarmPanelText.Backdrop(transform, "Backpack header");
        FarmPanelText.Inset(header.rectTransform, Vector2.up, Vector2.one,
            new Vector2(136f, -105f), new Vector2(-136f, -21f));
        var title = FarmPanelText.Label(header.transform, "Backpack title");
        title.text = "<b>背包</b>    拖动物品整理    B 关闭";
        FarmPanelText.Inset(title.rectTransform, Vector2.zero, Vector2.one,
            new Vector2(14f, 4f), new Vector2(-14f, -4f));
        FarmPanelText.Style(title, 46f, 53f, TextAlignmentOptions.Center);
    }

    private void Update()
    {
        if (AgricultureSystem.Instance?.HasOpenStation != true && FarmCampaign.Instance?.BlocksWorldInput != true
            && FarmCampaign.Instance?.IsDriving != true && Input.GetKeyDown(KeyCode.B))
        {
            var storage = InventorySystem.Instance.storageBox;
            if (storage.isOpen)
                storage.CloseActiveBox();
            else
                SwitchContainer();
        }
    }
}
