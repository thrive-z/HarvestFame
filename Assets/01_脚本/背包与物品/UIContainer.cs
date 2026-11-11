using UnityEngine;

/// <summary>
/// 容器的显示和隐藏
/// </summary>
public class UIContainer : MonoBehaviour
{
    public CanvasGroup _cg;
    public bool isOpen = false; //是否打开

    public void SwitchContainer()
    {
        if (isOpen)
        {
            CloseContainerAndCursor();
        }
        else
        {
            OpenContainerAndCursor();
        }
    }

    //打开容器还有光标
    public void OpenContainerAndCursor()
    {
        OpenContainer();


        //鼠标解锁
        UIManager.Instance.OpenCursor();

    }

    //关闭容器还有光标
    public void CloseContainerAndCursor()
    {

        CloseContainer();

        //鼠标关闭
        UIManager.Instance.HideCursor();
    }

    //打开容器
    public void OpenContainer(bool interactable = true)
    {
        if (_cg == null)
        {
            _cg = GetComponent<CanvasGroup>();
        }

        _cg.alpha = 1;
        _cg.interactable = interactable;
        _cg.blocksRaycasts = interactable;

        isOpen = true;

    }

    //关闭容器
    public void CloseContainer(bool interactable = false)
    {

        if (_cg == null)
        {
            _cg = GetComponent<CanvasGroup>();
        }

        _cg.alpha = 0;
        _cg.interactable = interactable;
        _cg.blocksRaycasts = interactable;

        isOpen = false;

    }
}