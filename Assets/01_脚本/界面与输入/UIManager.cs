using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class UIManager : MonoBehaviour
{

    public static UIManager Instance;

    public Canvas canvas; //画布
    public Camera uiCamera;

    private void Awake()
    {
        Instance = this;

        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        uiCamera = GameObject.Find("UICamera").GetComponent<Camera>();
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //飘号 ~
        if (AgricultureSystem.Instance?.HasOpenStation != true && FarmCampaign.Instance?.BlocksWorldInput != true
            && FarmCampaign.Instance?.IsDriving != true && Input.GetKeyDown(KeyCode.BackQuote))
        {
            if (Cursor.visible == true)
            {
                HideCursor();
            }
            else
            {
                OpenCursor();
            }
        }
    }

    //隐藏并锁定指针
    public void HideCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }


    //解锁并显示指针
    public void OpenCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }





}
