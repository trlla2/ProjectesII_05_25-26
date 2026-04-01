using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class OpenChestScreen : MonoBehaviour
{
    public ChestTile openedChest;

    [Header("Items Setup")]
    [SerializeField] TMP_Text _itemNameText;
    [SerializeField] TMP_Text _itemDescriptionText;
    
    [SerializeField] private List<Item> _itemsPool;

    [Header("RemovePanel")]
    [SerializeField] RemoveItemPanelHandler _removeItemPanel;

    [SerializeField] ChestAnimationHandler _animationHandler;
    
    Item _selectedItem;

    private TranslatorUI _translator;

    private void Awake() {  _translator = GetComponent<TranslatorUI>(); }

    private void Start()
    {
        _translator.ToTarget();
    }
    private void OnEnable() { _translator.ToTarget(); }
    public void Pull(ItemRarity rarity)
    {
        List<Item> candidates = new List<Item>();

        foreach (Item item in _itemsPool)
        {
            if(item.Rarity == rarity)
                candidates.Add(item);
        }

        _selectedItem = candidates[Random.Range(0, candidates.Count - 1)];

        

        Debug.Log("New item pulled");
    }

    public void OpenChest()
    {
        _animationHandler.LoadItem(_selectedItem);

        _animationHandler.OnOpen.AddListener(() =>
        {
            _itemNameText.text = _selectedItem.NameItem;
            _itemDescriptionText.text = _selectedItem.Description;
        });
    }

    public void KeepItem()
    {
        if (!GameManager.Instance.PlayerStats.AddItemToInventory(_selectedItem)) // If Cant Be Added
            _removeItemPanel.ShowPanel(_selectedItem);
        else
            CloseChest();
    }
    public void CloseChest()
    {
        HapticsManager.Instance.GenerateHapticFeedback(HapticsManager.HapticTypes.RigidImpact);
        openedChest.Replace();
        Camera.main.GetComponent<TileClickHandler>().HandleBoardRaycast(true);
        _animationHandler.ResetAnimation();
        _translator.ToOrigin();
    }
}
