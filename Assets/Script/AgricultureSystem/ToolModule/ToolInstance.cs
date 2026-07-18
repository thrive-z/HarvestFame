using System;

/// <summary>
/// 工具实例
/// </summary>
public class ToolInstance
{
    public ToolData toolData; //    工具数据
    public IToolBehavior behavior; //工具行为

    public ToolInstance(ToolData toolData)
    {
        this.toolData = toolData;
        behavior = ToolBehaviorFactory.Create(toolData.tool_type);
    }

    //装备
    public void OnEquip(ToolModule owner)
    {
        behavior.OnEquip(owner,this);
    }

    public void Use()
    {
        //判断是否有体力 -> 体力系统 -> 响应式变量
        //判断是否有耐久 -> 状态管理器


        bool used_result =  behavior.Use(this);

        //判断使用结果是否成功
        //可能扣减体力
        //可能耐久


    }
}
