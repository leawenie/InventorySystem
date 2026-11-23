using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 物品父类
/// </summary>
public class Item
{
    public cfg.ItemConfig ItemConfig { get; init; }
    public Sprite Sprite { get; init; }

    public Item(cfg.ItemConfig itemConfig, Sprite sprite)
    {
        ItemConfig = itemConfig;
        Sprite = sprite;
    }
}
