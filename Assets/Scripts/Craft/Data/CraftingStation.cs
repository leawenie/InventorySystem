using System;
using UnityEngine;

public sealed class CraftingStation : IDisposable
{
    public const int Width = 3;
    public const int Height = 3;

    public Inventory Ingredients { get; } = new Inventory(Width * Height);

    public cfg.ItemConfig PreviewItem { get; private set; }
    public int PreviewCount { get; private set; }

    public event Action PreviewChanged;

    private readonly Func<int, cfg.ItemConfig> findItem;

    public CraftingStation(Func<int, cfg.ItemConfig> findItem)
    {
        this.findItem = findItem ?? throw new System.ArgumentNullException(nameof(findItem));

        Ingredients.Changed += RefreshPreview;
        RefreshPreview();
    }

    private bool TryResolve(out cfg.ItemConfig item, out int count)
    {
        int[] sequence = new int[Ingredients.Capacity];
        bool hasIngredient = false;

        for (int i = 0; i < sequence.Length; i++)
        {
            InventorySlot slot = Ingredients.GetSlot(i);
            if (slot.IsEmpty)
                continue;

            sequence[i] = slot.Config.Id;
            hasIngredient = true;
        }

        item = null;
        count = 0;

        if (!hasIngredient)
            return false;

        CraftResult result = Crafter.CraftSomething(sequence);

        if (result.ItemId <= 0 || result.CraftCount <= 0)
            return false;

        item = findItem(result.ItemId);
        if (item == null)
            throw new System.InvalidOperationException($"合成产物不存在: {result.ItemId}");

        count = result.CraftCount;

        if (count > item.MaxStack)
            throw new System.InvalidOperationException(
                $"单次产物超过堆叠上限: {item.Id} x {count}"
            );

        return true;
    }

    private void RefreshPreview()
    {
        TryResolve(out cfg.ItemConfig item, out int count);

        PreviewItem = item;
        PreviewCount = count;
        PreviewChanged?.Invoke();
    }

    public bool TryTakeResult(InventoryInteraction cursor)
    {
        if (cursor == null)
            throw new ArgumentNullException(nameof(cursor));

        if (!TryResolve(out cfg.ItemConfig item, out int count))
            return false;

        if (!cursor.TryReceiveWithoutNotify(item, count))
            return false;

        for (int i = 0; i < Ingredients.Capacity; i++)
        {
            InventorySlot slot = Ingredients.GetSlot(i);
            if (!slot.IsEmpty)
                slot.Remove(1);
        }

        Ingredients.NotifyChanged();
        cursor.NotifyHeldChanged();
        return true;
    }

    public bool TryReturnIngredients(Inventory playerInventory)
    {
        if (playerInventory == null)
            throw new System.ArgumentNullException(nameof(playerInventory));

        bool changed = false;
        bool allReturned = true;

        for (int i = 0; i < Ingredients.Capacity; i++)
        {
            InventorySlot slot = Ingredients.GetSlot(i);
            if (slot.IsEmpty)
                continue;

            int originalCount = slot.Count;
            int remaining = playerInventory.AddWithoutNotify(slot.Config, originalCount);

            int transferred = originalCount - remaining;

            if (transferred > 0)
            {
                slot.Remove(transferred);
                changed = true;
            }

            if (!slot.IsEmpty)
                allReturned = false;
        }

        if (changed)
        {
            Ingredients.NotifyChanged();
            playerInventory.NotifyChanged();
        }

        return allReturned;
    }

    public void Dispose()
    {
        Ingredients.Changed -= RefreshPreview;
    }
}
