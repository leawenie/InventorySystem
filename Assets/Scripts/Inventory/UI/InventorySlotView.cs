using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class InventorySlotView : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI countText;

    public void Render(cfg.ItemConfig config, int count, Sprite sprite)
    {
        bool hasItem = config != null && count > 0;

        icon.sprite = hasItem ? sprite : null;
        icon.enabled = hasItem && sprite != null;

        countText.text = hasItem && count > 1 ? count.ToString() : "";

        countText.enabled = hasItem && count > 1;
    }
}
