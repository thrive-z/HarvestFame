using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance;

    public GameObject slot_prefab;
    public GameObject item_prefab;
    public Toolbar toolbar;
    public Backpack backpack;
    public StorageBox storageBox;
    public Palm palm;
    public Tip tip;
    public InventorySlot focusSlot;
    public InventorySlot beforeSlot;

    private IEnumerable<InventorySlot> PlayerSlots => toolbar.slotList.Concat(backpack.slotList);

    private void Awake()
    {
        Instance = this;
        slot_prefab = Resources.Load<GameObject>("Prefab/Slot");
        item_prefab = Resources.Load<GameObject>("Prefab/Item");
        toolbar = GameObject.Find("Toolbar").GetComponent<Toolbar>();
        toolbar.InitContainer(10);
        backpack = GameObject.Find("Backpack").GetComponent<Backpack>();
        backpack.InitContainer(32);
        storageBox = GameObject.Find("StorageBox").GetComponent<StorageBox>();
        storageBox.InitContainer(8);
        palm = GameObject.Find("Palm").GetComponent<Palm>();
        palm.InitContainer(1);
        tip = GameObject.Find("Tip").GetComponent<Tip>();
    }

    public bool TryAddItemById(int id, int count)
    {
        return LoadManager.instance.itemDatasDict.TryGetValue(id, out var item) && TryAddItem(item, count);
    }

    public bool TryAddItem(ItemData item, int count)
    {
        if (item == null || count <= 0 || item.max_stack <= 0)
            return false;

        var capacity = 0;
        foreach (var slot in PlayerSlots)
        {
            if (slot.bindItem == null)
                capacity += item.max_stack;
            else if (slot.bindItem.itemData.id == item.id)
                capacity += Mathf.Max(0, item.max_stack - slot.bindItem.itemData.current_stack);

            if (capacity >= count)
                break;
        }

        if (capacity < count)
            return false;

        var offset = 0;
        foreach (var slot in PlayerSlots)
        {
            var existing = slot.bindItem?.itemData;
            if (existing == null || existing.id != item.id || existing.current_stack >= item.max_stack)
                continue;

            var added = Mathf.Min(count, item.max_stack - existing.current_stack);
            existing.AppendStackRange(item, offset, added);
            slot.SetItemAmount(existing.current_stack + added);
            count -= added;
            offset += added;
            if (count == 0)
                return true;
        }

        foreach (var slot in PlayerSlots)
        {
            if (slot.bindItem != null)
                continue;

            var added = Mathf.Min(count, item.max_stack);
            slot.AddItem(item.CopyStackRange(offset, added), added);
            count -= added;
            offset += added;
            if (count == 0)
                return true;
        }

        return false;
    }

    public bool TryAddBatchById(IReadOnlyDictionary<int, int> rewards)
    {
        var freeSlots = PlayerSlots.Count(slot => slot.bindItem == null);
        foreach (var reward in rewards)
        {
            if (reward.Value <= 0 || !LoadManager.instance.itemDatasDict.TryGetValue(reward.Key, out var item)
                || item.max_stack <= 0)
                return false;

            var spaceInStacks = PlayerSlots
                .Where(slot => slot.bindItem?.itemData.id == reward.Key)
                .Sum(slot => Mathf.Max(0, item.max_stack - slot.bindItem.itemData.current_stack));
            var overflow = Mathf.Max(0, reward.Value - spaceInStacks);
            freeSlots -= Mathf.CeilToInt((float)overflow / item.max_stack);
            if (freeSlots < 0)
                return false;
        }

        foreach (var reward in rewards)
            TryAddItemById(reward.Key, reward.Value);
        return true;
    }

    public int CountItem(int id)
    {
        return PlayerSlots.Sum(slot => slot.bindItem?.itemData.id == id ? slot.bindItem.itemData.current_stack : 0);
    }

    public bool TryRemoveItems(IReadOnlyDictionary<int, int> costs)
    {
        foreach (var cost in costs)
        {
            if (cost.Value <= 0 || CountItem(cost.Key) < cost.Value)
                return false;
        }

        foreach (var cost in costs)
            RemoveItem(cost.Key, cost.Value);
        return true;
    }

    public bool TryRemoveItem(int id, int count)
    {
        if (count <= 0 || CountItem(id) < count)
            return false;

        RemoveItem(id, count);
        return true;
    }

    private void RemoveItem(int id, int count)
    {
        foreach (var slot in PlayerSlots)
        {
            var item = slot.bindItem?.itemData;
            if (item == null || item.id != id)
                continue;

            var removed = Mathf.Min(count, item.current_stack);
            slot.SetItemAmount(item.current_stack - removed);
            count -= removed;
            if (count == 0)
                return;
        }
    }

    public bool isPicked() => palm.slotList[0].bindItem != null;

    public bool Discard(ItemData item)
    {
        var player = Player.Instance.transform;
        return CreateItemInWorld(player.position + player.forward * 0.7f, item, item.current_stack);
    }

    public bool CreateItemInWorld(Vector3 position, ItemData data, int count)
    {
        var prefab = LoadManager.instance.GetPrefab(data.id);
        var ground = LayerMask.GetMask("Ground");
        if (Physics.Raycast(position + Vector3.up * 2f, Vector3.down, out var floor, 5f, ground))
            position = floor.point + Vector3.up * 0.04f;

        CollectItem pickup;
        if (prefab != null && prefab.GetComponent<CollectItem>() != null)
        {
            pickup = Instantiate(prefab, position, Quaternion.identity).GetComponent<CollectItem>();
        }
        else
        {
            var dropped = new GameObject(data.name);
            dropped.transform.position = position;
            var hitbox = dropped.AddComponent<BoxCollider>();
            hitbox.size = new Vector3(0.55f, 0.62f, 0.55f);
            hitbox.center = Vector3.up * 0.30f;

            var icon = new GameObject("Item icon", typeof(SpriteRenderer));
            icon.transform.SetParent(dropped.transform, false);
            icon.transform.localScale = Vector3.one * 1.4f;
            var renderer = icon.GetComponent<SpriteRenderer>();
            renderer.sprite = data.icon;
            if (renderer.sprite == null)
            {
                var parcel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                parcel.transform.SetParent(dropped.transform, false);
                parcel.transform.localPosition = Vector3.up * 0.21f;
                parcel.transform.localScale = new Vector3(0.35f, 0.28f, 0.35f);
                parcel.GetComponent<Collider>().enabled = false;
            }

            dropped.AddComponent<WorldPickupVisual>().Initialize(icon.transform);
            pickup = dropped.AddComponent<CollectItem>();
        }
        pickup.SetData(new ItemData(data), count);
        return true;
    }
}
