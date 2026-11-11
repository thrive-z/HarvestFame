using UnityEngine;

public sealed class FarmSpot : MonoBehaviour, IInteractable
{
    public enum Service { Market, Fishing, Sprinkler, Rest }
    [SerializeField] private Service service;
    [SerializeField] private FarmPlot[] wateredPlots;
    [SerializeField] private ParticleSystem spray;
    private float biteAt = -1f;
    private bool announced;

    public bool CanInteract() => true;

    public string GetInteractTip()
    {
        switch (service)
        {
            case Service.Market: return "F · 打开农场商店";
            case Service.Rest: return "F · 在营地休息";
            case Service.Sprinkler: return "F · 用花洒给喷灌器加水";
            default: return biteAt < 0f ? "F · 在码头抛竿" : Time.time >= biteAt ? "F · 收线" : "等待鱼咬钩";
        }
    }

    public void Interact(GameObject interactor)
    {
        var farm = AgricultureSystem.Instance;
        var inventory = InventorySystem.Instance;
        var item = inventory.toolbar.SelectedSlot.bindItem?.itemData;
        switch (service)
        {
            case Service.Market:
                Shop.Instance.OpenMarket();
                break;
            case Service.Rest:
                farm.Vitals.Restore(100f, Mathf.Max(25f, farm.Vitals.Satiety));
                FarmCampaign.Instance?.HealFromFood();
                farm.Notify("休息好了，恢复了体力。");
                break;
            case Service.Sprinkler:
                if (item?.item_sub_type != ItemSubType.WaterCan || FarmStation.WaterRemaining(item) < 2)
                {
                    farm.Notify("选中花洒，至少需要两格水。");
                    return;
                }
                var wet = 0;
                foreach (var plot in wateredPlots)
                {
                    if (plot.Water())
                        wet++;
                }
                if (wet == 0)
                {
                    farm.Notify("这片地暂时不需要浇水。");
                    return;
                }
                item.water_remaining = FarmStation.WaterRemaining(item) - 2;
                if (spray != null)
                    spray.Play(true);
                farm.Notify($"已浇水 {wet} 块地。");
                break;
            case Service.Fishing:
                Fish(item, inventory, farm);
                break;
        }
    }

    private void Fish(ItemData item, InventorySystem inventory, AgricultureSystem farm)
    {
        if (item?.item_sub_type != ItemSubType.FishingRod)
        {
            farm.Notify("先在工具栏选中鱼竿。");
            return;
        }
        if (biteAt < 0f)
        {
            biteAt = Time.time + Random.Range(2f, 4.5f);
            announced = false;
            farm.PlaySound("Water");
            farm.Notify("鱼线已抛出，鱼咬钩后按 F 收线。");
            return;
        }
        if (Time.time < biteAt)
        {
            farm.Notify("鱼还没咬钩。");
            return;
        }
        if (!inventory.TryAddItemById(56, 1))
        {
            farm.Notify("背包已满，腾出空位再收线。");
            return;
        }
        biteAt = -1f;
        farm.PlaySound("Harvest");
        farm.Notify("钓到一条溪鱼。可以在锅里做鱼汤。");
    }

    private void Update()
    {
        if (service != Service.Fishing || biteAt < 0f || announced || Time.time < biteAt)
            return;
        announced = true;
        if (Player.Instance != null && Vector3.Distance(Player.Instance.transform.position, transform.position) < 9f)
            AgricultureSystem.Instance.Notify("鱼咬钩了！");
    }
}
