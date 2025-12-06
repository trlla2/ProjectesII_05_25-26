using UnityEngine;
using UnityEngine.UI;

public class InventoryUIHandler : MonoBehaviour
{
    [Header("SETUP")]
    [SerializeField]
    private GameObject itemUIPrefab;
    private void Start()
    {
        GameManager.Instance.OnPlayerInventoryChange += RefreshInventoryUI;
        RefreshInventoryUI();
    }

    private void RefreshInventoryUI()
    {

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var item in GameManager.Instance.GetPlayerInventory())
        {
            GameObject itemUI = Instantiate(itemUIPrefab, transform);
            itemUI.GetComponent<Image>().sprite = item.enemySprite;

        }
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnPlayerInventoryChange -= RefreshInventoryUI;
    }   
}
