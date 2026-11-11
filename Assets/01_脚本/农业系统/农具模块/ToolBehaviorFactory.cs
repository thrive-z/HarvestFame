public static class ToolBehaviorFactory
{
    public static IToolBehavior Create(ToolType type)
    {
        switch (type)
        {
            case ToolType.Hoe:
            case ToolType.WaterCan:
                return new HoeBehavior();
            case ToolType.Axe:
            case ToolType.Pickaxe:
            case ToolType.Sickle:
                return new GatherBehavior();
            default:
                return null;
        }
    }
}
