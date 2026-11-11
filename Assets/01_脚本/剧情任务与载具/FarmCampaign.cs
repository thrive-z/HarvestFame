using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(100)]
public sealed class FarmCampaign : MonoBehaviour
{
    private enum View { Title, ConfirmNew, Opening, Dialogue, Npc, Journal, Map, Pause, Playing }
    private const float MaxHealth = 100f;
    private const int MonsterChapter = 2;
    private const int DrivingChapter = 3;

    [SerializeField] private FarmTractor tractor;
    [SerializeField] private FarmTractor pickup;
    [SerializeField] private Transform[] deliveryGates;
    [SerializeField] private Transform openingLookAt;

    public static FarmCampaign Instance { get; private set; }

    private readonly Dictionary<string, int> progress = new();
    private FarmMonster[] monsters;
    private FarmNpc[] npcs;
    private CampaignData story;
    private CampaignHud hud;
    private FarmWeapon weapon;
    private PersonCamera followCamera;
    private AudioSource ambience;
    private float nextFootstep;
    private FarmNpc currentNpc;
    private string[] dialogue;
    private string dialogueSpeaker;
    private int line;
    private int chapter;
    private float lineStarted;
    private float openingStarted;
    private float health = MaxHealth;
    private float nextDamage;
    private bool hasStarted;
    private bool sideComplete;
    private FarmTractor currentVehicle;
    private FarmTractor lastVehicle;
    public int Coins { get; private set; } = 160;
    private Vector3 spawn;
    private View view = View.Title;

    public bool IsRunning => hasStarted;
    public bool BlocksWorldInput => view != View.Playing;
    public bool IsDriving { get; private set; }
    public bool CombatUnlocked => hasStarted && chapter >= MonsterChapter;
    public bool DrivingUnlocked => hasStarted && chapter >= DrivingChapter;
    public bool IsComplete => chapter >= story.chapters.Length;
    public bool CanUseFarmActions => IsRunning && !BlocksWorldInput && !IsDriving;

    private InventorySystem Inventory => InventorySystem.Instance;
    private Chapter ActiveChapter => IsComplete ? null : story.chapters[chapter];
    private FarmPlot[] plots;
    private FarmStation[] stations;

    private void Awake() => Instance = this;

    private void Start()
    {
        story = AgricultureSystem.ReadData<CampaignData>("FarmCampaign");
        var definitions = AgricultureSystem.ReadData<List<FarmNpcData>>("Npc").ToDictionary(npc => npc.npcId);
        npcs = GetComponentsInChildren<FarmNpc>(true);
        foreach (var npc in npcs)
        {
            var definition = definitions[npc.Id];
            npc.Initialize(this, definition.name, definition.dialogue.FirstOrDefault()?.dialogues);
        }

        monsters = GetComponentsInChildren<FarmMonster>(true);
        foreach (var monster in monsters)
            monster.Initialize(this);

        plots = FindObjectsOfType<FarmPlot>().OrderBy(plot => plot.gameObject.name, StringComparer.Ordinal).ToArray();
        stations = FindObjectsOfType<FarmStation>().OrderBy(station => station.StationKind).ToArray();
        tractor.Initialize(this);
        if (pickup != null)
            pickup.Initialize(this);
        weapon = Player.Instance.GetComponent<FarmWeapon>();
        weapon.Initialize(this);
        spawn = Player.Instance.transform.position;
        followCamera = Camera.main.GetComponent<PersonCamera>();
        hud = new CampaignHud(transform);
        ambience = gameObject.AddComponent<AudioSource>();
        ambience.clip = Resources.Load<AudioClip>("Music/游戏背景音");
        ambience.loop = true;
        ambience.volume = 0.14f;
        ambience.playOnAwake = false;
        ShowTitle();
    }

    private void Update()
    {
        switch (view)
        {
            case View.Title:
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    if (CampaignSave.Exists)
                    {
                        view = View.ConfirmNew;
                        hud.PanelText("开始新游戏", "新游戏会覆盖当前存档。", "Y 确认     Esc 返回");
                    }
                    else
                        BeginNewGame();
                }
                else if (Input.GetKeyDown(KeyCode.C) && CampaignSave.Exists)
                    ContinueGame();
                break;
            case View.ConfirmNew:
                if (Input.GetKeyDown(KeyCode.Y))
                    BeginNewGame();
                else if (Input.GetKeyDown(KeyCode.Escape))
                    ShowTitle();
                break;
            case View.Opening:
                UpdateOpening();
                break;
            case View.Dialogue:
                UpdateDialogue();
                break;
            case View.Npc:
                if (Input.GetKeyDown(KeyCode.Q))
                    TryAdvance(currentNpc);
                else if (Input.GetKeyDown(KeyCode.E))
                    ShowDialogue(currentNpc.DisplayName, new[] { currentNpc.NextChat() });
                else if (Input.GetKeyDown(KeyCode.M) && currentNpc.Id == "Mary")
                {
                    ClosePanel();
                    Shop.Instance.OpenMarket();
                }
                else if (Input.GetKeyDown(KeyCode.T) && currentNpc.Id == "Mary")
                    TryDeliverEggs();
                else if (Input.GetKeyDown(KeyCode.Escape))
                    ClosePanel();
                break;
            case View.Journal:
                if (Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.Escape))
                    ClosePanel();
                break;
            case View.Map:
                if (Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.Escape))
                    ClosePanel();
                break;
            case View.Pause:
                if (Input.GetKeyDown(KeyCode.Escape))
                    ClosePanel();
                else if (Input.GetKeyDown(KeyCode.S))
                    Save(true);
                else if (Input.GetKeyDown(KeyCode.Q) && Save(false))
                    Application.Quit();
                break;
            case View.Playing:
                UpdatePlaying();
                break;
        }
    }

    private void LateUpdate()
    {
        if (hud == null)
            return;
        var status = IsDriving ? $"{currentVehicle.DisplayName} {currentVehicle.SpeedKph:0} km/h   WASD 驾驶 · 空格刹车 · F 下车" : weapon.Describe();
        var showWorldHud = view == View.Playing && !Inventory.backpack.isOpen && !Inventory.storageBox.isOpen
            && Shop.Instance?.isOpen != true && !AgricultureSystem.Instance.HasOpenStation;
        hud.Refresh(Player.Instance.transform, DescribeQuest(), status, hasStarted && showWorldHud);
    }

    private void ShowTitle()
    {
        view = View.Title;
        hud.PanelText("丰收农场  /  山坡驿站",
            "种下第一批作物，给山坡上的人带去补给。\n\nWASD 移动    鼠标转动视角    F 互动",
            CampaignSave.Exists ? "Enter 新游戏     C 继续游戏" : "Enter 开始游戏");
    }

    private void BeginNewGame()
    {
        hasStarted = true;
        if (ambience.clip != null)
            ambience.Play();
        openingStarted = Time.unscaledTime;
        view = View.Opening;
        followCamera.enabled = false;
        hud.PanelText("序章  /  风暴之后", "北路断了。今天的补给，要从这里出发。", "Enter 跳过镜头");
        Save(false);
    }

    private void UpdateOpening()
    {
        var fraction = Mathf.Clamp01((Time.unscaledTime - openingStarted) / 3.5f);
        var focus = openingLookAt.position;
        var camera = followCamera.transform;
        camera.position = focus + Vector3.Lerp(new Vector3(-11f, 13f, -28f), new Vector3(-3f, 7f, -15f), fraction);
        camera.LookAt(focus);
        if (fraction < 1f && !Input.GetKeyDown(KeyCode.Return))
            return;

        camera.position = Player.Instance.transform.position + new Vector3(0f, 3.4f, -4.7f);
        followCamera.enabled = true;
        ShowDialogue("旁白", story.intro);
    }

    private void ContinueGame()
    {
        try
        {
            Restore(CampaignSave.Read());
            hasStarted = true;
            if (ambience.clip != null)
                ambience.Play();
            ClosePanel();
            Notify("已读取存档。");
        }
        catch (Exception ex)
        {
            Debug.LogWarning("HarvestFame save could not load: " + ex.Message);
            hud.PanelText("无法读取存档", "存档内容损坏或不兼容。", "Enter 开始新游戏");
        }
    }

    private void UpdatePlaying()
    {
        if (AgricultureSystem.Instance.HasOpenStation || AgricultureSystem.Instance.ClosedStationThisFrame
            || Shop.Instance?.ClosedThisFrame == true || Cursor.lockState != CursorLockMode.Locked)
            return;

        if (!IsDriving && Time.time >= nextFootstep)
        {
            var movement = Player.Instance.GetComponent<PlayerMovement>();
            var velocity = movement.GetComponent<Rigidbody>().velocity;
            if (movement.isGrounded && new Vector2(velocity.x, velocity.z).sqrMagnitude > 3f)
            {
                nextFootstep = Time.time + (Input.GetKey(KeyCode.LeftShift) ? 0.30f : 0.46f);
                PlaySound("走路");
            }
        }
        if (Input.GetKeyDown(KeyCode.F5))
            Save(true);
        if (Input.GetKeyDown(KeyCode.M))
        {
            view = View.Map;
            hud.ExpandMap(true);
            return;
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            view = View.Journal;
            var side = sideComplete ? "鸡舍委托：已完成" : "鸡舍委托：给玛丽带两个鸡蛋，可在对话里提交。";
            hud.PanelText("农场日志", DescribeQuest() + "\n\n" + side, "J / Esc 返回");
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            view = View.Pause;
            hud.PanelText("暂停", "进度会在主线推进时自动保存。", "Esc 继续     S 存档     Q 保存并退出");
        }
    }

    public void OpenNpc(FarmNpc npc)
    {
        currentNpc = npc;
        view = View.Npc;
        var current = ActiveChapter;
        var action = current != null && current.npc == npc.Id ? "Q 主线任务     " : string.Empty;
        var side = npc.Id == "Mary" ? "     M 商店" + (sideComplete ? string.Empty : "     T 鸡舍委托") : string.Empty;
        hud.PanelText(npc.DisplayName, current != null && current.npc == npc.Id ? DescribeQuest() : "想聊点什么？",
            action + "E 闲聊" + side + "     Esc 离开");
    }

    private void TryAdvance(FarmNpc npc)
    {
        var active = ActiveChapter;
        if (active == null || active.npc != npc.Id)
            return;
        if (!active.goals.All(goal => Count(goal.key) >= goal.count))
        {
            Notify("还有任务目标没完成，按 J 查看。");
            return;
        }
        if (active.delivery != null && active.delivery.Any(item => Inventory.CountItem(item.Key) < item.Value))
        {
            Notify("请把做好的汤带在背包里再来交付。");
            return;
        }
        if (active.rewards != null && !Inventory.TryAddBatchById(active.rewards))
        {
            Notify("背包空间不足，腾出位置后再来。");
            return;
        }

        if (active.delivery != null)
            Inventory.TryRemoveItems(active.delivery);
        chapter++;
        Save(false);
        var lines = IsComplete ? active.lines.Concat(story.ending).ToArray() : active.lines;
        ShowDialogue(npc.DisplayName, lines);
    }

    private void TryDeliverEggs()
    {
        if (sideComplete)
            return;
        if (!Inventory.TryRemoveItem(53, 2))
        {
            ShowDialogue(currentNpc.DisplayName, new[] { "养大的鸡会下蛋。我想要两个鸡蛋，攒齐了再来。" });
            return;
        }

        sideComplete = true;
        AgricultureSystem.Instance.Vitals.Restore(100f, 100f);
        health = MaxHealth;
        Save(false);
        ShowDialogue(currentNpc.DisplayName, new[] { "多谢！坐下休息一会儿再走吧。体力和生命都恢复了。" });
    }

    private void ShowDialogue(string speaker, string[] lines)
    {
        dialogueSpeaker = speaker;
        dialogue = lines;
        line = 0;
        lineStarted = Time.unscaledTime;
        view = View.Dialogue;
        RenderLine();
    }

    private void UpdateDialogue()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePanel();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.F))
        {
            if (VisibleCharacters() < dialogue[line].Length)
                lineStarted = float.NegativeInfinity;
            else if (++line >= dialogue.Length)
            {
                ClosePanel();
                return;
            }
            else
                lineStarted = Time.unscaledTime;
        }
        RenderLine();
    }

    private int VisibleCharacters() => float.IsNegativeInfinity(lineStarted)
        ? dialogue[line].Length
        : Mathf.Min(dialogue[line].Length, Mathf.FloorToInt((Time.unscaledTime - lineStarted) * 34f));

    private void RenderLine() => hud.PanelText(dialogueSpeaker, dialogue[line].Substring(0, VisibleCharacters()),
        $"Enter / F 继续     Esc 跳过     {line + 1}/{dialogue.Length}");

    private void ClosePanel()
    {
        view = View.Playing;
        hud.ExpandMap(false);
        hud.HidePanel();
    }

    private string DescribeQuest()
    {
        var active = ActiveChapter;
        if (active == null)
            return "<b>丰收节 · 主线完成</b>\n自由经营农场   J 农场日志";
        var goals = string.Join("\n", active.goals.Select(goal => $"{goal.label}  {Mathf.Min(Count(goal.key), goal.count)}/{goal.count}"));
        var location = string.IsNullOrEmpty(goals) || active.goals.All(goal => Count(goal.key) >= goal.count)
            ? active.hint
            : goals;
        return $"<b>{active.title}</b>\n{location}";
    }

    private int Count(string key) => progress.TryGetValue(key, out var value) ? value : 0;

    public void Report(string key)
    {
        if (hasStarted)
            progress[key] = Count(key) + 1;
    }

    public void CheckRoute(Vector3 position)
    {
        var next = Count("route");
        if (!IsDriving || chapter != DrivingChapter || next >= deliveryGates.Length)
            return;
        if (Vector3.Distance(position, deliveryGates[next].position) > 4.8f)
            return;

        Report("route");
        Notify(next + 1 == deliveryGates.Length ? "已到驿站。停车下车，找维克。" : "通过第一道路标，继续向北。");
        Save(false);
    }

    public void SetDriving(FarmTractor vehicle)
    {
        currentVehicle = vehicle;
        IsDriving = vehicle != null;
        if (IsDriving)
            lastVehicle = vehicle;
    }

    public void SpendCoins(int amount) => Coins -= amount;

    public void SellSelected()
    {
        var slot = Inventory.toolbar.SelectedSlot;
        var item = slot.bindItem?.itemData;
        if (item == null || (item.item_type != ItemType.Food && item.item_type != ItemType.Material))
        {
            Notify("商店只收购食物和采集材料。");
            return;
        }

        var earnings = Mathf.Max(1, Mathf.FloorToInt(item.price * 0.6f));
        Coins += earnings;
        slot.SetItemAmount(item.current_stack - 1);
        Notify($"已出售{item.name}，获得 {earnings} 金币。");
    }

    public void HurtPlayer(float amount)
    {
        if (Time.time < nextDamage || !CanUseFarmActions)
            return;
        nextDamage = Time.time + 0.6f;
        health = Mathf.Max(0f, health - amount);
        ReportCombat($"受到攻击 · 生命 {Mathf.CeilToInt(health)}/100");
        if (health > 0f)
            return;

        var player = Player.Instance;
        player.transform.position = spawn + Vector3.up * 0.05f;
        player.GetComponent<Rigidbody>().velocity = Vector3.zero;
        health = MaxHealth;
        foreach (var monster in monsters)
        {
            if (!monster.IsDefeated)
                monster.Restore(false);
        }
        Notify("玛丽把你带回了农场。背包和任务进度还在。");
        Save(false);
    }

    public bool HealFromFood()
    {
        if (health >= MaxHealth)
            return false;
        health = Mathf.Min(MaxHealth, health + 22f);
        return true;
    }

    public string HealthText => $"生命 {Mathf.CeilToInt(health)}";
    public void PlaySound(string sound) => AgricultureSystem.Instance.PlaySound(sound);
    public void Notify(string message) => hud?.Notify(message);

    public void ReportCombat(string message)
    {
        Debug.Log(message);
        hud?.ShowCombat(message);
    }

    private bool Save(bool showFeedback)
    {
        if (!hasStarted)
            return false;
        try
        {
            var storage = FindObjectOfType<BoxEntity>();
            CampaignSave.Write(new CampaignSave
            {
                chapter = chapter,
                coins = Coins,
                progress = new Dictionary<string, int>(progress),
                health = health,
                stamina = AgricultureSystem.Instance.Vitals.Stamina,
                satiety = AgricultureSystem.Instance.Vitals.Satiety,
                sideComplete = sideComplete,
                toolbar = Inventory.toolbar.GetAllItemDataToList(),
                backpack = Inventory.backpack.GetAllItemDataToList(),
                storage = storage == null ? null : storage.isOpen
                    ? Inventory.storageBox.GetAllItemDataToList() : storage.itemDataList,
                held = Inventory.palm.slotList[0].bindItem?.itemData,
                selectedSlot = Inventory.toolbar.selectedSlot,
                plots = plots.Select(plot => plot.Capture()).ToArray(),
                stations = stations.Select(station => station.Capture()).ToArray(),
                defeated = monsters.Select(monster => monster.IsDefeated).ToArray(),
                tractorPosition = new CampaignSave.Position(tractor.transform.position),
                tractorYaw = tractor.transform.eulerAngles.y,
                pickupPosition = pickup == null ? null : new CampaignSave.Position(pickup.transform.position),
                pickupYaw = pickup == null ? 0f : pickup.transform.eulerAngles.y,
                lastVehicleWasPickup = pickup != null && lastVehicle == pickup
            });
            if (showFeedback)
                Notify("已保存进度。");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogWarning("HarvestFame save failed: " + ex.Message);
            Notify("无法保存进度。");
            return false;
        }
    }

    private void Restore(CampaignSave save)
    {
        if (save.chapter < 0 || save.chapter > story.chapters.Length || save.progress == null
            || (save.plots == null || save.plots.Length > plots.Length) || save.stations?.Length != stations.Length
            || save.defeated?.Length != monsters.Length || save.tractorPosition == null
            || save.toolbar?.Count != Inventory.toolbar.slotList.Count
            || save.backpack?.Count != Inventory.backpack.slotList.Count)
            throw new InvalidOperationException("Save data does not match this farm.");

        chapter = save.chapter;
        Coins = Mathf.Max(0, save.coins);
        foreach (var entry in save.progress)
            progress[entry.Key] = Mathf.Max(0, entry.Value);
        health = Mathf.Clamp(save.health, 1f, MaxHealth);
        sideComplete = save.sideComplete;
        AgricultureSystem.Instance.Vitals.Restore(save.stamina, save.satiety);
        RestoreItems(Inventory.toolbar, save.toolbar);
        RestoreItems(Inventory.backpack, save.backpack);
        if (save.held != null)
            RestoreItem(Inventory.palm.slotList[0], save.held);
        Inventory.toolbar.SelectSlot(Mathf.Clamp(save.selectedSlot, 0, Inventory.toolbar.slotList.Count - 1));
        var storage = FindObjectOfType<BoxEntity>();
        if (storage != null && save.storage != null)
            storage.itemDataList = save.storage.Select(RefreshItem).ToList();
        for (var i = 0; i < save.plots.Length; i++)
            plots[i].Restore(save.plots[i]);
        if (!progress.ContainsKey("pumpkinPlanted"))
            progress["pumpkinPlanted"] = Count("pumpkin") + save.plots.Count(plot => plot?.seed == 21);
        if (!progress.ContainsKey("axePurchase") && Inventory.CountItem(6) > 0)
            progress["axePurchase"] = 1;
        for (var i = 0; i < stations.Length; i++)
            stations[i].Restore(save.stations[i]);
        for (var i = 0; i < monsters.Length; i++)
            monsters[i].Restore(save.defeated[i]);

        tractor.transform.SetPositionAndRotation(save.tractorPosition.Value, Quaternion.Euler(0f, save.tractorYaw, 0f));
        if (pickup != null && save.pickupPosition != null)
            pickup.transform.SetPositionAndRotation(save.pickupPosition.Value, Quaternion.Euler(0f, save.pickupYaw, 0f));

        lastVehicle = save.lastVehicleWasPickup && pickup != null ? pickup : tractor;
        var parked = lastVehicle.transform;
        var respawn = chapter >= DrivingChapter ? parked.position - parked.right * 2.7f : spawn;
        if (!Physics.Raycast(respawn + Vector3.up * 2f, Vector3.down, out var ground, 4f, LayerMask.GetMask("Ground")))
            respawn = spawn;
        else
            respawn = ground.point + Vector3.up * 0.04f;
        Player.Instance.transform.position = respawn;
        Physics.SyncTransforms();
    }

    private static void RestoreItems(InventoryContainer container, List<ItemData> items)
    {
        container.ClearAllItem();
        for (var i = 0; i < items.Count; i++)
            RestoreItem(container.slotList[i], items[i]);
    }

    private static void RestoreItem(InventorySlot slot, ItemData item)
    {
        var restored = RefreshItem(item);
        if (restored != null)
            slot.AddItem(restored, restored.current_stack);
    }

    private static ItemData RefreshItem(ItemData saved)
    {
        if (saved == null || saved.current_stack <= 0 || !LoadManager.instance.itemDatasDict.TryGetValue(saved.id, out var item))
            return null;

        return new ItemData(item)
        {
            current_stack = Mathf.Min(saved.current_stack, item.max_stack),
            current_durable = saved.current_durable,
            water_remaining = saved.water_remaining,
            loaded_ammo = saved.loaded_ammo,
            spare_tools = saved.spare_tools?.Take(Mathf.Max(0, Mathf.Min(saved.current_stack, item.max_stack) - 1)).ToList()
        };
    }

    private void OnApplicationQuit() => Save(false);

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
        hud?.Release();
    }

    private sealed class CampaignData
    {
        public string[] intro;
        public string[] ending;
        public Chapter[] chapters;
    }

    private sealed class Chapter
    {
        public string title;
        public string npc;
        public string hint;
        public Goal[] goals;
        public string[] lines;
        public Dictionary<int, int> rewards;
        public Dictionary<int, int> delivery;
    }

    private sealed class Goal
    {
        public string key;
        public string label;
        public int count;
    }
}
