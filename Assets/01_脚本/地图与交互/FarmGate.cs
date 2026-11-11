using UnityEngine;

public sealed class FarmGate : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform leaf;
    [SerializeField] private float openAngle = -105f;

    private bool opened;
    private float angle;

    public bool CanInteract() => leaf != null;
    public string GetInteractTip() => opened ? "F · 关上鸡场门" : "F · 打开鸡场门";

    public void Interact(GameObject interactor)
    {
        opened = !opened;
        AgricultureSystem.Instance.PlaySound("储物箱");
    }

    private void Update()
    {
        if (leaf == null)
            return;

        var target = opened ? openAngle : 0f;
        angle = Mathf.MoveTowards(angle, target, 240f * Time.deltaTime);
        leaf.localRotation = Quaternion.Euler(0f, angle, 0f);
    }
}
