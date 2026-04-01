using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopSlotUI : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _descriptionText;
    [SerializeField] private TMP_Text _costText;
    [SerializeField] private Button _buyButton;

    private ShopOffer _offer;
    private ShopScreen _shopScreen;

    public void Setup(ShopOffer offer, ShopScreen screen)
    {
        _offer = offer;
        _shopScreen = screen;
        gameObject.SetActive(true);

        _buyButton.onClick.RemoveAllListeners();
        _buyButton.onClick.AddListener(OnBuyClicked);

        switch (offer.Type)
        {
            case ShopOffer.OfferType.Item:
                _titleText.text = offer.Item.NameItem;
                _descriptionText.text = offer.Item.Description;
                if (_icon != null && offer.Item.Sprite != null)
                    _icon.sprite = offer.Item.Sprite;
                break;

            case ShopOffer.OfferType.Heal:
                if (_icon != null) _icon.sprite = null;
                break;

            case ShopOffer.OfferType.StatUpgrade:
                if (_icon != null) _icon.sprite = null;
                break;
        }

        bool purchased = offer.IsPurchased;
        _buyButton.interactable = !purchased;
        _costText.text = purchased ? "Agotado" : $"{offer.Cost} oro";
    }

    private void OnBuyClicked()
    {
        _shopScreen.TryBuy(_offer);
    }

    private string GetStatLabel(StatUpgradeType stat)
    {
        switch (stat)
        {
            case StatUpgradeType.ExtraDamage: return "Daño";
            case StatUpgradeType.MaxHealth: return "Salud Máx.";
            case StatUpgradeType.Evasion: return "Evasión";
            case StatUpgradeType.Defense: return "Defensa";
            default: return stat.ToString();
        }
    }
}