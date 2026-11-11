using UnityEngine;

public sealed class CropDefinition
{
    public int item_id;
    public int harvest_id;
    public string prefab_name;
    public float[] timePerStage;
    public float[] modelHeights;
    public GameObject[] Models { get; set; }

    public string Name => LoadManager.instance.itemDatasDict[harvest_id].name;
}

public sealed class FarmPlot : MonoBehaviour, IInteractable
{
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly Color Tilled = new(0.34f, 0.20f, 0.11f);
    private static readonly Color Wet = new(0.19f, 0.11f, 0.07f);

    private AgricultureSystem farm;
    private Transform cropParent;
    private Renderer surface;
    private MaterialPropertyBlock tint;
    private CropDefinition crop;
    private GameObject model;
    private int stage;
    private float stageTime;
    private bool tilled;
    private bool watered;

    private bool IsMature => crop != null && stage >= crop.timePerStage.Length;

    public void Initialize(AgricultureSystem owner, Transform worldParent)
    {
        farm = owner;
        cropParent = worldParent;
        surface = GetComponent<Renderer>();
        tint = new MaterialPropertyBlock();
        RefreshSoil();
    }

    public bool Till()
    {
        if (tilled)
            return false;

        tilled = true;
        RefreshSoil();
        farm.Notify("土地已开垦。选中种子后左键播种。");
        return true;
    }

    public bool Plant(CropDefinition definition)
    {
        if (!tilled || crop != null)
        {
            farm.Notify(tilled ? "这块地已经有作物。" : "先用锄头开垦土地。");
            return false;
        }

        crop = definition;
        stage = 0;
        stageTime = 0f;
        RefreshPlant();
        farm.PlaySound("PlantSeed");
        farm.Notify($"已播种{crop.Name}。每个生长阶段需要浇一次水。");
        return true;
    }

    public bool Water()
    {
        if (!tilled || watered || IsMature)
            return false;

        watered = true;
        RefreshSoil();
        farm.PlaySound("Water");
        return true;
    }

    private void Update()
    {
        if (crop == null || !watered || IsMature)
            return;

        stageTime += Time.deltaTime;
        if (stageTime < crop.timePerStage[stage])
            return;

        stage++;
        stageTime = 0f;
        watered = false;
        RefreshSoil();
        RefreshPlant();
    }

    public string GetInteractTip()
    {
        if (crop == null)
            return tilled ? "已开垦 · 选中种子后左键播种" : "未开垦 · 选中锄头后左键";
        if (IsMature)
            return $"{crop.Name}已成熟 · F 收获";
        if (!watered)
            return $"{crop.Name}缺水 · 选中花洒后左键";
        return $"{crop.Name}生长中 · {Mathf.CeilToInt(crop.timePerStage[stage] - stageTime)} 秒";
    }

    public bool CanInteract() => IsMature;

    public void Interact(GameObject interactor)
    {
        if (!IsMature)
            return;

        if (!InventorySystem.Instance.TryAddItemById(crop.harvest_id, 1))
        {
            farm.Notify("背包已满。作物仍留在地里。");
            return;
        }

        FarmCampaign.Instance?.Report(crop.harvest_id == 32 ? "tomato" : "pumpkin");
        farm.Notify($"收获了{crop.Name}。");
        farm.PlaySound("Harvest");
        Destroy(model);
        model = null;
        crop = null;
    }

    public Snapshot Capture() => new Snapshot
    {
        seed = crop?.item_id ?? 0,
        tilled = tilled,
        watered = watered,
        stage = stage,
        seconds = stageTime
    };

    public void Restore(Snapshot saved)
    {
        if (saved == null)
            return;
        crop = farm.GetCrop(saved.seed);
        tilled = saved.tilled;
        watered = saved.watered;
        stage = crop == null ? 0 : Mathf.Clamp(saved.stage, 0, crop.timePerStage.Length);
        stageTime = Mathf.Max(0f, saved.seconds);
        RefreshSoil();
        if (crop != null)
            RefreshPlant();
    }

    public sealed class Snapshot
    {
        public int seed;
        public bool tilled;
        public bool watered;
        public int stage;
        public float seconds;
    }

    private void RefreshSoil()
    {
        tint.SetColor(ColorId, watered ? Wet : Tilled);
        surface.SetPropertyBlock(tilled ? tint : null);
    }

    private void RefreshPlant()
    {
        if (model != null)
            Destroy(model);

        if (crop.Models == null || stage >= crop.Models.Length || crop.Models[stage] == null)
            return;

        model = Instantiate(crop.Models[stage], transform.position + Vector3.up * 0.035f,
            Quaternion.identity, cropParent);
        if (crop.modelHeights != null && stage < crop.modelHeights.Length)
        {
            var renderers = model.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                var bounds = renderers[0].bounds;
                foreach (var renderer in renderers)
                    bounds.Encapsulate(renderer.bounds);
                if (bounds.size.y > 0.001f)
                {
                    var scale = crop.modelHeights[stage] / bounds.size.y;
                    var offset = bounds.min.y - model.transform.position.y;
                    model.transform.localScale *= scale;
                    model.transform.position -= Vector3.up * (offset * scale);
                }
            }
        }
        foreach (var collider in model.GetComponentsInChildren<Collider>())
            collider.enabled = false;
    }
}
