using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopScreen : MonoBehaviour
{
    [Header("Item Slots")]
    [SerializeField] private List<ShopSlotUI> _itemSlots;

    [Header("Utility Slots")]
    [SerializeField] private ShopSlotUI _healSlot;
    [SerializeField] private ShopSlotUI _statSlot;
    [SerializeField] public List<StatUpgradeData> _statUpgradeData;

    [Header("Reroll")]
    [SerializeField]
    private TMP_Text _rerollText;
    [SerializeField]
    private Button _rerollButton;

    [Header("Feedback")]
    [SerializeField] private SellerBehaviour _seller;

    [Header("Utility")]
    [SerializeField] private RemoveItemPanelHandler removePanel;

    private ShopTile _currentTile;
    private TranslatorUI _translator;

    [SerializeField] private MoneyUIHandler moneyUI;

    private int _maxRerolls = 3;
    private int _currentRerolls = 0;
    private int _costReroll = 50;

    private void Awake()
    {
        _translator = GetComponent<TranslatorUI>();
    }

   

    public void Open(ShopTile tile)
    {
        _rerollText.text = $"{_costReroll}";
        _currentRerolls = 0;
        _rerollButton.interactable = true;
        _currentTile = tile;
        Camera.main.GetComponent<TileClickHandler>().HandleBoardRaycast(false);
        RefreshSlots();
        _seller.EnterShopReacion();
        _translator.ToTarget();
        moneyUI.StayVisible();
    }

    public void Close()
    {
        moneyUI.UnShow();
        _seller.CloseShopReaction();
        _translator.ToOrigin();
        Camera.main.GetComponent<TileClickHandler>().HandleBoardRaycast(true);
        _currentTile.Replace();
        _currentTile = null;
    }

    public void RerollOffers()
    {
        _rerollText.text = $"{_costReroll}";


        if (!GameManager.Instance.PlayerStats.UseMoney(_costReroll))
            return;


        _currentRerolls++;
        _currentTile.GenerateOffers();
        RefreshSlots();

        if (_currentRerolls >= _maxRerolls)
        {
            _rerollButton.interactable = false;
            _rerollText.text = $"SoldOut";
            return;
        }
    }

    public void RefreshSlots(/*bool items, bool heal, bool stat*/)
    {
        if (_currentTile == null) return;

        List<ShopOffer> itemOffers = new List<ShopOffer>(_currentTile.ItemOffers);
        for (int i = 0; i < _itemSlots.Count; i++)
        {
            if (i < itemOffers.Count)
                _itemSlots[i].Setup(itemOffers[i], this);
            else
                _itemSlots[i].gameObject.SetActive(false);
        }

        if (_healSlot != null && _currentTile.HealOffer != null)
            _healSlot.Setup(_currentTile.HealOffer, this);

        if (_statSlot != null && _currentTile.StatOffer != null)
        {
            _statSlot.Setup(_currentTile.StatOffer, this);
        }
            
    }

    public void TryBuy(ShopOffer offer)
    {
        if (offer.IsPurchased)
        {
            _seller.OutOfStockReaction();
            return;
        }

        PlayerStats stats = GameManager.Instance.PlayerStats;

        if (!stats.UseMoney(offer.Cost))
        {
            _seller.NotEnoughMoneyReaction();
            return;
        }

        switch (offer.Type)
        {
            case ShopOffer.OfferType.Item:
                if (!stats.AddItemToInventory(offer.Item))
                {
                    removePanel.ShowPanel(offer.Item, true);
                    _seller.InventoryFullReaction();
                }
                else
                    _seller.PurchasedItemReaction();
                break;

            case ShopOffer.OfferType.Heal:
                //float healAmount = (stats.MaxHp + stats.ExtraHealth) * offer.HealPercent;
                stats.HealPercentage(offer.HealPercent);
                _seller.PurchasedHealReaction();
                break;

            case ShopOffer.OfferType.StatUpgrade:
                ApplyStatUpgrade(stats, offer);
                _seller.PurchasedItemReaction();
                break;
        }

        offer.MarkAsPurchased();
        RefreshSlots();
    }

    private void ApplyStatUpgrade(PlayerStats stats, ShopOffer offer)
    {
        switch (offer.StatType)
        {
            case StatUpgradeType.ExtraDamage:
                //int extraDmg = Mathf.RoundToInt(offer.StatAmount + stats.ExtraDamage * offer.StatPercent);
                stats.ModifyExtraDamageUpgradesLvl(1);
                break;

            case StatUpgradeType.MaxHealth:
                //int extraHp = Mathf.RoundToInt(offer.StatAmount + (stats.MaxHp + stats.ExtraHealth) * offer.StatPercent);
                stats.ModifyExtraHealthUpgradesLvl(1);
                break;

            case StatUpgradeType.Evasion:
                //int extraEvasion = Mathf.RoundToInt(offer.StatAmount + stats.Evasion * offer.StatPercent);
                stats.ModifyEvasionUpgradesLvl(1);
                break;

            case StatUpgradeType.Defense:
                //float extraDef = offer.StatAmount + stats.Defense * offer.StatPercent;
                stats.ModifyDefenseUpgradesLvl(1);
                break;
        }
    }

}