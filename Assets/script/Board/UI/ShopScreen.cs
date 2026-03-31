using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopScreen : MonoBehaviour
{
    [Header("Slots")]
    [SerializeField] private List<ShopSlotUI> _slots;

    [Header("Feedback")]
    [SerializeField] private TMP_Text _feedbackText;

    [Header("Player Gold Display")]
    [SerializeField] private TMP_Text _goldText;

    private ShopTile _currentTile;
    private TranslatorUI _translator;

    private void Awake()
    {
        _translator = GetComponent<TranslatorUI>();
    }

    public void Open(ShopTile tile)
    {
        _currentTile = tile;
        Camera.main.GetComponent<TileClickHandler>().HandleBoardRaycast(false);
        RefreshGold();
        RefreshSlots();
        _translator.ToTarget();
    }

    public void Close()
    {
        _translator.ToOrigin();
        Camera.main.GetComponent<TileClickHandler>().HandleBoardRaycast(true);
        _currentTile.Replace();
        _currentTile = null;
    }

    private void RefreshSlots()
    {
        if (_currentTile == null || _currentTile.Offers == null) return;

        for (int i = 0; i < _slots.Count; i++)
        {
            if (i < _currentTile.Offers.Count)
                _slots[i].Setup(_currentTile.Offers[i], this);
            else
                _slots[i].gameObject.SetActive(false);
        }
    }

    public void TryBuy(ShopOffer offer)
    {
        PlayerStats stats = GameManager.Instance.PlayerStats;

        if (!stats.UseMoney(offer.Cost))
        {
            ShowFeedback("¡Oro insuficiente!");
            return;
        }

        switch (offer.Type)
        {
            case ShopOffer.OfferType.Item:
                if (!stats.AddItemToInventory(offer.Item))
                {
                    // Devolver el dinero si el inventario está lleno
                    stats.AddMoney(offer.Cost);
                    ShowFeedback("¡Inventario lleno!");
                    return;
                }
                ShowFeedback($"¡{offer.Item.NameItem} comprado!");
                break;

            case ShopOffer.OfferType.Heal:
                float healAmount = (stats.MaxHp + stats.ExtraHealth) * offer.HealPercent;
                stats.Heal(healAmount);
                ShowFeedback($"¡Curado un {offer.HealPercent * 100f:0}% de HP!");
                break;

            case ShopOffer.OfferType.StatUpgrade:
                ApplyStatUpgrade(stats, offer);
                ShowFeedback($"¡{offer.StatType} mejorado!");
                break;
        }

        RefreshGold();
    }

    private void ApplyStatUpgrade(PlayerStats stats, ShopOffer offer)
    {
        switch (offer.StatType)
        {
            case StatUpgradeType.ExtraDamage:
                stats.ModifyExtraDamage((int)offer.StatAmount);
                break;
            case StatUpgradeType.MaxHealth:
                stats.ModifyExtraHealth((int)offer.StatAmount);
                break;
            case StatUpgradeType.Evasion:
                stats.ModifyEvasion((int)offer.StatAmount);
                break;
            case StatUpgradeType.Defense:
                stats.ModifyDefense(offer.StatAmount);
                break;
        }
    }

    private void RefreshGold()
    {
        if (_goldText != null)
            _goldText.text = $"Oro: {GameManager.Instance.PlayerStats.Money}";
    }

    private void ShowFeedback(string message)
    {
        if (_feedbackText != null)
            _feedbackText.text = message;
    }
}