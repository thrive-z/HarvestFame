
//工具行为接口
public interface IToolBehavior
{
    //装备该工具
    void OnEquip(ToolModule owner, ToolInstance tool);
    //使用
    bool Use(ToolInstance tool);
}