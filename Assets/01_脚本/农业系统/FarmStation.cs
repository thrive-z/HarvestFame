using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class FarmStation : MonoBehaviour, IInteractable
{
    public enum Kind { Well, Coop, CookingPot }

    public const int WaterCapacity = 8;
    private const int ChickItem = 51;
    private const int EggItem = 53;
    private const int FeedItem = 101;
    private const int FuelItem = 102;
    private const int TroughCapacity = 12;
    private const int CoopCapacity = 4;
    private const float FeedingInterval = 10f;

    private AgricultureSystem farm;
    private List<Recipe> recipes;
    private Recipe cooking;
    private Recipe cooked;
    private float cookTime;
    [SerializeField] private GameObject fire;
    [SerializeField] private Kind stationKind;

    private int feed;
    private int water;
    private int eggs;
    private readonly List<Hen> hens = new();
    [SerializeField] private Vector2 pastureHalfSize = new Vector2(3.4f, 2.3f);
    [SerializeField] private Vector3 pastureCenter = new Vector3(0f, 0f, -3.2f);
    private int selectedRecipe;

    private InventorySystem Inventory => InventorySystem.Instance;
    public Kind StationKind => stationKind;
    public float FeedFill => (float)feed / TroughCapacity;
    public float WaterFill => (float)water / TroughCapacity;

    public void Initialize(AgricultureSystem owner)
    {
        farm = owner;
        if (StationKind == Kind.CookingPot)
            recipes = AgricultureSystem.ReadData<List<Recipe>>("cooking");
    }

    public static int WaterRemaining(ItemData item) => item.water_remaining < 0 ? WaterCapacity : item.water_remaining;

    public bool CanInteract() => true;

    public string GetInteractTip()
    {
        switch (StationKind)
        {
            case Kind.Well: return "水井 · 选中花洒，按 F 补水";
            case Kind.Coop: return "鸡舍 · 按 F 查看";
            default: return "烹饪锅 · 按 F 查看";
        }
    }

    public void Interact(GameObject interactor)
    {
        if (StationKind != Kind.Well)
        {
            farm.OpenStation(this);
            return;
        }

        var item = Inventory.toolbar.SelectedSlot.bindItem?.itemData;
        if (item?.item_sub_type != ItemSubType.WaterCan)
        {
            farm.Notify("先在工具栏选中花洒。");
            return;
        }

        item.water_remaining = WaterCapacity;
        farm.PlaySound("FillWaterCan");
        farm.Notify("花洒已装满。");
    }

    private void Update()
    {
        if (cooking != null)
        {
            cookTime -= Time.deltaTime;
            if (cookTime <= 0f)
            {
                cooked = cooking;
                cooking = null;
                if (fire != null)
                    fire.SetActive(false);
                farm.Notify($"{cooked.name}煮好了。");
            }
        }

        foreach (var hen in hens)
            TendHen(hen);
    }

    private void TendHen(Hen hen)
    {
        if (Time.time >= hen.nextWander)
        {
            hen.target = pastureCenter + new Vector3(Random.Range(-pastureHalfSize.x, pastureHalfSize.x),
                0f, Random.Range(-pastureHalfSize.y, pastureHalfSize.y));
            hen.nextWander = Time.time + Random.Range(2f, 4f);
        }
        var delta = hen.model.localPosition - hen.target;
        if (delta.sqrMagnitude > 0.01f)
        {
            hen.model.localRotation = Quaternion.LookRotation(-delta);
            hen.model.localPosition = Vector3.MoveTowards(hen.model.localPosition, hen.target, Time.deltaTime * 0.60f);
        }
        if (feed <= 0 || water <= 0 || eggs >= TroughCapacity)
            return;

        hen.nextMeal -= Time.deltaTime;
        if (hen.nextMeal > 0f)
            return;

        hen.nextMeal = FeedingInterval;
        feed--;
        water--;
        hen.meals++;
        if (hen.meals == 2)
        {
            hen.model.localScale = Vector3.one;
            farm.Notify("小鸡长大了。继续喂养就会下蛋。");
        }
        else if (hen.meals > 2)
        {
            eggs++;
            farm.PlaySound("鸡打鸣");
        }
    }

    public void HandleMenuInput()
    {
        if (StationKind == Kind.CookingPot)
        {
            if (Input.GetKeyDown(KeyCode.R))
                CollectCooked();
            if (recipes.Count > 0 && Input.GetKeyDown(KeyCode.Q))
                StartCooking(recipes[0]);
            if (recipes.Count > 1 && Input.GetKeyDown(KeyCode.E))
                StartCooking(recipes[1]);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Q))
            AddChick();
        if (Input.GetKeyDown(KeyCode.E))
            AddFeed();
        if (Input.GetKeyDown(KeyCode.R))
            AddWater();
        if (Input.GetKeyDown(KeyCode.T))
            CollectEggs();
    }

    public string DescribeMenu()
    {
        var adults = hens.Count(hen => hen.meals >= 2);
        return $"<size=46>鸡舍</size>\n小鸡 {hens.Count - adults}  成年鸡 {adults}   栏位 {hens.Count}/{CoopCapacity}\n\n"
            + $"食槽 {feed}/{TroughCapacity}   水槽 {water}/{TroughCapacity}   鸡蛋 {eggs}\n"
            + "Q 放入选中的鸡   E 添草料\nR 用选中的花洒添水   T 收取鸡蛋\n\nEsc 关闭";
    }

    public IReadOnlyList<Recipe> Recipes => recipes;
    public Recipe Selected => recipes[selectedRecipe];
    public Recipe Current => cooking ?? cooked ?? Selected;
    public bool IsCooking => cooking != null;
    public bool IsCooked => cooked != null;
    public float CookProgress => cooked != null ? 1f : cooking == null ? 0f : 1f - cookTime / cooking.duration;
    public int IngredientCount(int id) => Inventory.CountItem(id);
    public void SelectRecipe(int index)
    {
        if (cooking == null && cooked == null && index >= 0 && index < recipes.Count)
            selectedRecipe = index;
    }
    public void CookSelected() => StartCooking(Selected);

    public void CancelCooking()
    {
        if (cooking == null)
            return;
        if (!Inventory.TryAddBatchById(RecipeCosts(cooking)))
        {
            farm.Notify("背包空间不足，无法退回材料。");
            return;
        }
        cooking = null;
        cookTime = 0f;
        if (fire != null)
            fire.SetActive(false);
    }

    private static Dictionary<int, int> RecipeCosts(Recipe recipe)
    {
        var costs = recipe.inputs.GroupBy(input => input.item_id)
            .ToDictionary(group => group.Key, group => group.Sum(input => input.count));
        if (recipe.require_fuel)
        {
            costs.TryGetValue(FuelItem, out var existing);
            costs[FuelItem] = existing + 1;
        }
        return costs;
    }

    private void StartCooking(Recipe recipe)
    {
        if (cooking != null || cooked != null)
        {
            farm.Notify("请先完成并领取当前菜品。");
            return;
        }

        if (!Inventory.TryRemoveItems(RecipeCosts(recipe)))
        {
            farm.Notify("食材或木柴不足。");
            return;
        }

        cooking = recipe;
        cookTime = recipe.duration;
        if (fire != null)
            fire.SetActive(true);
    }

    public void CollectCooked()
    {
        if (cooked == null)
            return;

        // The current recipe data produces one stack per batch.
        var output = cooked.outputs[0];
        if (!Inventory.TryAddItemById(output.item_id, output.count))
        {
            farm.Notify("背包已满。成品仍留在锅里。");
            return;
        }

        if (output.item_id == 55)
            FarmCampaign.Instance?.Report("soup");
        farm.Notify($"取出了{cooked.name}。选中后按 G 食用。");
        cooked = null;
    }

    private void AddChick()
    {
        if (hens.Count >= CoopCapacity)
        {
            farm.Notify("鸡舍满了。");
            return;
        }
        var item = Inventory.toolbar.SelectedSlot.bindItem?.itemData;
        var adult = item?.id == 52;
        if (!Inventory.TryRemoveItem(adult ? 52 : ChickItem, 1))
        {
            farm.Notify("背包里没有小鸡。也可以选中成年鸡再放入。");
            return;
        }
        SpawnChick(adult ? 2 : 0, FeedingInterval + hens.Count * 1.8f);
    }

    private void SpawnChick(int meals, float nextMeal)
    {
        var model = new GameObject("Chicken").transform;
        model.SetParent(transform, false);
        model.localPosition = pastureCenter + new Vector3(hens.Count - 1.5f, 0f, 0f);
        model.localScale = Vector3.one * (meals >= 2 ? 1f : 0.65f);
        var cream = meals >= 2 ? new Color(1f, 0.96f, 0.80f) : new Color(1f, 0.87f, 0.44f);
        AddChickenPart(model, "Body", new Vector3(0f, 0.32f, 0f), new Vector3(0.42f, 0.44f, 0.56f), cream);
        AddChickenPart(model, "Head", new Vector3(0f, 0.55f, 0.22f), Vector3.one * 0.27f, cream);
        AddChickenPart(model, "Beak", new Vector3(0f, 0.53f, 0.36f), new Vector3(0.11f, 0.075f, 0.16f), new Color(1f, 0.54f, 0.13f));
        AddChickenPart(model, "Comb", new Vector3(0f, 0.68f, 0.21f), new Vector3(0.07f, 0.13f, 0.13f), new Color(0.83f, 0.16f, 0.14f));
        AddChickenPart(model, "Eye L", new Vector3(-0.105f, 0.59f, 0.285f), Vector3.one * 0.042f, Color.black);
        AddChickenPart(model, "Eye R", new Vector3(0.105f, 0.59f, 0.285f), Vector3.one * 0.042f, Color.black);
        hens.Add(new Hen { model = model, meals = meals, nextMeal = nextMeal });
    }

    private static void AddChickenPart(Transform model, string name, Vector3 offset, Vector3 size, Color color)
    {
        var part = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        part.name = name;
        part.transform.SetParent(model, false);
        part.transform.localPosition = offset;
        part.transform.localScale = size;
        part.GetComponent<Collider>().enabled = false;
        var tint = new MaterialPropertyBlock();
        tint.SetColor("_Color", color);
        part.GetComponent<Renderer>().SetPropertyBlock(tint);
    }

    private sealed class Hen
    {
        public Transform model;
        public int meals;
        public float nextMeal;
        public Vector3 target;
        public float nextWander;
    }

    public void AddFeed()
    {
        if (feed >= TroughCapacity)
            return;
        if (Inventory.TryRemoveItem(FeedItem, 1))
            feed++;
        else
            farm.Notify("背包里没有草料。可在商店补充。");
    }

    public void AddWater()
    {
        if (water >= TroughCapacity)
            return;

        var item = Inventory.toolbar.SelectedSlot.bindItem?.itemData;
        if (item?.item_sub_type != ItemSubType.WaterCan || WaterRemaining(item) <= 0)
        {
            farm.Notify("请选择装有水的花洒。");
            return;
        }

        item.water_remaining = WaterRemaining(item) - 1;
        water++;
        farm.PlaySound("Water");
    }

    private void CollectEggs()
    {
        if (eggs <= 0)
            return;

        if (Inventory.TryAddItemById(EggItem, eggs))
            eggs = 0;
        else
            farm.Notify("背包已满。鸡蛋仍留在鸡舍。");
    }

    public Snapshot Capture() => new Snapshot
    {
        chicken = hens.Count > 0,
        meals = hens.Count == 0 ? 0 : hens[0].meals,
        flock = hens.Select(hen => new HenSave { meals = hen.meals, nextMeal = hen.nextMeal }).ToArray(),
        feed = feed,
        water = water,
        eggs = eggs,
        nextMeal = hens.Count == 0 ? 0f : hens[0].nextMeal,
        cooking = cooking?.id ?? 0,
        cooked = cooked?.id ?? 0,
        cookTime = cookTime
    };

    public void Restore(Snapshot saved)
    {
        if (saved == null)
            return;

        if (StationKind == Kind.CookingPot)
        {
            cooking = recipes.Find(recipe => recipe.id == saved.cooking);
            cooked = recipes.Find(recipe => recipe.id == saved.cooked);
            cookTime = Mathf.Max(0f, saved.cookTime);
            if (fire != null)
                fire.SetActive(cooking != null);
        }
        else if (StationKind == Kind.Coop)
        {
            feed = Mathf.Clamp(saved.feed, 0, TroughCapacity);
            water = Mathf.Clamp(saved.water, 0, TroughCapacity);
            eggs = Mathf.Clamp(saved.eggs, 0, TroughCapacity);
            if (saved.flock != null)
            {
                foreach (var hen in saved.flock.Take(CoopCapacity))
                    SpawnChick(hen.meals, hen.nextMeal);
            }
            else if (saved.chicken)
                SpawnChick(saved.meals, saved.nextMeal);
        }
    }

    public sealed class Snapshot
    {
        public HenSave[] flock;
        public bool chicken;
        public int meals;
        public int feed;
        public int water;
        public int eggs;
        public float nextMeal;
        public int cooking;
        public int cooked;
        public float cookTime;
    }

    public sealed class HenSave
    {
        public int meals;
        public float nextMeal;
    }

    public sealed class Recipe
    {
        public int id;
        public string name;
        public float duration;
        public bool require_fuel;
        public Ingredient[] inputs;
        public Ingredient[] outputs;
    }

    public sealed class Ingredient
    {
        public int item_id;
        public int count;
    }
}
