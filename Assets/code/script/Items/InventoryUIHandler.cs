using UnityEngine;
using UnityEngine.UI;

public class InventoryUIHandler : MonoBehaviour
{
    [Header("SETUP")]
    [SerializeField]
    private GameObject itemUIPrefab;
    private void Start()
    {
        PlayerStats.Instance.OnInventoryChange += RefreshInventoryUI;
        RefreshInventoryUI();
    }

    private void RefreshInventoryUI()
    {
        
        foreach (var item in PlayerStats.Instance.GetInventory())
        {
            GameObject itemUI = Instantiate(itemUIPrefab, transform);
            itemUI.GetComponent<Image>().sprite = item.enemySprite;

        }
    }

    private void OnDestroy()
    {
        PlayerStats.Instance.OnInventoryChange -= RefreshInventoryUI;
    }   
}
