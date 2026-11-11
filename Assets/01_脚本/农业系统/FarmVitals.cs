using System.Collections.Generic;
using UnityEngine;

public sealed class FarmVitals : MonoBehaviour
{
    private const float Maximum = 100f;
    private List<FoodEffect> effects;

    public float Stamina { get; private set; } = Maximum;
    public float Satiety { get; private set; } = Maximum;

    public void Initialize()
    {
        effects = AgricultureSystem.ReadData<List<FoodEffect>>("buff");
    }

    private void Update()
    {
        Satiety = Mathf.Max(0f, Satiety - 0.20f * Time.deltaTime);
        if (Satiety > 0f)
            Stamina = Mathf.Min(Maximum, Stamina + 1.8f * Time.deltaTime);
    }

    public void Restore(float stamina, float satiety)
    {
        Stamina = Mathf.Clamp(stamina, 0f, Maximum);
        Satiety = Mathf.Clamp(satiety, 0f, Maximum);
    }

    public void SpendStamina(int amount)
    {
        Stamina = Mathf.Max(0f, Stamina - amount);
    }

    public void TryEat(InventorySlot slot)
    {
        var item = slot.bindItem.itemData;
        var farm = AgricultureSystem.Instance;
        var canEat = false;
        var hasEffect = false;
        foreach (var effect in effects)
        {
            if (effect.item_id != item.id || effect.buff_type != "Instant")
                continue;

            hasEffect = true;
            if (effect.vital_type == "Satiety" && Satiety < Maximum)
            {
                Satiety = Mathf.Min(Maximum, Satiety + effect.value);
                canEat = true;
            }
            else if (effect.vital_type == "Stamina" && Stamina < Maximum)
            {
                Stamina = Mathf.Min(Maximum, Stamina + effect.value);
                canEat = true;
            }
        }

        if (hasEffect && FarmCampaign.Instance?.HealFromFood() == true)
            canEat = true;

        if (!canEat)
        {
            farm.Notify(item.item_type == ItemType.Food ? "当前无法从这份食物获得恢复。" : "请选择可食用的物品。");
            return;
        }

        slot.SetItemAmount(item.current_stack - 1);
        farm.Notify($"食用了{item.name}。");
    }

    private sealed class FoodEffect
    {
        public int item_id;
        public string vital_type;
        public float value;
        public string buff_type;
    }
}
