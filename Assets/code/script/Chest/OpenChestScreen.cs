using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class OpenChestScreen : MonoBehaviour
{
    public ChestTile openedChest;

    [Header("Items Setup")]
    [SerializeField] TMP_Text _itemNameText;
    [SerializeField] TMP_Text _itemDescriptionText;
    [SerializeField] Image _itemSprite;
    
    [SerializeField] private List<Item> _itemsPool;

    [Header("RemovePanel")]
    [SerializeField] RemoveItemPanelHandler _removeItemPanel;
    
    Item _selectedItem;

    private TranslatorUI _translator;

    private void Awake() {  _translator = GetComponent<TranslatorUI>(); }

    private void Start()
    {
        Pull();
        _translator.ToTarget();
    }
    private void OnEnable() { _translator.ToTarget(); }
    public void Pull()
    {
        _selectedItem = _itemsPool[Random.Range(0, _itemsPool.Count)];

        _itemNameText.text = _selectedItem.nameItem;
        _itemDescriptionText.text = _selectedItem.description;
        _itemSprite.sprite = _selectedItem.Sprite;

        Debug.Log("New item pulled");
    }

    public void KeepItem()
    {
        if (!GameManager.Instance.AddItemToPlayerInventory(_selectedItem)) // If Cant Be Added
            _removeItemPanel.ShowPanel();
        else
            CloseChest();
    }
    public void CloseChest()
    {
        HapticsManager.Instance.GenerateHapticFeedback(HapticsManager.HapticTypes.RigidImpact);
        openedChest.Replace();
        Camera.main.GetComponent<TileClickHandler>().HandleBoardRaycast(true);
        _translator.ToOrigin();
    }
}
