using System;
using UnityEngine;

public sealed class InventoryInteraction
{
    public event Action HeldChanged;

    public cfg.ItemConfig HeldConfig { get; private set; } //持有物品的配置
    public int HeldCount { get; private set; } //持有数量
    public bool IsHolding => HeldCount > 0; //拿起状态

    private readonly Inventory inventory;

    //快速转移部分
    private readonly int hotbarCapacity;

    public InventoryInteraction(Inventory inventory, int hotbarCapacity)
    {
        this.inventory = inventory ?? throw new System.ArgumentNullException(nameof(inventory));

        if (hotbarCapacity < 0 || hotbarCapacity > inventory.Capacity)
            throw new System.ArgumentOutOfRangeException(nameof(hotbarCapacity));

        this.hotbarCapacity = hotbarCapacity;
    }

    public bool QuickMove(int index)
    {
        InventorySlot source = inventory.GetSlot(index);

        if (source.IsEmpty)
            return false;

        int targetStart;
        int targetCount;

        if (index < hotbarCapacity)
        {
            targetStart = hotbarCapacity;
            targetCount = inventory.Capacity - hotbarCapacity;
        }
        else
        {
            targetStart = 0;
            targetCount = hotbarCapacity;
        }

        int transferred = InventoryTransfer.Transfer(
            inventory,
            index,
            inventory,
            targetStart,
            targetCount
        );

        return transferred > 0;
    }

    public bool LeftClick(int index)
    {
        return LeftClick(inventory, index);
    }

    public bool LeftClick(Inventory container, int index)
    {
        if (container == null)
            throw new ArgumentNullException(nameof(container));

        InventorySlot slot = container.GetSlot(index);

        if (!IsHolding)
        {
            if (slot.IsEmpty)
                return false;
            PickUp(slot, slot.Count);
        }
        else if (slot.IsEmpty || slot.Config.Id == HeldConfig.Id)
        {
            if (!Place(slot, HeldCount))
                return false;
        }
        else
        {
            Swap(slot);
        }

        PublishChanges(container);
        return true;
    }

    public bool RightClick(int index)
    {
        return RightClick(inventory, index);
    }

    public bool RightClick(Inventory container, int index)
    {
        if (container == null)
            throw new ArgumentNullException(nameof(container));

        InventorySlot slot = container.GetSlot(index);
        if (!IsHolding)
        {
            if (slot.IsEmpty)
                return false;

            int amount = slot.Count / 2 + slot.Count % 2;
            PickUp(slot, amount);
        }
        else if (slot.IsEmpty || slot.Config.Id == HeldConfig.Id)
        {
            if (!Place(slot, 1))
                return false;
        }
        else
        {
            Swap(slot);
        }
        PublishChanges(container);
        return true;
    }

    private void PickUp(InventorySlot slot, int amount)
    {
        cfg.ItemConfig config = slot.Config;
        int removed = slot.Remove(amount);

        SetHeld(config, removed);
    }

    private void SetHeld(cfg.ItemConfig config, int count)
    {
        HeldCount = count;
        HeldConfig = count == 0 ? null : config;
    }

    private bool Place(InventorySlot slot, int amount)
    {
        int remaining = slot.Add(HeldConfig, amount);
        int placed = amount - remaining;

        if (placed == 0)
            return false;

        SetHeld(HeldConfig, HeldCount - placed);
        return true;
    }

    private void Swap(InventorySlot slot)
    {
        cfg.ItemConfig oldConfig = slot.Config;
        int oldCount = slot.Count;

        slot.Clear();
        slot.Add(HeldConfig, HeldCount);

        SetHeld(oldConfig, oldCount);
    }

    private void PublishChanges(Inventory container)
    {
        container.NotifyChanged();
        HeldChanged?.Invoke();
    }

    public bool CanReceive(cfg.ItemConfig config, int amount)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));

        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount));

        if (!IsHolding)
            return amount <= config.MaxStack;

        return HeldConfig.Id == config.Id && amount <= HeldConfig.MaxStack - HeldCount;
    }

    public bool TryReceiveWithoutNotify(cfg.ItemConfig config, int amount)
    {
        if (!CanReceive(config, amount))
            return false;

        SetHeld(config, HeldCount + amount);
        return true;
    }

    public void NotifyHeldChanged()
    {
        HeldChanged?.Invoke();
    }

    public bool TryReturnHeld()
    {
        if (!IsHolding)
            return true;

        int originalCount = HeldCount;
        int remaining = inventory.AddWithoutNotify(HeldConfig, HeldCount);

        if (remaining != originalCount)
        {
            SetHeld(HeldConfig, remaining);
            PublishChanges(inventory);
        }

        return !IsHolding;
    }
}
