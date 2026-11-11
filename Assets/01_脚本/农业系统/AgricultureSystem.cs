using System.Collections.Generic;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AgricultureSystem : MonoBehaviour
{
    public static AgricultureSystem Instance { get; private set; }

    [SerializeField] private GameObject[] pumpkinStages;
    [SerializeField] private GameObject[] tomatoStages;
    [SerializeField] private Transform farmLayout;

    private readonly Dictionary<int, CropDefinition> crops = new();
    private readonly Dictionary<string, AudioClip> sounds = new();
    private AudioSource audioSource;
    private TMP_Text status;
    private TMP_Text message;
    private TMP_Text panel;
    private FarmCookingPanel cookingPanel;
    private float messageUntil;
    private FarmStation activeStation;
    private int stationClosedFrame = -1;

    public FarmVitals Vitals { get; private set; }
    public bool HasOpenStation => activeStation != null;
    public bool ClosedStationThisFrame => stationClosedFrame == Time.frameCount;
    public static bool WorldInputEnabled => Cursor.lockState == CursorLockMode.Locked
        && (Instance == null || !Instance.HasOpenStation)
        && FarmCampaign.Instance?.BlocksWorldInput != true;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Vitals = Player.Instance.gameObject.AddComponent<FarmVitals>();
        Vitals.Initialize();
        foreach (var crop in ReadData<List<CropDefinition>>("PlantData"))
        {
            crop.Models = crop.prefab_name == "pumpkin_seed" ? pumpkinStages : tomatoStages;
            crops.Add(crop.item_id, crop);
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        foreach (var plot in farmLayout.GetComponentsInChildren<FarmPlot>())
            plot.Initialize(this, farmLayout);
        foreach (var station in farmLayout.GetComponentsInChildren<FarmStation>())
            station.Initialize(this);

        BuildHud();
    }

    private void Update()
    {
        if (activeStation != null)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                CloseStation();
            else
                activeStation.HandleMenuInput();
        }
        else if (WorldInputEnabled && FarmCampaign.Instance?.IsDriving != true && Input.GetKeyDown(KeyCode.G))
        {
            var slot = InventorySystem.Instance.toolbar.SelectedSlot;
            if (slot.bindItem != null)
                Vitals.TryEat(slot);
        }
    }

    private void LateUpdate()
    {
        if (status == null)
            return;

        var inventory = InventorySystem.Instance;
        status.gameObject.SetActive(!HasOpenStation && !inventory.backpack.isOpen && !inventory.storageBox.isOpen
            && Shop.Instance?.isOpen != true && FarmCampaign.Instance?.BlocksWorldInput != true);
        var item = inventory.toolbar.SelectedSlot.bindItem?.itemData;
        var selection = item?.name ?? "空手";
        if (item?.current_durable >= 0)
            selection += $"  耐久 {item.current_durable}";
        if (item?.item_sub_type == ItemSubType.WaterCan)
            selection += $"  水量 {FarmStation.WaterRemaining(item)}/{FarmStation.WaterCapacity}";

        var health = FarmCampaign.Instance == null ? string.Empty : FarmCampaign.Instance.HealthText + "   ";
        status.text = $"{health}体力 {Mathf.CeilToInt(Vitals.Stamina)}   饱食 {Mathf.CeilToInt(Vitals.Satiety)}   {selection}\n"
            + "1-0 工具  F 互动  G 食用  B 背包";
        message.gameObject.SetActive(Time.time < messageUntil);
        if (activeStation != null)
        {
            if (activeStation.StationKind == FarmStation.Kind.CookingPot)
                cookingPanel.Refresh();
            else
                panel.text = activeStation.DescribeMenu();
        }
    }

    public static T ReadData<T>(string name)
    {
        return JsonConvert.DeserializeObject<T>(Resources.Load<TextAsset>("Data/" + name).text);
    }

    public bool TryUsePlot(ToolType tool, float reach)
    {
        var interaction = Player.Instance.GetComponent<PlayerInteraction>();
        if (!interaction.TryGetTarget(out var hit, reach))
            return false;

        if (tool == ToolType.WaterCan && hit.collider.GetComponentInParent<FarmStation>() is FarmStation station
            && station.StationKind == FarmStation.Kind.Well)
        {
            station.Interact(Player.Instance.gameObject);
            return false; // Refilling does not consume tool charges.
        }

        var plot = hit.collider.GetComponent<FarmPlot>();
        if (plot == null)
            return false;

        if (tool == ToolType.Hoe)
            return plot.Till();

        var item = InventorySystem.Instance.toolbar.SelectedSlot.bindItem.itemData;
        if (FarmStation.WaterRemaining(item) <= 0)
        {
            Notify("花洒空了。对准水井按 F 补水。");
            return false;
        }
        if (!plot.Water())
            return false;

        item.water_remaining = FarmStation.WaterRemaining(item) - 1;
        return true;
    }

    public CropDefinition GetCrop(int itemId) => crops.TryGetValue(itemId, out var crop) ? crop : null;

    public void PlantSelected(InventorySlot slot)
    {
        var item = slot.bindItem?.itemData;
        if (item == null || !crops.TryGetValue(item.id, out var crop))
            return;

        var interaction = Player.Instance.GetComponent<PlayerInteraction>();
        if (!interaction.TryGetTarget(out var hit) || !hit.collider.TryGetComponent(out FarmPlot plot))
            return;

        if (plot.Plant(crop))
        {
            slot.SetItemAmount(item.current_stack - 1);
            if (crop.item_id == 21)
                FarmCampaign.Instance?.Report("pumpkinPlanted");
        }
    }

    public void OpenStation(FarmStation station)
    {
        activeStation = station;
        if (station.StationKind == FarmStation.Kind.CookingPot)
        {
            cookingPanel.Open(station);
            UIManager.Instance.OpenCursor();
        }
        else
        {
            panel.transform.parent.gameObject.SetActive(true);
            panel.text = station.DescribeMenu();
        }
    }

    private void CloseStation()
    {
        if (activeStation.StationKind == FarmStation.Kind.CookingPot)
        {
            cookingPanel.Close();
            UIManager.Instance.HideCursor();
        }
        activeStation = null;
        stationClosedFrame = Time.frameCount;
        panel.transform.parent.gameObject.SetActive(false);
    }

    public void Notify(string text)
    {
        message.text = text;
        messageUntil = Time.time + 3f;
    }

    public void PlaySound(string name)
    {
        if (!sounds.TryGetValue(name, out var clip))
        {
            clip = Resources.Load<AudioClip>("Music/" + name);
            sounds[name] = clip;
        }

        if (clip != null)
            audioSource.PlayOneShot(clip, 0.55f);
    }

    private void BuildHud()
    {
        var hud = new GameObject("Farm HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        hud.transform.SetParent(transform);
        var canvas = hud.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;
        var scaler = hud.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;

        status = AddLabel(hud.transform, "Status", new Vector2(0f, 1f), new Vector2(8f, -6f),
            new Vector2(715f, 104f), 30f, TextAlignmentOptions.TopLeft);
        message = AddLabel(hud.transform, "Feedback", new Vector2(0.5f, 0f), new Vector2(0f, 145f),
            new Vector2(1090f, 56f), 33f, TextAlignmentOptions.Center);
        var aim = AddLabel(hud.transform, "Aim", new Vector2(0.5f, 0.5f), Vector2.zero,
            new Vector2(20f, 20f), 19f, TextAlignmentOptions.Center);
        aim.text = "+";

        var background = new GameObject("Station Menu", typeof(RectTransform), typeof(Image));
        background.transform.SetParent(hud.transform, false);
        var rect = (RectTransform)background.transform;
        rect.sizeDelta = new Vector2(930f, 510f);
        rect.anchoredPosition = new Vector2(0f, 40f);
        var image = background.GetComponent<Image>();
        image.color = new Color(0.08f, 0.13f, 0.10f, 0.97f);
        image.raycastTarget = false;
        panel = AddLabel(background.transform, "Station Text", new Vector2(0.5f, 0.5f), Vector2.zero,
            new Vector2(860f, 450f), 34f, TextAlignmentOptions.TopLeft);
        background.SetActive(false);
        cookingPanel = new FarmCookingPanel(hud.transform);
    }

    private static TMP_Text AddLabel(Transform parent, string name, Vector2 anchor, Vector2 position,
        Vector2 size, float fontSize, TextAlignmentOptions alignment)
    {
        var label = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        label.transform.SetParent(parent, false);
        var rect = (RectTransform)label.transform;
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        var text = label.GetComponent<TextMeshProUGUI>();
        text.font = Resources.Load<TMP_FontAsset>("Font/AlibabaPuHuiTi-3-55-Regular SDF");
        text.fontSize = fontSize;
        text.color = new Color(1f, 0.96f, 0.82f);
        text.alignment = alignment;
        text.raycastTarget = false;
        return text;
    }
}
