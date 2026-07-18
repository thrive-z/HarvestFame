using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 工具模块
/// </summary>
public class ToolModule : MonoBehaviour
{
    public ToolInstance currentTool; //记录当前的工具

    //工具数据的字典
    public Dictionary<int,ToolData> toolDatasDict = new Dictionary<int,ToolData>();

    private void Awake()
    {
        //拿到工具json
        TextAsset text = Resources.Load<TextAsset>("Data/tool");
        List<ToolData>  toolDatas = JsonConvert.DeserializeObject<List<ToolData>>(text.text);

        foreach (ToolData toolData in toolDatas)
        {
            toolDatasDict.Add(toolData.item_id, toolData);
        }

    }


    //监听事件
    private void OnEnable()
    {
        EventBus.Subscribe<ToolbarItemChangedEvent>(OnToolbarItemChanged);
    }
    private void OnDisable()
    {
        EventBus.Unsubscribe<ToolbarItemChangedEvent>(OnToolbarItemChanged);
    }

    private void Update()
    {
        //如果当前有工具 并按了左键
        if (currentTool != null && Input.GetMouseButtonDown(0))
        {
            UseCurrentTool();
        }
    }

    //使用当前工具
    private void UseCurrentTool()
    {
        currentTool.Use();
    }


    //监听到该事件的处理方法
    private void OnToolbarItemChanged(ToolbarItemChangedEvent e)
    {
        //如果传过来的是空，卸载当前工具
        if (e.item == null || e.item.item_type != ItemType.Tool)
        {
            //没有操作

            //如果有工具，卸载当前工具
            //如果当前有工具 则卸载
            if (currentTool != null)
            {
                UnEquipTool();
            }

            return;
        }

        Debug.Log($"切换到工具 {e.item.item_sub_type}");

        //如果当前有工具 则卸载
        if (currentTool != null)
        {
            UnEquipTool();
        }


        //装备工具
        ToolData toolData = toolDatasDict[e.item.id];//拿到当前的 toolData
        //构造工具实例
        ToolInstance ins = new ToolInstance(toolData);
        //装备方法
        EquipTool(ins);

    }

    //装备工具
    private void EquipTool(ToolInstance ins)
    {
       currentTool = ins; //记录当前工具实例
        currentTool.OnEquip(this);
    }

    //卸载装备方法
    public void UnEquipTool()
    {
        Debug.Log("卸载装备" + currentTool.toolData.tool_type);
        currentTool = null;

        //卸载动画
        //卸载声音
    }
   
}

