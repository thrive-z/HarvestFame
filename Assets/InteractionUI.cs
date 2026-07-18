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
        textGo = text.gameObject;

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
