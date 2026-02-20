using UnityEngine;

[RequireComponent(typeof(TranslatorUI))]
public class RemoveItemPanelHandler : MonoBehaviour
{

    [Header("Setup")]
    [SerializeField]
    private Transform itemsPanel;
    [SerializeField]
    private GameObject itemSelectablePrefab;
    [SerializeField]
    private OpenChestScreen chestScreenRef;

    private TranslatorUI translatorUI;

    private ItemSelectable currentSelectedItem;

    private void Awake()
    {
        translatorUI = GetComponent<TranslatorUI>();
    }

    private void ShowRemoveItems()
    {
        foreach (Transform child in itemsPanel)
        {
            Destroy(child.gameObject);
        }

        foreach (Item item in GameManager.Instance.PlayerStats.Inventory)
        {
            GameObject itemUI = Instantiate(itemSelectablePrefab, itemsPanel);
            itemUI.GetComponent<ItemSelectable>().SetItem(this,item);
        }
    }

    public void ShowPanel()
    {
        ShowRemoveItems();
        translatorUI.ToTarget();
    }
    public void RemoveItem()
    {
        if (currentSelectedItem != null)
        {
            GameManager.Instance.PlayerStats.RemoveItemFromInventory(currentSelectedItem.Item);
            translatorUI.ResetPosition();
        }
    }

    public void SelectItem(ItemSelectable selectedItem)
    {
        if (currentSelectedItem != null)
            currentSelectedItem.UnSelect(); 

        currentSelectedItem = selectedItem;
    }


}

