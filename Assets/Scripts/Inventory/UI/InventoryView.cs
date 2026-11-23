using TMPro;
using UnityEngine;

public class InventoryView : MonoBehaviour
{
    [SerializeField]
    private InventorySlotView slotPrefab;

    [SerializeField]
    private Transform mainArea;

    [SerializeField]
    private Transform hotbarArea;

    [SerializeField]
    private Transform craftingArea;

    [SerializeField]
    private Transform craftedArea;

    [Header("鼠标持有物体")]
    [SerializeField]
    private RectTransform cursorLayer;

    [SerializeField]
    private RectTransform cursorVisual;

    [SerializeField]
    private UnityEngine.UI.Image cursorIcon;

    [SerializeField]
    private TextMeshProUGUI cursorCount;

    private readonly System.Collections.Generic.Dictionary<string, Sprite> icons = new();
    private readonly System.Collections.Generic.HashSet<string> missingIcons = new();

    private Inventory inventory;
    private InventoryInteraction interaction;
    private InventorySlotView[] slotViews;

    public void Init(
        Inventory inventory,
        InventoryInteraction interaction,
        InventoryInteractionController controller,
        int hotbarCapacity
    )
    {
        if (this.inventory != null)
            return;

        if (hotbarCapacity < 0 || hotbarCapacity > inventory.Capacity)
            throw new System.ArgumentOutOfRangeException(nameof(hotbarCapacity));

        this.inventory = inventory;
        this.interaction = interaction;

        foreach (Sprite sprite in Resources.LoadAll<Sprite>("Images/items_2x4"))
        {
            icons.TryAdd(sprite.name, sprite);
        }

        slotViews = new InventorySlotView[inventory.Capacity];

        for (int i = 0; i < inventory.Capacity; i++)
        {
            Transform parent = i < hotbarCapacity ? hotbarArea : mainArea;

            InventorySlotView view = Instantiate(slotPrefab, parent, false);

            InventorySlotInput input = view.GetComponent<InventorySlotInput>();

            if (input == null)
                throw new System.InvalidOperationException("格子预制体缺少 InventorySlotInput");

            input.Init(controller, inventory, i);
            slotViews[i] = view;
        }

        inventory.Changed += RefreshSlots;
        interaction.HeldChanged += RefreshCursor;

        RefreshSlots();
        RefreshCursor();
    }

    public void RefreshSlots()
    {
        if (inventory == null)
            return;

        for (int i = 0; i < inventory.Capacity; i++)
        {
            InventorySlot slot = inventory.GetSlot(i);

            slotViews[i].Render(slot.Config, slot.Count, GetIcon(slot.Config));
        }
    }

    public void RefreshCursor()
    {
        if (interaction == null)
            return;

        bool holding = interaction.IsHolding;
        cursorVisual.gameObject.SetActive(holding);

        if (!holding)
            return;

        cursorIcon.sprite = GetIcon(interaction.HeldConfig);
        cursorIcon.enabled = cursorIcon.sprite != null;

        cursorCount.text = interaction.HeldCount > 1 ? interaction.HeldCount.ToString() : "";
        cursorCount.enabled = interaction.HeldCount > 1;

        cursorLayer.SetAsLastSibling();
        cursorVisual.SetAsLastSibling();
    }

    public void SetCursorPosition(Vector2 screenPosition)
    {
        Canvas canvas = cursorLayer.GetComponentInParent<Canvas>();
        Canvas root = canvas.rootCanvas;

        Camera camera = root.renderMode == RenderMode.ScreenSpaceOverlay ? null : root.worldCamera;

        if (
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                cursorLayer,
                screenPosition,
                camera,
                out Vector3 position
            )
        )
        {
            cursorVisual.position = position;
        }
    }

    public void HideCursor()
    {
        cursorVisual.gameObject.SetActive(false);
    }

    private Sprite GetIcon(cfg.ItemConfig config)
    {
        if (config == null)
            return null;

        if (icons.TryGetValue(config.Icon, out Sprite sprite))
            return sprite;

        if (missingIcons.Add(config.Icon))
        {
            Debug.LogError($"物品 {config.Id} 缺少图标：{config.Icon}", this);
        }

        return null;
    }

    void OnEnable()
    {
        RefreshCursor();
        RefreshSlots();
    }

    private void OnDisable()
    {
        HideCursor();
    }

    void OnDestroy()
    {
        if (inventory != null)
            inventory.Changed -= RefreshSlots;

        if (interaction != null)
            interaction.HeldChanged -= RefreshCursor;
    }
}
