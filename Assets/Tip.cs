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

    public float moveSpeed = 200f;//平滑移动速度

    public Vector2 offset = new Vector2(200,0); //偏移量 防止挡住图标

    private void Awake()
    {
        _title = transform.Find("Title").GetComponent<TMP_Text>();
        _content = transform.Find("Content").GetComponent<TMP_Text>();
        _cg = GetComponent<CanvasGroup>();
        _rt = GetComponent<RectTransform>();

    }






    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isOpen)
        {
            //鼠标位置转为canvas的坐标
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                UIManager.Instance.canvas.GetComponent<RectTransform>(),
                Input.mousePosition,
                UIManager.Instance.uiCamera,
                out Vector2 canvasLocalPosition
            );


            _rt.anchoredPosition = Vector2.Lerp(
                _rt.anchoredPosition,
                canvasLocalPosition + offset,
                Time.deltaTime * moveSpeed
                );
        }
    }

    public void SetUI(ItemData itemData)
    {
        if (itemData != null)
        {
            _title.text = itemData.name;
            _content.text = itemData.describe;

        }
    }


}
