using UnityEngine;

public sealed class InventoryManager : MonoBehaviour
{
    [SerializeField]
    private InventoryView view;

    [SerializeField]
    private InventoryInteractionController controller;

    [SerializeField]
    private CraftingStationView stationView;

    public Inventory Inventory { get; private set; }
    public InventoryInteraction Interaction { get; private set; }
    public CraftingStation Station { get; private set; }

    private void Awake()
    {
        GameConfig.Load();

        int capacity = GameConfig.Tables.TbInventory.Capacity;
        int hotbarCapacity = GameConfig.Tables.TbInventory.HotBarCapacity;

        Inventory = new Inventory(capacity);
        Interaction = new InventoryInteraction(Inventory, hotbarCapacity);

        // Crafter 第一次使用发生在配置加载之后。
        Station = new CraftingStation(id => GameConfig.Tables.TbItem.GetOrDefault(id));

        // 演示数据先准备好，View 首次绑定就能显示。
        Inventory.Add(GameConfig.Tables.TbItem.Get(1001), 70);
        Inventory.Add(GameConfig.Tables.TbItem.Get(1002), 20);
        Inventory.Add(GameConfig.Tables.TbItem.Get(2001), 2);

        controller.Init(Interaction, Inventory, Station);
        view.Init(Inventory, Interaction, controller, hotbarCapacity);
        stationView.Init(Station, controller);
    }

    void OnDestroy()
    {
        Station?.Dispose();
    }
}
