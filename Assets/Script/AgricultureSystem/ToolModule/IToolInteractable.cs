using UnityEngine;

//工具可交互接口，由可被工具交互的物品实现
public interface IToolInteractable 
{
    ToolUseResult OnToolUsed(ToolContext context);
}

//工具上下文
public class ToolContext
{
    public ToolData toolData; //工具数据
    public int toolId; //工具id
    public GameObject user; //使用者
    public Vector3 hitPoint; //命中点

}

//工具使用结果
public enum ToolUseResult
{
    None, //没发生任何事 斧头砍空气
    Invalid, //命中目标 但无效， 斧头砍石头
    Success, //使用成功 斧头砍树
}