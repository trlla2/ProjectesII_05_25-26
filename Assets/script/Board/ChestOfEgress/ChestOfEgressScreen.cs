using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class ChestOfEgressScreen : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private ChestOfEgressInventory _chestInventory;
    [SerializeField] private Transform _playerItemsPanel;
    [SerializeField] private Transform _chestItemsPanel;
    [SerializeField] private GameObject _itemSelectablePrefab;

    [SerializeField] private TMP_Text itemName;
    [SerializeField] private TMP_Text itemInfo;

    [SerializeField] private Color commonTextColor;
    [SerializeField] private Color rareTextColor;
    [SerializeField] private Color epicTextColor;
    [SerializeField] private Color legendaryTextColor;

    [Header("Pagination")]
    [SerializeField] private int _itemsPerPage = 10;
    [SerializeField] private Button _nextPageBtn;
    [SerializeField] private Button _prevPageBtn;

    private ChestOfEgressTile _openedTile;
    private TranslatorUI _translator;
    private CanvasGroup _canvasGroup;

    private ItemSelectable _selectedItem;
    private int _currentPage = 0;

    private void Awake()
    {
        _translator = GetComponent<TranslatorUI>();
        _canvasGroup = GetComponent<CanvasGroup>();
        SetVisible(false);
    }

    public void SetVisible(bool visible)
    {
        _canvasGroup.alpha = visible ? 1f : 0f;
        _canvasGroup.interactable = visible;
        _canvasGroup.blocksRaycasts = visible;
    }

    private bool ValidateSetup()
    {
        if (_chestInventory == null) return false;
        if (_playerItemsPanel == null) return false;
        if (_chestItemsPanel == null) return false;
        if (_itemSelectablePrefab == null) return false;
        if (_nextPageBtn == null) return false;
        if (_prevPageBtn == null) return false;
        return true;
    }

    public void Open(ChestOfEgressTile tile)
    {
        if (!ValidateSetup()) return;

        _currentPage = 0;
        _openedTile = tile;
        SetVisible(true);
        Refresh();
        _translator.ToTarget();
        Camera.main.GetComponent<TileClickHandler>().HandleBoardRaycast(false);
    }

    public void Close()
    {
        _translator.ToOrigin();
        Camera.main.GetComponent<TileClickHandler>().HandleBoardRaycast(true);
        _openedTile.Replace();
        _openedTile = null;
    }

    private void Refresh()
    {
        _selectedItem = null;

        var playerItems = GameManager.Instance.PlayerStats.Inventory;
        int totalPages = Mathf.Max(1, Mathf.CeilToInt((float)playerItems.Count / _itemsPerPage));

        if (_currentPage >= totalPages) _currentPage = totalPages - 1;
        if (_currentPage < 0) _currentPage = 0;

        _prevPageBtn.interactable = _currentPage > 0;
        _nextPageBtn.interactable = _currentPage < totalPages - 1;

        List<Item> pageItems = new List<Item>();
        int startIndex = _currentPage * _itemsPerPage;
        for (int i = 0; i < _itemsPerPage && startIndex + i < playerItems.Count; i++)
        {
            pageItems.Add(playerItems[startIndex + i]);
        }

        RefreshPanel(_playerItemsPanel, pageItems, false);
        RefreshPanel(_chestItemsPanel, new List<Item>(_chestInventory.Items), true);
    }

    private void RefreshPanel(Transform panel, IList<Item> items, bool isChestPanel)
    {
        foreach (Transform child in panel)
            Destroy(child.gameObject);

        foreach (Item item in items)
        {
            GameObject go = Instantiate(_itemSelectablePrefab, panel);
            ChestOfEgressItemSlot slot = go.AddComponent<ChestOfEgressItemSlot>();
            slot.Setup(item, isChestPanel, this);
            ItemSelectable iS = go.GetComponent<ItemSelectable>();
            iS.SetItem(null, item);
            iS.OnSelect += () =>
            {
                if (!isChestPanel) return;
                _selectedItem?.UnSelect();
                _selectedItem = iS;
            };
        }
    }

    public void NextPage()
    {
        _currentPage++;
        Refresh();
    }

    public void PrevPage()
    {
        _currentPage--;
        Refresh();
    }

    public void MoveItemToChest(Item item)
    {
        if (_chestInventory.IsFull()) return;

        GameManager.Instance.PlayerStats.RemoveItemFromInventory(item);
        _chestInventory.AddItem(item);

        if (_selectedItem != null)
            MoveItemToPlayer(_selectedItem.Item);

        Refresh();
    }

    public void MoveItemToPlayer(Item item)
    {
        if (!GameManager.Instance.PlayerStats.AddItemToInventory(item)) return;

        _chestInventory.RemoveItem(item);
        Refresh();
    }

    public void SetItemInfo(Item i)
    {
        Color targetColor = commonTextColor;

        switch (i.Rarity)
        {
            case ItemRarity.COMMON: targetColor = commonTextColor; break;
            case ItemRarity.RARE: targetColor = rareTextColor; break;
            case ItemRarity.EPIC: targetColor = epicTextColor; break;
            case ItemRarity.LEGENDARY: targetColor = legendaryTextColor; break;
        }

        string hexColor = ColorUtility.ToHtmlStringRGBA(targetColor);

        itemName.text = $"<color=#{hexColor}>{i.NameItem}</color>:";
        itemInfo.text = i.Description;
    }
}