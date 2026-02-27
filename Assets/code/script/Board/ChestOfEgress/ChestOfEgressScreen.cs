using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CanvasGroup))]
public class ChestOfEgressScreen : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private ChestOfEgressInventory _chestInventory;
    [SerializeField] private Transform _playerItemsPanel;
    [SerializeField] private Transform _chestItemsPanel;
    [SerializeField] private GameObject _itemSelectablePrefab;

    private ChestOfEgressTile _openedTile;
    private TranslatorUI _translator;
    private CanvasGroup _canvasGroup;

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
        if (_chestInventory == null)
        {
            Debug.LogError("[ChestOfEgressScreen] _chestInventory is null.", this);
            return false;
        }
        if (_playerItemsPanel == null)
        {
            Debug.LogError("[ChestOfEgressScreen] _playerItemsPanel is null.", this);
            return false;
        }
        if (_chestItemsPanel == null)
        {
            Debug.LogError("[ChestOfEgressScreen] _chestItemsPanel is null.", this);
            return false;
        }
        if (_itemSelectablePrefab == null)
        {
            Debug.LogError("[ChestOfEgressScreen] _itemSelectablePrefab is null.", this);
            return false;
        }
        return true;
    }

    public void Open(ChestOfEgressTile tile)
    {
        if (!ValidateSetup()) return;

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
        _openedTile = null;
    }

    private void Refresh()
    {
        RefreshPanel(_playerItemsPanel, GameManager.Instance.PlayerStats.Inventory, false);
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
            go.GetComponent<ItemSelectable>().SetItem(null, item);
        }
    }

    public void MoveItemToChest(Item item)
    {
        if (_chestInventory.IsFull()) return;

        GameManager.Instance.PlayerStats.RemoveItemFromInventory(item);
        _chestInventory.AddItem(item);
        Refresh();
    }

    public void MoveItemToPlayer(Item item)
    {
        if (!GameManager.Instance.PlayerStats.AddItemToInventory(item)) return;

        _chestInventory.RemoveItem(item);
        Refresh();
    }
}