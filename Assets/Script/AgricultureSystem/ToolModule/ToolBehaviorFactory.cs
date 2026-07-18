using UnityEngine;

/// <summary>
/// 工具行为工厂
/// </summary>
public static class ToolBehaviorFactory
{
    public static IToolBehavior Create(ToolType type)
    {
        switch (type)
        {
            //如果是锄头
            case ToolType.Hoe:
                return new HoeBehavior();
            case ToolType.Axe:
                return new AxeBehavior();
            case ToolType.Pickaxe:
                return new PickaxeBehavior();

            // Hoe, //锄头
            // Sickle, //镰刀
            // WaterCan, //水壶
            // FishingRod, //鱼竿

            default:
                Debug.Log("没找到当前类型" + type);
                return null;
        }
    }
}