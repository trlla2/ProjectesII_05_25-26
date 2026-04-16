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
    private OpenChestScreen chestScreenRef = null;
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

    private void ShowRemoveItems(Item newItem, bool mustBeEquiped)
    {
        foreach (Transform child in itemsPanel)
        {
            Destroy(child.gameObject);
        }
        ItemSelectable i = null;
        foreach (Item item in GameManager.Instance.PlayerStats.Inventory)
        {
            GameObject itemUI = Instantiate(itemSelectablePrefab, itemsPanel);
            itemUI.GetComponent<ItemSelectable>().SetItem(this,item);
            i = itemUI.GetComponent<ItemSelectable>();
        }

        newItemSelectable.SetItem(this,newItem);


        if(mustBeEquiped)
        {
            i.Select();
            newItemSelectable.CanBeSelected = false;

        }
    }

    public void ShowPanel(Item newItem, bool mustBeEquiped = false)
    {
        ShowRemoveItems(newItem, mustBeEquiped);
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
                if (chestScreenRef != null)
                    chestScreenRef.KeepItem();
                else
                {
                    GameManager.Instance.PlayerStats.AddItemToInventory(newItemSelectable.Item);
                    translatorUI.ToOrigin();
                }
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

