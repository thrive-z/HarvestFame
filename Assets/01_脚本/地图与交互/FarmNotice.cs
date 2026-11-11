using UnityEngine;

public sealed class FarmNotice : MonoBehaviour, IInteractable
{
    [SerializeField] private string title;
    [TextArea, SerializeField] private string message;

    public bool CanInteract() => true;
    public string GetInteractTip() => $"F · 查看{title}";
    public void Interact(GameObject interactor) => AgricultureSystem.Instance.Notify(message);
}
