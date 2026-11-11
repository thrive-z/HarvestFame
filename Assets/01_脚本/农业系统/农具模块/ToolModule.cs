using System.Collections.Generic;
using UnityEngine;

public class ToolModule : MonoBehaviour
{
    private readonly Dictionary<int, ToolData> tools = new();
    private ItemData equippedItem;
    [SerializeField] private GameObject[] heldTools;
    public ToolInstance currentTool { get; private set; }

    private void Awake()
    {
        foreach (var tool in AgricultureSystem.ReadData<List<ToolData>>("tool"))
            tools.Add(tool.item_id, tool);
    }

    private void Update()
    {
        var slot = InventorySystem.Instance.toolbar.SelectedSlot;
        var item = slot.bindItem?.itemData;
        if (!ReferenceEquals(item, equippedItem))
            Equip(item);

        if (!AgricultureSystem.WorldInputEnabled || FarmCampaign.Instance?.IsDriving == true
            || !Input.GetMouseButtonDown(0) || item == null)
            return;

        var farm = AgricultureSystem.Instance;
        if (item.item_type == ItemType.Seed)
        {
            farm?.PlantSelected(slot);
            return;
        }

        if (currentTool == null)
            return;

        var data = currentTool.toolData;
        var vitals = farm?.Vitals;
        if (data.use_durability && item.current_durable <= 0 && !item.SelectUsableSpareTool(data.max_durability))
        {
            farm?.Notify("这件工具已经损坏。");
            return;
        }
        if (data.use_stamina && vitals != null && vitals.Stamina < data.stamina_cost)
        {
            farm.Notify("体力不足。");
            return;
        }

        if (!currentTool.Use())
            return;

        if (data.use_durability)
            item.current_durable = Mathf.Max(0, item.current_durable - data.durability_cost);
        if (data.use_stamina)
            vitals?.SpendStamina(data.stamina_cost);
    }

    private void Equip(ItemData item)
    {
        equippedItem = item;
        currentTool = null;
        if (heldTools != null)
        {
            for (var i = 0; i < heldTools.Length; i++)
            {
                if (heldTools[i] != null)
                    heldTools[i].SetActive(item?.item_type == ItemType.Tool
                        && tools.TryGetValue(item.id, out var equippedData) && (int)equippedData.tool_type == i);
            }
        }
        if (item?.item_type != ItemType.Tool || !tools.TryGetValue(item.id, out var data))
            return;

        if (data.use_durability && item.current_durable < 0)
            item.current_durable = data.max_durability;
        var tool = new ToolInstance(data);
        if (tool.behavior == null)
            return;

        currentTool = tool;
        tool.OnEquip(this);
    }
}
