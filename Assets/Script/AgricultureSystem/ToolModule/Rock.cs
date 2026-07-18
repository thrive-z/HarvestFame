using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 石头
/// </summary>
public class Rock : MonoBehaviour,IToolInteractable
{
    private int hp = 3;

    

    public ToolUseResult OnToolUsed(ToolContext context)
    {
        //判断工具类型
        if (context.toolData.tool_type != ToolType.Pickaxe)
        {
            return ToolUseResult.Invalid;
        }

        hp--;
        Debug.Log("采石");

        //播放动画 音效 特效

        //判断树是否砍死
        if (hp <= 0)
        {
            Debug.Log("石头采空");
            //todo 添加石头到背包.


            Destroy(gameObject);

        }

        return ToolUseResult.Success;
    }


}
