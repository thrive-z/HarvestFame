public sealed class GatherBehavior : IToolBehavior
{
    public void OnEquip(ToolModule owner, ToolInstance tool) { }

    public bool Use(ToolInstance tool)
    {
        var player = Player.Instance;
        if (player == null || !player.GetComponent<PlayerInteraction>().TryGetTarget(out var hit, tool.toolData.interact_distance))
            return false;
        var target = hit.collider.GetComponentInParent<IToolInteractable>();
        if (target == null)
            return false;

        return target.OnToolUsed(new ToolContext
        {
            toolData = tool.toolData,
            toolId = tool.toolData.item_id,
            user = player.gameObject,
            hitPoint = hit.point
        }) == ToolUseResult.Success;
    }
}
