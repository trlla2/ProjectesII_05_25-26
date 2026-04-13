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
            _statSlot.Setup(_currentTile.StatOffer, this);
    }

    public void TryBuy(ShopOffer offer)
    {
        if (offer.IsPurchased)
        {
            ShowFeedback("¡Ya comprado!");
            return;
        }

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

        offer.MarkAsPurchased();
        RefreshGold();
        RefreshSlots();
    }

    private void ApplyStatUpgrade(PlayerStats stats, ShopOffer offer)
    {
        switch (offer.StatType)
        {
            case StatUpgradeType.ExtraDamage:
                int extraDmg = Mathf.RoundToInt(offer.StatAmount + stats.ExtraDamage * offer.StatPercent);
                stats.ModifyExtraDamage(extraDmg);
                break;

            case StatUpgradeType.MaxHealth:
                int extraHp = Mathf.RoundToInt(offer.StatAmount + (stats.MaxHp + stats.ExtraHealth) * offer.StatPercent);
                stats.ModifyExtraHealth(extraHp);
                break;

            case StatUpgradeType.Evasion:
                int extraEvasion = Mathf.RoundToInt(offer.StatAmount + stats.Evasion * offer.StatPercent);
                stats.ModifyEvasion(extraEvasion);
                break;

            case StatUpgradeType.Defense:
                float extraDef = offer.StatAmount + stats.Defense * offer.StatPercent;
                stats.ModifyDefense(extraDef);
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
        {
            _feedbackText.text = message;
            StartCoroutine(ClearFeedbackText());
        }
    }

    private IEnumerator ClearFeedbackText()
    {
        yield return new WaitForSeconds(1f);
        _feedbackText.text = "";
    }
}