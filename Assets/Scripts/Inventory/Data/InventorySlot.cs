using UnityEngine;

/// <summary>
/// 背包格子
/// 生命周期：游戏级
/// </summary>
public class InventorySlot
{
    public cfg.ItemConfig Config { get; private set; } = null;
    public int Count { get; private set; } = 0;
    public bool IsEmpty => Count == 0;
    public int SlotIndex { get; private set; } = -1;

    public InventorySlot(int slotIndex)
    {
        SlotIndex = slotIndex;
    }

    //返回未添加数量，为0表示全添加
    public int Add(cfg.ItemConfig config, int amount)
    {
        if (config == null)
            throw new System.ArgumentNullException(nameof(config), "该物品配置为空");

        if (amount < 0)
            throw new System.ArgumentOutOfRangeException(nameof(amount), "背包添加物品数量非法");

        if (amount == 0)
            return 0;

        if (IsEmpty)
        {
            Count = Mathf.Min(amount, config.MaxStack);
            Config = config;
            return amount - Count;
        }
        else
        {
            bool isSame = config.Id == Config.Id;
            if (!isSame)
                return amount;

            int canAddCount = Config.MaxStack - Count;
            canAddCount = Mathf.Min(canAddCount, amount);
            Count += canAddCount;
            return amount - canAddCount;
        }
    }

    //返回未添加数量，为0表示全添加
    public int AddAmount(int amount)
    {
        if (amount < 0)
            throw new System.ArgumentOutOfRangeException(nameof(amount), "背包添加物品数量非法");

        if (amount == 0)
            return 0;

        if (IsEmpty || Config == null)
            return amount;

        int canAddCount = Config.MaxStack - Count;
        canAddCount = Mathf.Min(canAddCount, amount);
        Count += canAddCount;
        return amount - canAddCount;
    }

    //返回成功移除的数量
    public int Remove(int amount)
    {
        if (amount < 0)
            throw new System.ArgumentOutOfRangeException(nameof(amount), "背包移除物品数量非法");

        if (Count <= 0)
            return 0;

        if (IsEmpty)
            return 0;

        int count = Count;
        Count -= amount;
        if (Count <= 0)
        {
            Clear();
            return count;
        }
        else
        {
            return amount;
        }
    }

    public void Clear()
    {
        Count = 0;
        Config = null;
    }
}
