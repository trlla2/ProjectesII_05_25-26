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

    private void OnEnable() { RefreshInventoryUI(); }

    private void RefreshInventoryUI()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        foreach (Item item in GameManager.Instance.PlayerStats.Inventory)
        {
            GameObject itemUI = Instantiate(itemUIPrefab, transform);
            itemUI.GetComponent<Image>().sprite = item.Sprite;
            itemUI.GetComponent<TranslatorUI>().ToTarget();

        }
    }

    private void OnDestroy()
    {
        GameManager.Instance.PlayerStats.OnInventoryChange -= RefreshInventoryUI;
    }   
}
