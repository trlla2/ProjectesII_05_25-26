using System.Collections.Generic;
using UnityEngine;

public class ShopTile : BoardTile
{
    private const int ShopSlots = 3;

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

    [Header("Item Offer Settings")]
    [SerializeField] private int _itemCost = 25;

    private List<ShopOffer> _offers;
    public IReadOnlyList<ShopOffer> Offers => _offers;

    private List<Item> _shopItemPool;

    private void Start()
    {
        // Cargar la pool de items desde el StageLevelData al instanciarse la tile
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

    /// <summary>
    /// Genera las 3 ofertas de la tienda de forma aleatoria al interactuar con la casilla.
    /// Siempre hay al menos una oferta de curación y una de mejora de estadística.
    /// El tercer slot siempre es un item.
    /// </summary>
    public void GenerateOffers()
    {
        _offers = new List<ShopOffer>(ShopSlots);

        // Slot 1: curación
        _offers.Add(ShopOffer.CreateHealOffer(_healPercent, _healCost));

        // Slot 2: mejora de estadística aleatoria
        _offers.Add(CreateRandomStatUpgradeOffer());

        // Slot 3: item aleatorio de la pool
        Item item = _boardManager.currentStageLevelData.GetShopItem();
        if (item != null)
            _offers.Add(ShopOffer.CreateItemOffer(item, _itemCost));
        else
            _offers.Add(CreateRandomStatUpgradeOffer()); // fallback si no hay items

        Debug.Log($"ShopTile: {_offers.Count} ofertas generadas.");
    }

    private ShopOffer CreateRandomStatUpgradeOffer()
    {
        StatUpgradeType[] stats = (StatUpgradeType[])System.Enum.GetValues(typeof(StatUpgradeType));
        StatUpgradeType chosen = stats[Random.Range(0, stats.Length)];

        float amount = chosen switch
        {
            StatUpgradeType.ExtraDamage => _damageUpgradeAmount,
            StatUpgradeType.MaxHealth   => _healthUpgradeAmount,
            StatUpgradeType.Evasion     => _evasionUpgradeAmount,
            StatUpgradeType.Defense     => _defenseUpgradeAmount,
            _                           => 1f
        };

        return ShopOffer.CreateStatUpgradeOffer(chosen, amount, _statUpgradeCost);
    }

    public void Replace()
    {
        if (_postInteractionTile != null)
            _boardManager.ReplaceTileType(this, _postInteractionTile);
    }
}
