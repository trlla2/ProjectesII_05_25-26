using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class OpenChestScreen : MonoBehaviour
{
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

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Pull()
    {
        _selectedItem = _itemsPool[Random.Range(0, _itemsPool.Count)];

        _itemNameText.text = _selectedItem.nameItem;
        _itemDescriptionText.text = _selectedItem.description;
        _itemSprite.sprite = _selectedItem.enemySprite;
    }
}
