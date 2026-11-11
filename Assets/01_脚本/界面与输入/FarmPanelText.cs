using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal static class FarmPanelText
{
    public static readonly Color BackdropColor = new(0.16f, 0.09f, 0.05f, 0.98f);
    public static readonly Color QuantityBackdropColor = new(0.26f, 0.15f, 0.08f, 0.86f);
    public static readonly Color Foreground = new(1f, 0.97f, 0.89f);
    public static readonly Color Accent = new(1f, 0.82f, 0.48f);

    public static Image Backdrop(Transform parent, string name)
    {
        var image = new GameObject(name, typeof(Image)).GetComponent<Image>();
        image.transform.SetParent(parent, false);
        image.color = BackdropColor;
        image.raycastTarget = false;
        return image;
    }

    public static TMP_Text Label(Transform parent, string name)
    {
        var label = new GameObject(name, typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        label.transform.SetParent(parent, false);
        label.font = Resources.Load<TMP_FontAsset>("Font/AlibabaPuHuiTi-3-55-Regular SDF");
        return label;
    }

    public static void Inset(RectTransform rect, Vector2 minAnchor, Vector2 maxAnchor, Vector2 min, Vector2 max)
    {
        rect.anchorMin = minAnchor;
        rect.anchorMax = maxAnchor;
        rect.offsetMin = min;
        rect.offsetMax = max;
    }

    public static void Style(TMP_Text label, float minimum, float maximum, TextAlignmentOptions alignment)
    {
        label.color = Foreground;
        label.fontSizeMin = minimum;
        label.fontSizeMax = maximum;
        label.enableAutoSizing = true;
        label.overflowMode = TextOverflowModes.Ellipsis;
        label.margin = Vector4.zero;
        label.alignment = alignment;
        label.raycastTarget = false;
    }
}
