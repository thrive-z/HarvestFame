using System.Collections.Generic;
using Newtonsoft.Json;

public partial class ItemData
{
    public struct ToolState
    {
        public int durability;
        public int water;
    }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public List<ToolState> spare_tools;

    public ItemData CopyStackRange(int start, int count)
    {
        var copy = new ItemData(this) { current_stack = count, spare_tools = null };
        if (item_type == ItemType.Tool)
        {
            var first = GetToolState(start);
            copy.current_durable = first.durability;
            copy.water_remaining = first.water;
            if (count > 1)
            {
                copy.spare_tools = new List<ToolState>(count - 1);
                for (var i = 1; i < count; i++)
                    copy.spare_tools.Add(GetToolState(start + i));
            }
        }
        return copy;
    }

    public void AppendStackRange(ItemData source, int start, int count)
    {
        if (item_type != ItemType.Tool)
            return;

        spare_tools ??= new List<ToolState>();
        while (spare_tools.Count < current_stack - 1)
            spare_tools.Add(GetToolState(0));
        for (var i = 0; i < count; i++)
            spare_tools.Add(source.GetToolState(start + i));
    }

    public void TrimSpareTools(int count)
    {
        var keep = System.Math.Max(0, count - 1);
        if (spare_tools != null && spare_tools.Count > keep)
            spare_tools.RemoveRange(keep, spare_tools.Count - keep);
    }

    public bool SelectUsableSpareTool(int maximumDurability)
    {
        if (spare_tools == null)
            return false;

        var index = spare_tools.FindIndex(tool => tool.durability != 0);
        if (index < 0)
            return false;

        var replacement = spare_tools[index];
        spare_tools[index] = GetToolState(0);
        current_durable = replacement.durability < 0 ? maximumDurability : replacement.durability;
        water_remaining = replacement.water;
        return true;
    }

    private ToolState GetToolState(int index) => index > 0 && spare_tools != null && index <= spare_tools.Count
        ? spare_tools[index - 1]
        : new ToolState { durability = current_durable, water = water_remaining };
}
