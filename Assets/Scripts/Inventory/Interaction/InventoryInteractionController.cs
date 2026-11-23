using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryInteractionController : MonoBehaviour
{
    [SerializeField]
    private InventoryView view;

    [SerializeField]
    private GameObject panel;

    private InventoryInteraction interaction;
    private Inventory playerInventory;
    private CraftingStation station;

    public void Init(
        InventoryInteraction interaction,
        Inventory playerInventory,
        CraftingStation station
    )
    {
        this.playerInventory = playerInventory;
        this.interaction = interaction;
        this.station = station;
    }

    public void HandlePointerDown(Inventory container, int index, PointerEventData eventData)
    {
        if (interaction == null || !panel.activeInHierarchy)
            return;

        bool left = eventData.button == PointerEventData.InputButton.Left;

        bool right = eventData.button == PointerEventData.InputButton.Right;

        if (!left && !right)
            return;

        if (
            !ReferenceEquals(container, playerInventory)
            && !ReferenceEquals(container, station.Ingredients)
        )
        {
            return;
        }

        bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (shift)
        {
            if (ReferenceEquals(container, playerInventory))
            {
                interaction.QuickMove(index);
            }
            else
            {
                // 工作台材料快速取回玩家背包。
                InventoryTransfer.Transfer(
                    station.Ingredients,
                    index,
                    playerInventory,
                    0,
                    playerInventory.Capacity
                );
            }
        }
        else if (left)
        {
            interaction.LeftClick(container, index);
        }
        else
        {
            interaction.RightClick(container, index);
        }

        view.SetCursorPosition(eventData.position);
    }

    public void HandleCraftResult(PointerEventData eventData)
    {
        if (interaction == null || !panel.activeInHierarchy)
            return;

        bool left = eventData.button == PointerEventData.InputButton.Left;

        bool right = eventData.button == PointerEventData.InputButton.Right;

        if (!left && !right)
            return;

        bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (shift)
            return;

        station.TryTakeResult(interaction);

        view.SetCursorPosition(eventData.position);
    }

    private void Update()
    {
        if (interaction == null || !panel.activeInHierarchy)
            return;

        if (interaction.IsHolding)
            view.SetCursorPosition(Input.mousePosition);

        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape))
            TryClose();
    }

    public bool TryClose()
    {
        if (interaction == null)
            return false;

        if (!interaction.TryReturnHeld())
            return false;

        if (!station.TryReturnIngredients(playerInventory))
            return false;

        view.HideCursor();
        panel.SetActive(false);
        return true;
    }

    public void Open()
    {
        panel.SetActive(true);
    }
}
