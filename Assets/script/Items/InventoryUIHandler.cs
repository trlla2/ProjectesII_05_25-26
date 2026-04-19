using UnityEngine;
using UnityEngine.UI;

public class InventoryUIHandler : MonoBehaviour
{
    [Header("SETUP")]
    [SerializeField] private GameObject itemUIPrefab;

    private void Start()
    {
        GameManager.Instance.PlayerStats.OnInventoryChange += RefreshInventoryUI;
        RefreshInventoryUI();
    }

    private void OnEnable()
    {
        RefreshInventoryUI();
    }

    private void RefreshInventoryUI()
    {

        // Clear existing UI elements
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        //Create UI for each item in inventory
        foreach (Item item in GameManager.Instance.PlayerStats.Inventory)
        {
            GameObject itemUI = Instantiate(itemUIPrefab, transform);
            
            ItemUIButton itemButton = itemUI.GetComponent<ItemUIButton>();
            if (itemButton == null)
            {
                Debug.LogError("No ITEM BUTTON SCRIPT attached");
            }
            else
            {
                itemButton.Initialize(item);
            }

            TranslatorUI translator = itemUI.GetComponent<TranslatorUI>();
            if (translator != null)
            {
                translator.ToTarget();
            }
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null && GameManager.Instance.PlayerStats != null)
        {
            GameManager.Instance.PlayerStats.OnInventoryChange -= RefreshInventoryUI;
        }
    }   
}
