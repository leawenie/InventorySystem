using System;
using UnityEngine;

public enum InventoryMoveResult
{
    Rejected,
    AcceptedUnchanged,
    Changed,
}

/// <summary>
/// 背包类
/// 生命周期：游戏级
/// </summary>
public sealed class Inventory
{
    public event Action Changed;

    private readonly InventorySlot[] slots;

    public int Capacity => slots.Length;

    public Inventory(int capacity)
    {
        if (capacity <= 0)
            throw new System.ArgumentOutOfRangeException(nameof(capacity));

        slots = new InventorySlot[capacity];
        for (int i = 0; i < capacity; i++)
        {
            slots[i] = new InventorySlot(i);
        }
    }

    public InventorySlot GetSlot(int index)
    {
        if (index < 0 || index >= Capacity)
            throw new System.ArgumentOutOfRangeException(nameof(index));

        return slots[index];
    }

    public int Add(cfg.ItemConfig config, int amount)
    {
        if (config == null)
            throw new System.ArgumentNullException(nameof(config));

        if (amount < 0)
            throw new System.ArgumentOutOfRangeException(nameof(amount));

        int remaining = AddWithoutNotify(config, amount);

        if (remaining != amount)
            NotifyChanged();

        return remaining;
    }

    public int AddWithoutNotify(cfg.ItemConfig config, int amount)
    {
        int remaining = amount;

        foreach (InventorySlot slot in slots)
        {
            if (remaining <= 0)
                break;
            if (!slot.IsEmpty && slot.Config.Id == config.Id)
                remaining = slot.Add(config, remaining);
        }

        foreach (InventorySlot slot in slots)
        {
            if (remaining <= 0)
                break;

            if (slot.IsEmpty)
                remaining = slot.Add(config, remaining);
        }

        return remaining;
    }

    public void NotifyChanged()
    {
        Changed?.Invoke();
    }
}
