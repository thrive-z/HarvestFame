using UnityEngine;

public sealed class GreenhouseDoors : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform firstLeaf;
    [SerializeField] private Transform secondLeaf;
    [SerializeField] private float swing = 92f;
    private Quaternion firstRest;
    private Quaternion secondRest;
    private float angle;
    private bool open;

    private void Awake()
    {
        firstRest = firstLeaf.localRotation;
        secondRest = secondLeaf.localRotation;
    }

    public bool CanInteract() => firstLeaf != null && secondLeaf != null;
    public string GetInteractTip() => open ? "F · 关闭温室门" : "F · 打开温室门";
    public void Interact(GameObject interactor)
    {
        open = !open;
        AgricultureSystem.Instance.PlaySound("储物箱");
    }

    private void Update()
    {
        angle = Mathf.MoveTowards(angle, open ? swing : 0f, Time.deltaTime * 210f);
        firstLeaf.localRotation = firstRest * Quaternion.Euler(0f, angle, 0f);
        secondLeaf.localRotation = secondRest * Quaternion.Euler(0f, -angle, 0f);
    }
}
