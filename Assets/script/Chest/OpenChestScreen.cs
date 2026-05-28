using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class OpenChestScreen : MonoBehaviour
{
    public ChestTile openedChest;

    [Header("Items Setup")]
    [SerializeField] TMP_Text _itemNameText;
    [SerializeField] TranslatorUI _itemNameTranslator;
    [SerializeField] TMP_Text _itemDescriptionText;
    [SerializeField] TranslatorUI _itemDescriptionTranslator;
    
    [SerializeField] private List<Item> _itemsPool;

    [Header("SETUP ART")]
    [SerializeField] private Image _headerImage;
    [SerializeField] private Sprite _legendaryHeader;
    [SerializeField] private Sprite _epicHeader;
    [SerializeField] private Sprite _rareHeader;
    [SerializeField] private Sprite _commonHeader;

    [Header("RemovePanel")]
    [SerializeField] RemoveItemPanelHandler _removeItemPanel;

    [SerializeField] ChestAnimationHandler _animationHandler;
    
    Item _selectedItem;

    private TranslatorUI _translator;

    private void Awake() {  _translator = GetComponent<TranslatorUI>(); }
    public void Open() { _translator.ToTarget(); }
    public void Pull(ItemRarity rarity)
    {
        List<Item> candidates = new List<Item>();

        foreach (Item item in _itemsPool)
        {
            if(item.Rarity == rarity)
                candidates.Add(item);
        }
        _selectedItem = candidates[Random.Range(0, candidates.Count)];

        switch (_selectedItem.Rarity)
        {
            case ItemRarity.LEGENDARY:
                _headerImage.sprite = _legendaryHeader;
                break;
            case ItemRarity.EPIC:
                _headerImage.sprite = _epicHeader;
                break;
            case ItemRarity.RARE:
                _headerImage.sprite = _rareHeader;
                break;
            case ItemRarity.COMMON:
                _headerImage.sprite = _commonHeader;
                break;
        }

        Debug.Log("New item pulled");
    }

    public void OpenChest()
    {
        _animationHandler.LoadItem(_selectedItem);

        _animationHandler.OnOpen.AddListener(() =>
        {
            _itemNameText.text = LocalizationSettings.StringDatabase.GetLocalizedString("ItemNames", _selectedItem.NameItem);
            _itemDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("ItemDescriptions", _selectedItem.NameItem);
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
