using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;

public class ShopSlotUI : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _descriptionText;
    [SerializeField] private TMP_Text _costText;
    [SerializeField] private Button _buyButton;

    [Header("ItemStuff")]
    [SerializeField]
    private Image itemBaner = null;
    [SerializeField]
    private Image itemBG = null;

    [SerializeField]
    private GameObject itemLegendaryEffect = null;

    [Header("ItemRarity Header")]
    [SerializeField]
    private Sprite commonHeader;
    [SerializeField]
    private Sprite rareHeader;
    [SerializeField]
    private Sprite epicHeader;
    [SerializeField]
    private Sprite legendaryHeader;

    [Header("ItemRarity BG")]
    [SerializeField]
    private Sprite commonBG;
    [SerializeField]
    private Sprite rareBG;
    [SerializeField]
    private Sprite epicBG;
    [SerializeField]
    private Sprite legendaryBG;

    [Header("HealthBarStuff")]

    [SerializeField]
    private PlayerHealthBar healthBar;

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
                _titleText.text = LocalizationSettings.StringDatabase.GetLocalizedString("ItemNames", offer.Item.NameItem);
                _descriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("ItemDescriptions", offer.Item.NameItem);
                if (_icon != null && offer.Item.Sprite != null)
                    _icon.sprite = offer.Item.Sprite;

                if(itemBaner != null && itemBG != null)
                {
                    switch (offer.Item.Rarity)
                    {
                        case ItemRarity.COMMON:
                            itemBaner.sprite = commonHeader;
                            itemBG.sprite = commonBG;
                            itemLegendaryEffect.SetActive(false);
                            break;
                        case ItemRarity.RARE:
                            itemBaner.sprite = rareHeader;
                            itemBG.sprite = rareBG;
                            itemLegendaryEffect.SetActive(false);
                            break;
                        case ItemRarity.EPIC:
                            itemBaner.sprite = epicHeader;
                            itemBG.sprite = epicBG;
                            itemLegendaryEffect.SetActive(false);
                            break;
                        case ItemRarity.LEGENDARY:
                            itemBaner.sprite = legendaryHeader;
                            itemBG.sprite = legendaryBG;
                            itemLegendaryEffect.SetActive(true);
                            break;
                    }
                }

                break;

            case ShopOffer.OfferType.Heal:
                //if (_icon != null) _icon.sprite = null;
                break;

            case ShopOffer.OfferType.StatUpgrade:
                if (offer.StatType == StatUpgradeType.Defense)
                    _icon.sprite = _shopScreen._statUpgradeData.Where(s => s.Type == StatUpgradeType.Defense).FirstOrDefault().Icon;
                //else if (offer.StatType == StatUpgradeType.Evasion)
                //    _icon.sprite = _shopScreen._statUpgradeData.Where(s => s.Type == StatUpgradeType.Evasion).FirstOrDefault().Icon;
                else if (offer.StatType == StatUpgradeType.ExtraDamage)
                    _icon.sprite = _shopScreen._statUpgradeData.Where(s => s.Type == StatUpgradeType.ExtraDamage).FirstOrDefault().Icon;
                else if (offer.StatType == StatUpgradeType.MaxHealth)
                    _icon.sprite = _shopScreen._statUpgradeData.Where(s => s.Type == StatUpgradeType.MaxHealth).FirstOrDefault().Icon;
                break;
        }

        bool purchased = offer.IsPurchased;
        _buyButton.interactable = !purchased;
        _costText.text = purchased ? "SoldOut" : $"{offer.Cost}";

        if (purchased)
        {
            _icon.color = new Color(_icon.color.r, _icon.color.g, _icon.color.b, 0.5f);


            if (itemBaner != null && itemBG != null)
            {
                itemBaner.color = new Color(itemBaner.color.r, itemBaner.color.g, itemBaner.color.b, 0.5f);
                itemBG.color = new Color(itemBG.color.r, itemBG.color.g, itemBG.color.b, 0.5f);
                itemLegendaryEffect.SetActive(false);
            }
        }
        else
        {
            _icon.color = new Color(_icon.color.r, _icon.color.g, _icon.color.b, 1f);
            if (itemBaner != null && itemBG != null)
            {
                itemBaner.color = new Color(itemBaner.color.r, itemBaner.color.g, itemBaner.color.b, 1f);
                itemBG.color = new Color(itemBG.color.r, itemBG.color.g, itemBG.color.b, 1f);
            }
        }

    }

    private void OnBuyClicked()
    {
        _shopScreen.TryBuy(_offer);
        healthBar?.UpdateHealthBar();
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