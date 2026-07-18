using UnityEngine;

//锄头
public class HoeBehavior : IToolBehavior
{
    public ToolModule owner;

    //装备
    void IToolBehavior.OnEquip(ToolModule owner, ToolInstance tool)
    {
        this.owner = owner;

        // 添加模型
        Debug.Log("添加锄头模型");

        //添加动画

    }

    //使用
    bool IToolBehavior.Use(ToolInstance tool)
    {
        return true;
    }
}
