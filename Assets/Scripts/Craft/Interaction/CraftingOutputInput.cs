using UnityEngine;
using UnityEngine.EventSystems;

public class CraftingOutputInput : MonoBehaviour, IPointerDownHandler
{
    private InventoryInteractionController controller;

    public void Init(InventoryInteractionController controller)
    {
        this.controller = controller;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (controller == null)
            return;

        controller.HandleCraftResult(eventData);
    }
}
