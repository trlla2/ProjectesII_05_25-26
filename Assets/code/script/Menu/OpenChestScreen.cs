using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class OpenChestScreen : MonoBehaviour
{
    public ChestTile openedChest;

    [SerializeField] TMP_Text _itemNameText;
    [SerializeField] TMP_Text _itemDescriptionText;
    [SerializeField] Image _itemSprite;

    
    [SerializeField] private List<ItemsType> _itemsPool;
    ItemsType _selectedItem;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Pull();
    }

    public void Pull()
    {
        _selectedItem = _itemsPool[Random.Range(0, _itemsPool.Count)];

        _itemNameText.text = _selectedItem.nameItem;
        _itemDescriptionText.text = _selectedItem.description;
        _itemSprite.sprite = _selectedItem.enemySprite;

        Debug.Log("New item pulled");
    }

    public void KeepItem()
    {
        PlayerStats.Instance.AddItemToInventory(_selectedItem);
    }

    public void CloseChest()
    {
        openedChest.Replace();
        this.gameObject.SetActive(false);
    }
}
