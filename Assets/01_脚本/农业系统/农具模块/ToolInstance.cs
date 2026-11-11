public class ToolInstance
{
    public readonly ToolData toolData;
    public readonly IToolBehavior behavior;

    public ToolInstance(ToolData data)
    {
        toolData = data;
        behavior = ToolBehaviorFactory.Create(data.tool_type);
    }

    public void OnEquip(ToolModule owner) => behavior.OnEquip(owner, this);
    public bool Use() => behavior.Use(this);
}
