using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour, IToolInteractable
{
    public int hp = 3;

    //被使用的方法
    public ToolUseResult OnToolUsed(ToolContext context)
    {
        if (context.toolData.tool_type != ToolType.Axe)
        {
            return ToolUseResult.Invalid;
        }

        hp--;
        Debug.Log("砍树");

        //播放动画 音效 特效

        //判断树是否砍死
        if (hp <= 0)
        {
            Debug.Log("树倒了");
            //todo 添加木头到背包.
            Destroy(gameObject);

        }
        return ToolUseResult.Success;
    }
}
