using UnityEngine;

/// <summary>
/// 斧头使用
/// </summary>
public class AxeBehavior : IToolBehavior
{
    public ToolModule owner;


    public void OnEquip(ToolModule owner, ToolInstance tool)
    {
        this.owner = owner;

    }

    public bool Use(ToolInstance tool)
    {
        Ray ray = Camera.main.ScreenPointToRay(
            new Vector3(Screen.width / 2f, Screen.height / 2f, 0));

        if (!Physics.Raycast(ray, out RaycastHit hit,
                tool.toolData.interact_distance,
                LayerMask.GetMask("Env")))
        {
            Debug.Log("没有任何工具可交互对象");
            return false;
        }

        //尝试获取 工具交互接口
        if (!hit.collider.TryGetComponent(out IToolInteractable target))
        {
            Debug.Log("没有找到工具可交互对象");
            return false;
        }

        //构造工具上下文
        ToolContext contenxt = new ToolContext()
        {
            toolData = tool.toolData,
            toolId = tool.toolData.item_id,
            user = owner.gameObject,
            hitPoint = hit.point
        };

        //调用物体的交互方法
        ToolUseResult toolUseResult = target.OnToolUsed(contenxt);
        //判断使用结果
        if (toolUseResult != ToolUseResult.Success)
        {
            Debug.Log("该物体不可正常交互");
            return false;
        }

        return true;


    }
}