using UnityEngine;

public static class InventoryTransfer
{
    public static int Transfer(
        Inventory sourceInventory,
        int sourceIndex,
        Inventory targetInventory,
        int targetStart,
        int targetCount
    )
    {
        if (sourceInventory == null)
            throw new System.ArgumentNullException(nameof(sourceInventory));
        if (targetInventory == null)
            throw new System.ArgumentNullException(nameof(targetInventory));

        InventorySlot source = sourceInventory.GetSlot(sourceIndex);

        if (targetStart < 0 || targetStart > targetInventory.Capacity)
            throw new System.ArgumentOutOfRangeException(nameof(targetStart));

        if (targetCount < 0 || targetCount > targetInventory.Capacity - targetStart)
            throw new System.ArgumentOutOfRangeException(nameof(targetCount));

        if (source.IsEmpty || targetCount == 0)
            return 0;

        cfg.ItemConfig config = source.Config;
        int originalCount = source.Count;
        int remaining = originalCount;
        int targetEnd = targetStart + targetCount;

        for (int i = targetStart; i < targetEnd && remaining > 0; i++)
        {
            InventorySlot target = targetInventory.GetSlot(i);

            if (ReferenceEquals(source, target))
                continue;
            if (target.IsEmpty || target.Config.Id != config.Id)
                continue;

            remaining = target.Add(config, remaining);
        }

        for (int i = targetStart; i < targetEnd && remaining > 0; i++)
        {
            InventorySlot target = targetInventory.GetSlot(i);

            if (ReferenceEquals(source, target))
                continue;
            if (!target.IsEmpty)
                continue;

            remaining = target.Add(config, remaining);
        }

        int transferred = originalCount - remaining;
        if (transferred == 0)
            return 0;

        source.Remove(transferred);

        sourceInventory.NotifyChanged();

        if (!ReferenceEquals(sourceInventory, targetInventory))
            targetInventory.NotifyChanged();

        return transferred;
    }
}
