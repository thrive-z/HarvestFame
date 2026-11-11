using UnityEngine;

public sealed class HarvestNode : MonoBehaviour, IToolInteractable, IInteractable
{
    [SerializeField] private ToolType tool = ToolType.Axe;
    [SerializeField] private int itemId = 102;
    [SerializeField] private int yieldCount = 3;
    [SerializeField] private int hits = 3;
    [SerializeField] private float regrowSeconds = 65f;
    [SerializeField] private GameObject model;
    [SerializeField] private GameObject depletedModel;

    private Collider[] hitboxes;
    private int remainingHits;
    private float readyAt;

    private void Awake()
    {
        remainingHits = hits;
        hitboxes = GetComponentsInChildren<Collider>();
    }

    private void Update()
    {
        if (remainingHits > 0 || Time.time < readyAt)
            return;
        remainingHits = hits;
        SetAvailable(true);
    }

    public ToolUseResult OnToolUsed(ToolContext context)
    {
        if (context.toolData.tool_type != tool || remainingHits <= 0)
            return ToolUseResult.Invalid;

        var farm = AgricultureSystem.Instance;
        farm.PlaySound(tool == ToolType.Axe ? "伐木" : tool == ToolType.Pickaxe ? "采石" : "割草");
        if (--remainingHits > 0)
            return ToolUseResult.Success;

        var data = LoadManager.instance.itemDatasDict[itemId];
        InventorySystem.Instance.CreateItemInWorld(transform.position + Vector3.right * 0.65f, data, yieldCount);
        readyAt = Time.time + regrowSeconds;
        SetAvailable(false);
        return ToolUseResult.Success;
    }

    private void SetAvailable(bool available)
    {
        model.SetActive(available);
        if (depletedModel != null)
            depletedModel.SetActive(!available);
        foreach (var hitbox in hitboxes)
            hitbox.enabled = available;
    }

    public bool CanInteract() => false;
    public void Interact(GameObject interactor) { }
    public string GetInteractTip() => tool == ToolType.Axe ? "用斧头砍伐，按左键"
        : tool == ToolType.Pickaxe ? "用镐采石，按左键" : "用镰刀割草，按左键";
}
