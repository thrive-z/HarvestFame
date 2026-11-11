using System.Collections.Generic;
using UnityEngine;

public class BoxEntity : MonoBehaviour, IInteractable
{
    public List<ItemData> itemDataList = new();
    public bool isOpen;

    private void Update()
    {
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
            StorageBox.Instance.CloseActiveBox();
    }

    public void OpenBox()
    {
        if (!isOpen)
            isOpen = StorageBox.Instance.OpenBox(this);
    }

    public string GetInteractTip() => "按 F 打开储物箱";

    public bool CanInteract() => !isOpen;

    public void Interact(GameObject interactor) => OpenBox();
}
