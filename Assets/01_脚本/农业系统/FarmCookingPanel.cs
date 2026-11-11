using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class FarmCookingPanel
{
    private static readonly Color Paper = new(0.95f, 0.81f, 0.53f);
    private static readonly Color SlotColor = new(0.80f, 0.52f, 0.27f);
    private readonly TMP_FontAsset font;
    private readonly GameObject panel;
    private readonly TMP_Text state;
    private readonly ItemCell[] ingredients = new ItemCell[4];
    private readonly ItemCell fuel;
    private readonly ItemCell output;
    private readonly ItemCell[] recipeCells = new ItemCell[3];
    private readonly Image progress;
    private readonly Button start;
    private readonly Button cancel;
    private readonly Button collect;
    private FarmStation station;

    public FarmCookingPanel(Transform canvas)
    {
        font = Resources.Load<TMP_FontAsset>("Font/AlibabaPuHuiTi-3-55-Regular SDF");
        panel = Rectangle(canvas, "Cooking board", Vector2.zero, new Vector2(1110f, 506f), Paper).gameObject;
        var parent = Rectangle(panel.transform, "Cooking text backdrop", Vector2.zero, new Vector2(1086f, 482f),
            FarmPanelText.BackdropColor).transform;
        Label(parent, "烹饪锅", new Vector2(-335f, 198f), new Vector2(354f, 60f), 46, TextAlignmentOptions.Left)
            .color = FarmPanelText.Accent;
        state = Label(parent, string.Empty, new Vector2(0f, 133f), new Vector2(1020f, 54f), 37, TextAlignmentOptions.Left);
        var bar = Rectangle(parent, "Progress track", new Vector2(0f, 96f), new Vector2(1020f, 12f), SlotColor);
        progress = Rectangle(bar.transform, "Cooking progress", Vector2.zero, new Vector2(1020f, 12f),
            new Color(0.38f, 0.55f, 0.25f));
        progress.rectTransform.pivot = new Vector2(0f, 0.5f);
        progress.rectTransform.anchoredPosition = new Vector2(-510f, 0f);

        Label(parent, "食材", new Vector2(-424f, 62f), new Vector2(168f, 44f), 34, TextAlignmentOptions.Left);
        Label(parent, "燃料", new Vector2(-424f, -76f), new Vector2(168f, 44f), 34, TextAlignmentOptions.Left);
        Label(parent, "成品", new Vector2(42f, 62f), new Vector2(124f, 44f), 34);
        Label(parent, "配方", new Vector2(346f, 62f), new Vector2(180f, 44f), 34);
        for (var i = 0; i < ingredients.Length; i++)
            ingredients[i] = new ItemCell(this, parent, $"Ingredient {i + 1}", new Vector2(-464f + i * 110f, -9f));
        fuel = new ItemCell(this, parent, "Fuel", new Vector2(-464f, -147f));
        output = new ItemCell(this, parent, "Output", new Vector2(42f, -9f));
        for (var i = 0; i < recipeCells.Length; i++)
        {
            var index = i;
            recipeCells[i] = new ItemCell(this, parent, $"Recipe {i + 1}",
                new Vector2(228f + i * 118f, -9f));
            var button = recipeCells[i].Background.gameObject.AddComponent<Button>();
            button.targetGraphic = recipeCells[i].Background;
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(() => station.SelectRecipe(index));
        }

        start = Action(parent, "开始制作", new Vector2(-135f, -111f), () => station.CookSelected());
        cancel = Action(parent, "取消制作", new Vector2(110f, -111f), () => station.CancelCooking());
        collect = Action(parent, "领取成品", new Vector2(355f, -111f), () => station.CollectCooked());
        Label(parent, "点选右侧配方查看用料    Esc 关闭", new Vector2(84f, -199f), new Vector2(806f, 45f), 32,
            TextAlignmentOptions.Left);
        panel.SetActive(false);
    }

    public void Open(FarmStation active)
    {
        station = active;
        panel.SetActive(true);
        Refresh();
    }

    public void Close() => panel.SetActive(false);

    public void Refresh()
    {
        var recipe = station.Current;
        var busy = station.IsCooking || station.IsCooked;
        state.text = station.IsCooked ? $"{recipe.name}  ·  已完成"
            : station.IsCooking ? $"{recipe.name}  ·  烹饪中" : recipe.name + "  ·  检查食材和木柴";
        progress.rectTransform.localScale = new Vector3(Mathf.Clamp01(station.CookProgress), 1f, 1f);
        for (var i = 0; i < ingredients.Length; i++)
        {
            var cost = i < recipe.inputs.Length ? recipe.inputs[i] : null;
            ingredients[i].Set(cost?.item_id ?? 0, cost == null ? string.Empty : busy
                ? cost.count.ToString() : $"{station.IngredientCount(cost.item_id)}/{cost.count}");
        }
        fuel.Set(recipe.require_fuel ? 102 : 0, !recipe.require_fuel ? string.Empty
            : busy ? "1" : $"{station.IngredientCount(102)}/1");
        output.Set(recipe.outputs[0].item_id, station.IsCooked ? recipe.outputs[0].count.ToString() : string.Empty);
        for (var i = 0; i < recipeCells.Length; i++)
        {
            var card = i < station.Recipes.Count ? station.Recipes[i] : null;
            recipeCells[i].Set(card?.outputs[0].item_id ?? 0, string.Empty);
        }
        start.interactable = !busy;
        cancel.interactable = station.IsCooking;
        collect.interactable = station.IsCooked;
    }

    private Button Action(Transform parent, string text, Vector2 position, UnityEngine.Events.UnityAction pressed)
    {
        var image = Rectangle(parent, text, position, new Vector2(220f, 64f), new Color(0.48f, 0.24f, 0.10f));
        var button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        var colors = button.colors;
        colors.disabledColor = new Color(0.50f, 0.50f, 0.50f, 1f);
        button.colors = colors;
        button.onClick.AddListener(pressed);
        Label(image.transform, text, Vector2.zero, new Vector2(210f, 54f), 37);
        return button;
    }

    private static Image Rectangle(Transform parent, string name, Vector2 position, Vector2 size, Color color)
    {
        var element = new GameObject(name, typeof(Image));
        element.transform.SetParent(parent, false);
        var rect = (RectTransform)element.transform;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        var image = element.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private TMP_Text Label(Transform parent, string text, Vector2 position, Vector2 size, float sizePt,
        TextAlignmentOptions align = TextAlignmentOptions.Center)
    {
        var label = new GameObject("Label", typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        label.rectTransform.SetParent(parent, false);
        label.rectTransform.anchoredPosition = position;
        label.rectTransform.sizeDelta = size;
        label.font = font;
        label.text = text;
        FarmPanelText.Style(label, Mathf.Max(12f, sizePt - 4f), sizePt, align);
        return label;
    }

    private sealed class ItemCell
    {
        private readonly Image icon;
        private readonly TMP_Text caption;
        public Image Background { get; }

        public ItemCell(FarmCookingPanel owner, Transform parent, string name, Vector2 position)
        {
            Background = Rectangle(parent, name, position, new Vector2(96f, 90f), SlotColor);
            icon = Rectangle(Background.transform, "Icon", new Vector2(0f, 4f), new Vector2(76f, 76f), Color.white);
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            var badge = FarmPanelText.Backdrop(Background.transform, "Quantity");
            badge.color = FarmPanelText.QuantityBackdropColor;
            badge.rectTransform.anchoredPosition = new Vector2(11f, -24f);
            badge.rectTransform.sizeDelta = new Vector2(70f, 36f);
            caption = owner.Label(badge.transform, string.Empty, Vector2.zero, new Vector2(66f, 36f), 27);
        }

        public void Set(int item, string text)
        {
            LoadManager.instance.itemDatasDict.TryGetValue(item, out var data);
            icon.sprite = data?.icon;
            icon.enabled = icon.sprite != null;
            caption.text = data == null ? string.Empty : icon.enabled ? text : string.IsNullOrEmpty(text) ? data.name : text;
            caption.transform.parent.gameObject.SetActive(caption.text.Length > 0);
        }
    }
}
