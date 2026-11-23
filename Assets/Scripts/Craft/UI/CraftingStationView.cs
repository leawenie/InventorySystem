using UnityEngine;

public class CraftingStationView : MonoBehaviour
{
    [SerializeField]
    private InventorySlotView inputPrefab;

    [SerializeField]
    private InventorySlotView outputPrefab;

    [SerializeField]
    private Transform inputArea;

    [SerializeField]
    private Transform outputArea;

    private readonly System.Collections.Generic.Dictionary<string, Sprite> icons = new();
    private readonly System.Collections.Generic.HashSet<string> missingIcons = new();

    private CraftingStation station;
    private InventorySlotView[] inputs;
    private InventorySlotView output;

    public void Init(CraftingStation station, InventoryInteractionController controller)
    {
        if (this.station != null)
            return;
        this.station = station;

        foreach (Sprite sprite in Resources.LoadAll<Sprite>("Images/items_2x4"))
        {
            icons.TryAdd(sprite.name, sprite);
        }

        inputs = new InventorySlotView[station.Ingredients.Capacity];
        for (int i = 0; i < inputs.Length; i++)
        {
            inputs[i] = Instantiate(inputPrefab, inputArea, false);

            inputs[i].GetComponent<InventorySlotInput>().Init(controller, station.Ingredients, i);
        }

        output = Instantiate(outputPrefab, outputArea, false);
        output.GetComponent<CraftingOutputInput>().Init(controller);

        station.Ingredients.Changed += RefreshInputs;
        station.PreviewChanged += RefreshOutput;

        RefreshInputs();
        RefreshOutput();
    }

    private void RefreshInputs()
    {
        if (station == null)
            throw new System.ArgumentNullException(nameof(station));

        for (int i = 0; i < station.Ingredients.Capacity; i++)
        {
            InventorySlot slot = station.Ingredients.GetSlot(i);
            InventorySlotView view = inputs[i];

            view.Render(slot.Config, slot.Count, GetIcon(slot.Config));
        }
    }

    private void RefreshOutput()
    {
        if (station == null)
            throw new System.ArgumentNullException(nameof(station));

        output.Render(station.PreviewItem, station.PreviewCount, GetIcon(station.PreviewItem));
    }

    private Sprite GetIcon(cfg.ItemConfig config)
    {
        if (config == null)
            return null;

        if (icons.TryGetValue(config.Icon, out Sprite sprite))
            return sprite;

        if (missingIcons.Add(config.Icon))
            Debug.LogError($"物品 {config.Id} 缺少图标 {config.Icon}", this);

        return null;
    }

    void OnEnable()
    {
        RefreshInputs();
        RefreshOutput();
    }

    void OnDestroy()
    {
        if (station == null)
            return;

        station.Ingredients.Changed -= RefreshInputs;
        station.PreviewChanged -= RefreshOutput;
    }
}
