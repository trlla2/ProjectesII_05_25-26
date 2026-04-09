using TMPro;
//using UnityEditor.Experimental.GraphView;
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
    [SerializeField]
    private TMP_Text selectedItemDescription;
    [SerializeField]
    private ItemSelectable newItemSelectable;

    private TranslatorUI translatorUI;

    private ItemSelectable currentSelectedItem;

    private void Awake()
    {
        translatorUI = GetComponent<TranslatorUI>();
    }

    private void ShowRemoveItems(Item newItem)
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

        newItemSelectable.SetItem(this,newItem);

        //newItemSelectable.Select();
    }

    public void ShowPanel(Item newItem)
    {
        ShowRemoveItems(newItem);
        translatorUI.ToTarget();
    }

    public void RemoveItem()
    {
        if (currentSelectedItem != null)
        {
            if (currentSelectedItem == newItemSelectable)
            {
                chestScreenRef.CloseChest();
            }
            else
            {
                GameManager.Instance.PlayerStats.RemoveItemFromInventory(currentSelectedItem.Item);
                chestScreenRef.KeepItem();
            }
        }
    }

    public void SelectItem(ItemSelectable selectedItem)
    {
        if (currentSelectedItem != null)
            currentSelectedItem.UnSelect(); 

        currentSelectedItem = selectedItem;
        selectedItemDescription.text = currentSelectedItem.Item.Description;
    }


}

