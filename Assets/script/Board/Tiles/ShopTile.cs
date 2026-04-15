using System.Collections.Generic;
using UnityEngine;

public class ShopTile : BoardTile
{
    private const int ItemSlots = 3;

    [Header("Heal Offer Settings")]
    [SerializeField, Range(0f, 1f)] private float _healPercent = 0.30f;
    [SerializeField] private int _healCost = 15;

    [Header("Stat Upgrade Settings")]
    [SerializeField] private int _statUpgradeCost = 20;
    [SerializeField, Range(0f, 1f)]
    private float _statUpgradePercent = 0.1f;
    [SerializeField] private float _damageUpgradeAmount  = 2f;
    [SerializeField] private float _healthUpgradeAmount  = 10f;
    [SerializeField] private float _evasionUpgradeAmount = 5f;
    [SerializeField] private float _defenseUpgradeAmount = 3f;

    private List<ShopOffer> _itemOffers = new List<ShopOffer>();
    private ShopOffer _healOffer;
    private ShopOffer _statOffer;

    public IReadOnlyList<ShopOffer> ItemOffers => _itemOffers;
    public ShopOffer HealOffer => _healOffer;
    public ShopOffer StatOffer => _statOffer;

    private List<Item> _shopItemPool;

    private void Start()
    {
        if (_boardManager != null && _boardManager.currentStageLevelData != null)
            _shopItemPool = _boardManager.currentStageLevelData.GetShopItemPool();

        if (_shopItemPool == null)
            _shopItemPool = new List<Item>();
    }

    public override void Interact()
    {
        GenerateOffers();
        _boardManager.OpenShop(this);
    }

    public void GenerateOffers()
    {
        _itemOffers = new List<ShopOffer>(ItemSlots);

        HashSet<Item> usedItems = new HashSet<Item>();

        for (int i = 0; i < ItemSlots; i++)
        {
            Item item = GetUniqueShopItem(usedItems);
            if (item != null)
            {
                usedItems.Add(item);
                _itemOffers.Add(ShopOffer.CreateItemOffer(item, item.Rarity.GetShopPrice()));
            }
        }

        _healOffer = ShopOffer.CreateHealOffer(_healPercent, _healCost);
        _statOffer = CreateRandomStatUpgradeOffer();

        Debug.Log($"ShopTile: {_itemOffers.Count} items, 1 heal, 1 stat generados.");
    }

    private Item GetUniqueShopItem(HashSet<Item> usedItems)
    {
        StageLevelData stageData = _boardManager.currentStageLevelData;
        if (stageData == null) return null;

        const int maxAttempts = 10;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Item candidate = stageData.GetShopItem();
            if (candidate != null && !usedItems.Contains(candidate))
                return candidate;
        }

        List<Item> pool = stageData.GetShopItemPool();
        if (pool != null)
        {
            foreach (Item item in pool)
            {
                if (item != null && !usedItems.Contains(item))
                    return item;
            }
        }

        return null;
    }

    private ShopOffer CreateRandomStatUpgradeOffer()
    {
        StatUpgradeType[] stats = (StatUpgradeType[])System.Enum.GetValues(typeof(StatUpgradeType));
        StatUpgradeType chosen = stats[Random.Range(0, stats.Length)];

        float amount = chosen switch
        {
            StatUpgradeType.ExtraDamage => _damageUpgradeAmount,
            StatUpgradeType.MaxHealth => _healthUpgradeAmount,
            StatUpgradeType.Evasion => _evasionUpgradeAmount,
            StatUpgradeType.Defense => _defenseUpgradeAmount,
            _ => 1f
        };

        return ShopOffer.CreateStatUpgradeOffer(chosen, amount, _statUpgradePercent, _statUpgradeCost);
    }

    public void Replace()
    {
        ReplaceWithPostInteraction();
    }
}
