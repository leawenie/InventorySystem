using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlotInput : MonoBehaviour, IPointerDownHandler
{
    private InventoryInteractionController controller;
    private int slotIndex = -1;
    private Inventory container;

    public void Init(InventoryInteractionController controller, Inventory container, int slotIndex)
    {
        this.controller = controller;
        this.slotIndex = slotIndex;
        this.container = container;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (controller == null || slotIndex < 0 || container == null)
            return;

        controller.HandlePointerDown(container, slotIndex, eventData);
    }
}
