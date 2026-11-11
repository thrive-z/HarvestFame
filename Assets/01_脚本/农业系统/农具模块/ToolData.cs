
/// <summary>
/// 工具数据
/// </summary>
public class ToolData
{
    public int tool_id; //工具id
    public int item_id; //对应的itemid

    public ToolType tool_type; //工具类型
    public bool use_stamina; // 是否消耗体力
    public int stamina_cost; //体力消耗

    public bool use_durability; //是否消耗耐久
    public int max_durability; //最大耐久值
    public int durability_cost; //每次使用消耗的耐久值

    public float interact_distance; //交互距离

    
}
