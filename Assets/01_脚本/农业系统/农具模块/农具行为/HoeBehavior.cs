public sealed class HoeBehavior : IToolBehavior
{
    public void OnEquip(ToolModule owner, ToolInstance tool) { }

    public bool Use(ToolInstance tool)
    {
        var farm = AgricultureSystem.Instance;
        return farm != null && farm.TryUsePlot(tool.toolData.tool_type, tool.toolData.interact_distance);
    }
}
