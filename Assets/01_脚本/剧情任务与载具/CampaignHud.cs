using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class CampaignHud
{
    private const float MinimapTop = -364f;
    private readonly TMP_FontAsset font;
    private readonly Transform root;
    private readonly GameObject overlay;
    private readonly TMP_Text title;
    private readonly TMP_Text body;
    private readonly TMP_Text prompt;
    private readonly RawImage portrait;
    private readonly GameObject tracker;
    private readonly TMP_Text objective;
    private readonly TMP_Text controls;
    private readonly TMP_Text feedback;
    private readonly TMP_Text combat;
    private readonly GameObject mapPanel;
    private readonly RectTransform mapImage;
    private readonly TMP_Text mapMarker;
    private readonly TMP_Text mapLegend;
    private bool expanded;
    private readonly Camera mapCamera;
    private readonly bool mapSupported;
    private readonly RenderTexture mapTexture;
    private float feedbackUntil;
    private float combatUntil;

    public CampaignHud(Transform owner)
    {
        font = Resources.Load<TMP_FontAsset>("Font/AlibabaPuHuiTi-3-55-Regular SDF");
        var canvasObject = new GameObject("Campaign HUD", typeof(Canvas), typeof(CanvasScaler));
        root = canvasObject.transform;
        root.SetParent(owner, false);
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 30;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;

        tracker = Panel(root, "Quest Tracker", Vector2.one, new Vector2(-8f, -6f), new Vector2(500f, 336f));
        ApplyBoard(tracker, "little board");
        var inset = Panel(tracker.transform, "Quest Text Backdrop", new Vector2(0f, 1f),
            new Vector2(36f, -85f), new Vector2(428f, 225f));
        inset.GetComponent<Image>().color = new Color(0.16f, 0.09f, 0.05f, 0.94f);
        objective = Label(inset.transform, "Objectives", new Vector2(0f, 1f),
            new Vector2(15f, -7f), new Vector2(398f, 211f), 30, TextAlignmentOptions.TopLeft);
        objective.color = new Color(1f, 0.97f, 0.89f);
        objective.enableAutoSizing = true;
        objective.fontSizeMin = 24f;
        objective.fontSizeMax = 32f;
        objective.overflowMode = TextOverflowModes.Ellipsis;
        controls = Label(root, "Weapon and Vehicle", new Vector2(0.5f, 0f), new Vector2(0f, 130f), new Vector2(1080f, 55f), 31, TextAlignmentOptions.Center);
        feedback = Label(root, "Messages", new Vector2(0.5f, 0f), new Vector2(0f, 188f), new Vector2(1090f, 58f), 31, TextAlignmentOptions.Center);
        var combatBackdrop = FarmPanelText.Backdrop(root, "Combat feedback backdrop");
        combatBackdrop.color = new Color(0.16f, 0.09f, 0.05f, 0.72f);
        Place(combatBackdrop.rectTransform, root, Vector2.zero, new Vector2(18f, 178f), new Vector2(455f, 50f));
        combat = Label(combatBackdrop.transform, "Combat feedback", Vector2.zero, new Vector2(14f, 0f),
            new Vector2(427f, 50f), 29f, TextAlignmentOptions.MidlineLeft);
        FarmPanelText.Style(combat, 23f, 29f, TextAlignmentOptions.MidlineLeft);
        combatBackdrop.gameObject.SetActive(false);

        mapPanel = Panel(root, "Minimap", Vector2.one, new Vector2(-18f, MinimapTop), new Vector2(158f, 167f));
        mapSupported = SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null;
        mapTexture = mapSupported ? new RenderTexture(512, 512, 16) : null;
        var view = new GameObject("Map Image", typeof(RawImage));
        view.transform.SetParent(mapPanel.transform, false);
        mapImage = (RectTransform)view.transform;
        mapImage.sizeDelta = new Vector2(148f, 148f);
        mapImage.anchoredPosition = new Vector2(0f, 3f);
        var image = view.GetComponent<RawImage>();
        image.texture = mapTexture;
        image.raycastTarget = false;
        mapMarker = Label(view.transform, "Player", new Vector2(0.5f, 0.5f), Vector2.zero,
            new Vector2(42f, 44f), 30, TextAlignmentOptions.Center);
        mapMarker.text = "^";
        mapLegend = Label(mapPanel.transform, "Map legend", new Vector2(0.5f, 0f), new Vector2(0f, 11f),
            new Vector2(765f, 92f), 29f, TextAlignmentOptions.Center);
        mapLegend.text = "西侧：鸡场、林地、池塘    东侧：温室、风车\n北侧：驿站    M / Esc 关闭地图";
        mapLegend.gameObject.SetActive(false);
        mapMarker.color = new Color(1f, 0.82f, 0.29f);
        var map = new GameObject("Map Camera", typeof(Camera));
        map.transform.SetParent(owner, false);
        mapCamera = map.GetComponent<Camera>();
        mapCamera.orthographic = true;
        mapCamera.orthographicSize = 17f;
        mapCamera.targetTexture = mapTexture;
        mapCamera.farClipPlane = 80f;
        mapCamera.cullingMask = Physics.DefaultRaycastLayers & ~LayerMask.GetMask("UI");
        mapCamera.clearFlags = CameraClearFlags.SolidColor;
        mapCamera.backgroundColor = new Color(0.14f, 0.20f, 0.15f);
        mapCamera.enabled = false;

        overlay = Panel(root, "Story Panel", new Vector2(0.5f, 0.5f), new Vector2(0f, -18f), new Vector2(1080f, 538f));
        ApplyBoard(overlay, "bulletin board");
        var storyInset = FarmPanelText.Backdrop(overlay.transform, "Story text backdrop");
        Place(storyInset.rectTransform, overlay.transform, new Vector2(0f, 1f),
            new Vector2(76f, -109f), new Vector2(928f, 378f));
        title = Label(overlay.transform, "Title", new Vector2(0f, 1f), new Vector2(78f, -31f), new Vector2(924f, 72f), 47, TextAlignmentOptions.Center);
        FarmPanelText.Style(title, 37f, 47f, TextAlignmentOptions.Center);
        title.color = FarmPanelText.Accent;
        body = Label(overlay.transform, "Body", new Vector2(0f, 1f), new Vector2(105f, -124f), new Vector2(870f, 308f), 39, TextAlignmentOptions.TopLeft);
        FarmPanelText.Style(body, 27f, 39f, TextAlignmentOptions.TopLeft);
        portrait = new GameObject("Portrait", typeof(RawImage)).GetComponent<RawImage>();
        portrait.texture = Resources.Load<Texture2D>("Image/UI/avatar_sheep");
        portrait.raycastTarget = false;
        Place((RectTransform)portrait.transform, overlay.transform, new Vector2(0f, 1f),
            new Vector2(100f, -133f), new Vector2(102f, 102f));
        prompt = Label(overlay.transform, "Prompt", new Vector2(0f, 0f), new Vector2(105f, 54f), new Vector2(870f, 49f), 35, TextAlignmentOptions.MidlineLeft);
        FarmPanelText.Style(prompt, 28f, 35f, TextAlignmentOptions.MidlineLeft);
        prompt.color = FarmPanelText.Accent;
        tracker.SetActive(false);
        mapPanel.SetActive(false);
    }

    public void PanelText(string heading, string copy, string keys)
    {
        overlay.SetActive(true);
        title.text = heading;
        portrait.enabled = heading == "玛丽";
        body.rectTransform.anchoredPosition = new Vector2(portrait.enabled ? 225f : 105f, -124f);
        body.rectTransform.sizeDelta = new Vector2(portrait.enabled ? 750f : 870f, 308f);
        body.text = copy;
        prompt.text = keys;
    }

    public void HidePanel() => overlay.SetActive(false);

    public void Notify(string message)
    {
        feedback.text = message;
        feedbackUntil = Time.unscaledTime + 3.6f;
    }

    public void ShowCombat(string message)
    {
        combat.text = message;
        combatUntil = Time.unscaledTime + 4f;
    }

    public void Refresh(Transform player, string quest, string controlsText, bool inGame)
    {
        tracker.SetActive(inGame);
        objective.text = quest;
        controls.text = controlsText;
        feedback.enabled = Time.unscaledTime < feedbackUntil;
        combat.transform.parent.gameObject.SetActive(inGame && Time.unscaledTime < combatUntil);
        var showMap = (inGame || expanded) && mapSupported;
        mapPanel.SetActive(showMap);
        mapCamera.enabled = showMap;
        if (showMap)
        {
            var center = new Vector3(5f, 0f, 73f);
            mapCamera.orthographicSize = expanded ? 89f : 17f;
            mapCamera.transform.SetPositionAndRotation((expanded ? center : player.position) + Vector3.up * 36f,
                Quaternion.Euler(90f, expanded ? 0f : player.eulerAngles.y, 0f));
            var delta = player.position - center;
            mapMarker.rectTransform.anchoredPosition = expanded ? new Vector2(delta.x, delta.z) * (460f / 178f) : Vector2.zero;
            mapMarker.rectTransform.localRotation = Quaternion.Euler(0f, 0f, expanded ? -player.eulerAngles.y : 0f);
        }
    }

    public void ExpandMap(bool show)
    {
        expanded = show;
        var rect = (RectTransform)mapPanel.transform;
        rect.anchorMin = rect.anchorMax = rect.pivot = show ? new Vector2(0.5f, 0.5f) : Vector2.one;
        rect.sizeDelta = show ? new Vector2(800f, 632f) : new Vector2(158f, 167f);
        rect.anchoredPosition = show ? Vector2.zero : new Vector2(-18f, MinimapTop);
        mapImage.sizeDelta = show ? new Vector2(460f, 460f) : new Vector2(148f, 148f);
        mapImage.anchoredPosition = show ? new Vector2(0f, 49f) : new Vector2(0f, 3f);
        mapLegend.gameObject.SetActive(show);
        if (show)
            rect.SetAsLastSibling();
    }

    public void Release()
    {
        if (mapCamera != null)
            mapCamera.targetTexture = null;
        if (mapTexture != null)
        {
            mapTexture.Release();
            Object.Destroy(mapTexture);
        }
    }

    private static void ApplyBoard(GameObject target, string name)
    {
        var sprite = Resources.Load<Sprite>("Image/UI/" + name);
        if (sprite == null)
            return;
        var image = target.GetComponent<Image>();
        image.sprite = sprite;
        image.color = Color.white;
    }

    private GameObject Panel(Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size)
    {
        var panel = new GameObject(name, typeof(Image));
        var image = panel.GetComponent<Image>();
        image.color = new Color(0.075f, 0.13f, 0.10f, 0.96f);
        image.raycastTarget = false;
        Place((RectTransform)panel.transform, parent, anchor, position, size);
        return panel;
    }

    private TMP_Text Label(Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size,
        float sizeInPoints, TextAlignmentOptions alignment)
    {
        var label = new GameObject(name, typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        Place(label.rectTransform, parent, anchor, position, size);
        label.font = font;
        label.fontSize = sizeInPoints;
        label.color = new Color(1f, 0.96f, 0.86f);
        label.raycastTarget = false;
        label.alignment = alignment;
        return label;
    }

    private static void Place(RectTransform rect, Transform parent, Vector2 anchor, Vector2 position, Vector2 size)
    {
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }
}
