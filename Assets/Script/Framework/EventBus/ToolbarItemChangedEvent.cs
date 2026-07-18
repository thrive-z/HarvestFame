/// <summary>
/// 工具栏切换事件
/// </summary>
public class ToolbarItemChangedEvent
{
    public ItemData item { get; }

    public ToolbarItemChangedEvent(ItemData itemData)
    {
        item = itemData;
    }
}