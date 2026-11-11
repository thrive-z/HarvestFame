using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class FarmWeapon : MonoBehaviour
{
    private const int Sword = 3;
    private const int Pistol = 200;
    private const int Ammo = 201;

    [SerializeField] private GameObject swordModel;
    [SerializeField] private GameObject pistolModel;
    [SerializeField] private GameObject tracerPrefab;
    private FarmCampaign campaign;
    private PlayerInteraction interaction;
    private InventorySystem inventory;
    private WeaponData melee;
    private WeaponData pistol;
    private ItemData equipped;
    private ItemData reloadItem;
    private float reloadEnds;
    private float nextAttack;
    private bool aiming;
    private float normalFov;

    public void Initialize(FarmCampaign owner)
    {
        campaign = owner;
        interaction = GetComponent<PlayerInteraction>();
        inventory = InventorySystem.Instance;
        var definitions = AgricultureSystem.ReadData<List<WeaponData>>("Weapon");
        melee = definitions.Single(data => data.id == "spear_01");
        pistol = definitions.Single(data => data.id == "gun_01");
        normalFov = interaction.playerCamera.fieldOfView;
        Equip(null);
    }

    private void Update()
    {
        if (campaign == null || !campaign.IsRunning)
            return;

        var selected = inventory.toolbar.SelectedSlot.bindItem?.itemData;
        if (!ReferenceEquals(selected, equipped))
            Equip(selected);
        var canUse = AgricultureSystem.WorldInputEnabled && !campaign.IsDriving;
        if (!canUse)
        {
            reloadItem = null;
            SetAim(false);
            return;
        }

        SetAim(equipped?.id == Pistol && Input.GetMouseButton(1));
        if (equipped?.id == Pistol && Input.GetKeyDown(KeyCode.R))
            BeginReload();
        if (reloadItem != null && Time.time >= reloadEnds)
            FinishReload();
        if (Input.GetMouseButtonDown(0))
            Attack();
    }

    private void Equip(ItemData item)
    {
        equipped = item;
        reloadItem = null;
        swordModel.SetActive(item?.id == Sword);
        pistolModel.SetActive(item?.id == Pistol);
        SetAim(false);
    }

    private void SetAim(bool value)
    {
        if (aiming == value)
            return;
        aiming = value;
        interaction.playerCamera.fieldOfView = aiming ? normalFov / Mathf.Max(1f, pistol.adsZoom) : normalFov;
    }

    private void BeginReload()
    {
        if (reloadItem != null || equipped.loaded_ammo >= pistol.magazine_size)
            return;
        if (inventory.CountItem(Ammo) == 0)
        {
            campaign.Notify("背包里没有 9mm 子弹。");
            return;
        }

        reloadItem = equipped;
        reloadEnds = Time.time + pistol.reloadTime;
        campaign.PlaySound("Reload");
    }

    private void FinishReload()
    {
        var amount = Mathf.Min(pistol.magazine_size - reloadItem.loaded_ammo, inventory.CountItem(Ammo));
        if (amount > 0 && inventory.TryRemoveItem(Ammo, amount))
        {
            reloadItem.loaded_ammo += amount;
            campaign.ReportCombat($"装弹完成 · {reloadItem.loaded_ammo}/{pistol.magazine_size}");
        }
        reloadItem = null;
    }

    private void Attack()
    {
        if (equipped == null || (equipped.id != Pistol && equipped.id != Sword)
            || reloadItem != null || Time.time < nextAttack)
            return;

        var ranged = equipped.id == Pistol;
        var data = ranged ? pistol : melee;
        nextAttack = Time.time + data.attackCooldown;
        if (!campaign.CombatUnlocked)
        {
            campaign.Notify("先完成玛丽的农场委托。");
            return;
        }

        var reach = ranged ? 36f : data.attackRange;
        var didHit = interaction.TryGetTarget(out var hit, reach);
        if (ranged)
        {
            if (equipped.loaded_ammo <= 0)
            {
                campaign.PlaySound("EmptyBullet");
                campaign.ReportCombat("弹匣空了 · R 装弹");
                return;
            }

            equipped.loaded_ammo--;
            campaign.PlaySound("Shoot");
            var camera = interaction.playerCamera.transform;
            var end = didHit ? hit.point : camera.position + camera.forward * (reach + 5f);
            ShowTracer(pistolModel.transform.position, end);
        }

        var monster = didHit ? hit.collider.GetComponentInParent<FarmMonster>() : null;
        if (monster == null)
            campaign.ReportCombat(ranged ? "射击未命中" : "挥剑未命中");
        else
        {
            var critical = Random.value < data.critRate;
            monster.Hurt(data.damage * (critical ? data.critMultiplier : 1f));
        }
    }

    private void ShowTracer(Vector3 start, Vector3 end)
    {
        if (tracerPrefab == null || (end - start).sqrMagnitude < 0.02f)
            return;

        var tracer = Instantiate(tracerPrefab, start, Quaternion.LookRotation(end - start));
        var collider = tracer.GetComponentInChildren<Collider>();
        if (collider != null)
            collider.enabled = false;
        var body = tracer.GetComponent<Rigidbody>();
        if (body == null)
            body = tracer.AddComponent<Rigidbody>();
        body.useGravity = false;
        body.isKinematic = false;
        body.velocity = (end - start).normalized * 85f;
        Destroy(tracer, Mathf.Max(0.025f, Vector3.Distance(start, end) / 85f));
    }

    public string Describe()
    {
        if (equipped?.id == Pistol)
        {
            if (reloadItem != null)
                return "装弹中";
            return $"手枪 {equipped.loaded_ammo}/{pistol.magazine_size}  备用 {inventory.CountItem(Ammo)}   R 装弹 / 右键瞄准";
        }
        return equipped?.id == Sword ? "剑   左键近战" : string.Empty;
    }

    private sealed class WeaponData
    {
        public string id;
        public float damage;
        public float critRate;
        public float critMultiplier;
        public float attackCooldown;
        public float attackRange;
        public int magazine_size;
        public float reloadTime;
        public float adsZoom;
    }
}
