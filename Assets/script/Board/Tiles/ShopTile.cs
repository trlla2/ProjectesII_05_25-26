using System.Collections.Generic;
using UnityEngine;

public class ShopTile : BoardTile
{
    private const int ItemSlots = 3;

    [SerializeField] private BoardTileData _postInteractionTile;

    [Header("Heal Offer Settings")]
    [SerializeField, Range(0f, 1f)] private float _healPercent = 0.30f;
    [SerializeField] private int _healCost = 15;

    [Header("Stat Upgrade Settings")]
    [SerializeField] private int _statUpgradeCost = 20;
    [SerializeField] private float _damageUpgradeAmount = 2f;
    [SerializeField] private float _healthUpgradeAmount = 10f;
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

        for (int i = 0; i < ItemSlots; i++)
        {
            Item item = _boardManager.currentStageLevelData.GetShopItem();
            if (item != null)
                _itemOffers.Add(ShopOffer.CreateItemOffer(item, item.Rarity.GetShopPrice()));
        }

        _healOffer = ShopOffer.CreateHealOffer(_healPercent, _healCost);
        _statOffer = CreateRandomStatUpgradeOffer();

        Debug.Log($"ShopTile: {_itemOffers.Count} items, 1 heal, 1 stat generados.");
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

        return ShopOffer.CreateStatUpgradeOffer(chosen, amount, _statUpgradeCost);
    }

    public void Replace()
    {
        if (_postInteractionTile != null)
            _boardManager.ReplaceTileType(this, _postInteractionTile);
    }
}
