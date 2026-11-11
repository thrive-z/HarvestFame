using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 物品详情提示
/// </summary>
public class Tip : UIContainer
{
    public TMP_Text _title;
    public TMP_Text _content;

    public RectTransform _rt;

    private RectTransform hoveredSlot;

    private void Awake()
    {
        _title = transform.Find("Title").GetComponent<TMP_Text>();
        _content = transform.Find("Content").GetComponent<TMP_Text>();
        _cg = GetComponent<CanvasGroup>();
        _rt = GetComponent<RectTransform>();
        _rt.sizeDelta = new Vector2(620f, 296f);
        var backdrop = FarmPanelText.Backdrop(transform, "Item details backdrop");
        FarmPanelText.Inset(backdrop.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        backdrop.transform.SetAsFirstSibling();
        FarmPanelText.Inset(_title.rectTransform, Vector2.up, Vector2.one,
            new Vector2(20f, -87f), new Vector2(-20f, -12f));
        FarmPanelText.Style(_title, 43f, 52f, TextAlignmentOptions.MidlineLeft);
        _title.color = FarmPanelText.Accent;
        FarmPanelText.Inset(_content.rectTransform, Vector2.zero, Vector2.one,
            new Vector2(20f, 16f), new Vector2(-20f, -94f));
        FarmPanelText.Style(_content, 35f, 43f, TextAlignmentOptions.TopLeft);

    }






    private void LateUpdate()
    {
        if (isOpen && hoveredSlot != null)
            PlaceBesideSlot();
    }

    public void SetUI(ItemData itemData, RectTransform slot)
    {
        if (itemData == null)
            return;

        _title.text = itemData.name;
        _content.text = itemData.describe;
        hoveredSlot = slot;
        PlaceBesideSlot();
    }

    private void PlaceBesideSlot()
    {
        const float gap = 16f;
        var panel = (RectTransform)_rt.parent;
        var canvas = (RectTransform)GetComponentInParent<Canvas>().rootCanvas.transform;
        var canvasMin = panel.InverseTransformPoint(canvas.TransformPoint(canvas.rect.min));
        var canvasMax = panel.InverseTransformPoint(canvas.TransformPoint(canvas.rect.max));
        var bounds = Rect.MinMaxRect(canvasMin.x, canvasMin.y, canvasMax.x, canvasMax.y);
        var size = _rt.rect.size;
        var slotMin = (Vector2)panel.InverseTransformPoint(hoveredSlot.TransformPoint(hoveredSlot.rect.min));
        var slotMax = (Vector2)panel.InverseTransformPoint(hoveredSlot.TransformPoint(hoveredSlot.rect.max));
        var left = slotMax.x + gap;
        var top = slotMin.y - gap;
        if (left + size.x > bounds.xMax && top - size.y < bounds.yMin)
            left = slotMin.x - gap - size.x;
        left = Mathf.Clamp(left, bounds.xMin, Mathf.Max(bounds.xMin, bounds.xMax - size.x));
        if (top - size.y < bounds.yMin && left < slotMax.x && left + size.x > slotMin.x)
            top = slotMax.y + gap + size.y;
        top = Mathf.Clamp(top, Mathf.Min(bounds.yMax, bounds.yMin + size.y), bounds.yMax);

        _rt.anchoredPosition = new Vector2(left + size.x * _rt.pivot.x,
            top - size.y * (1f - _rt.pivot.y)) - panel.rect.center;
    }
}
