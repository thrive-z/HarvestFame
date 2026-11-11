using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InteractionUI : MonoBehaviour
{
    public static InteractionUI Instance;

    public TMP_Text text;
    public GameObject textGo;

    private void Awake()
    {
        Instance = this;
        text = GetComponentInChildren<TMP_Text>();
        var banner = FarmPanelText.Backdrop(transform, "Interaction prompt backdrop");
        var background = banner.color;
        background.a = 0.47f;
        banner.color = background;
        FarmPanelText.Inset(banner.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(-720f, -155f), new Vector2(720f, -77f));
        text.rectTransform.SetParent(banner.transform, false);
        FarmPanelText.Inset(text.rectTransform, Vector2.zero, Vector2.one,
            new Vector2(20f, 0f), new Vector2(-20f, 0f));
        // This canvas uses a 1920-wide reference, while the task board uses 1280.
        FarmPanelText.Style(text, 38f, 48f, TextAlignmentOptions.Center);
        text.alpha = 0.84f;
        textGo = banner.gameObject;

        Close();
    }

    public void Open(string tip )
    {
        textGo.SetActive(true);
        text.text = tip;
    }

    public void Close()
    {
        text.text = "";
        textGo.SetActive(false);
    }

}
