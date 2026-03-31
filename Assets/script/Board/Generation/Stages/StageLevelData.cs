using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "NewStageLevelData", menuName = "Game/Stage Level Data")]
public class StageLevelData : ScriptableObject
{
    [Header("Board")]
    [SerializeField] int _numOfEmptyTiles;
    [SerializeField] int _numOfEnemyTiles;
    [SerializeField] int _numOfMiniBossTiles;
    [SerializeField] int _numOfChestTiles;

    [Header("Branched Paths")]
    [SerializeField] int _minBranches;
    [SerializeField] int _maxBranches;
    [SerializeField] int _minBranchLengthBeforeRejoin;
    [SerializeField] int _maxBranchLength;
    [SerializeField, Range(0f, 1f)] float _branchProbabilityPerCandidate;

    [Header("Encounters")]
    [SerializeField] private List<EnemyType> _enemyList;
    [SerializeField] private List<EnemyType> _bossList;

    [SerializeField]
    private List<EnemyDifficulty> encountersOrder;

    [SerializeField]
    private List<BoardTile> interactableTiles;

    [Header("Items")]
    [SerializeField, Range(0, 100)]
    private float commonRarityPercentage;
    [SerializeField, Range(0, 100)]
    private float rareRarityPercentage;
    [SerializeField, Range(0, 100)]
    private float epicRarityPercentage;
    [SerializeField, Range(0, 100)]
    private float legendaryRarityPercentage;

    [Header("Shop")]
    [SerializeField] private List<Item> _shopItemPool;

    // Probabilidades fijas de rareza para la tienda
    private const float ShopCommonChance    = 40f;
    private const float ShopRareChance      = 30f;
    private const float ShopEpicChance      = 15f;
    private const float ShopLegendaryChance = 5f;

    public ItemRarity GetPullItemRarity() 
    {
        float toatalRarity = commonRarityPercentage + rareRarityPercentage + epicRarityPercentage + legendaryRarityPercentage;

        float pullSeed = Random.Range(0, toatalRarity);

        if (pullSeed < commonRarityPercentage)
            return ItemRarity.COMMON;
        else if (pullSeed < commonRarityPercentage + rareRarityPercentage)
            return ItemRarity.RARE;
        else if (pullSeed < commonRarityPercentage + rareRarityPercentage + epicRarityPercentage)
            return ItemRarity.EPIC;
        else 
            return ItemRarity.LEGENDARY;
    }

    /// <summary>
    /// Selecciona un item aleatorio de la pool de tienda respetando las probabilidades de rareza
    /// de la tienda (40% común, 30% raro, 15% épico, 5% legendario).
    /// Devuelve null si no hay items disponibles de la rareza sorteada.
    /// </summary>
    public Item GetShopItem()
    {
        if (_shopItemPool == null || _shopItemPool.Count == 0) return null;

        ItemRarity rarity = RollShopRarity();

        List<Item> candidates = _shopItemPool.FindAll(i => i != null && i.Rarity == rarity);

        // Si no hay candidatos de esa rareza, buscar en cualquier rareza disponible
        if (candidates.Count == 0)
            candidates = _shopItemPool.FindAll(i => i != null);

        if (candidates.Count == 0) return null;

        return candidates[Random.Range(0, candidates.Count)];
    }

    private ItemRarity RollShopRarity()
    {
        float total = ShopCommonChance + ShopRareChance + ShopEpicChance + ShopLegendaryChance;
        float roll  = Random.Range(0f, total);

        if (roll < ShopCommonChance)
            return ItemRarity.COMMON;
        else if (roll < ShopCommonChance + ShopRareChance)
            return ItemRarity.RARE;
        else if (roll < ShopCommonChance + ShopRareChance + ShopEpicChance)
            return ItemRarity.EPIC;
        else
            return ItemRarity.LEGENDARY;
    }

    public List<Item> GetShopItemPool() { return _shopItemPool; }

    public int GetNumOfEmptyTiles() { return _numOfEmptyTiles; }
    public int GetNumOfEnemyTiles() { return _numOfEnemyTiles; }
    public int GetNumOfMiniBossTiles() { return _numOfMiniBossTiles; }
    public int GetNumOfChestTiles() { return _numOfChestTiles; }

    public int GetMinBranches() { return _minBranches; }
    public int GetMaxBranches() { return _maxBranches; }
    public int GetMinBranchLengthBeforeRejoin() { return _minBranchLengthBeforeRejoin; }
    public int GetMaxBranchLength() { return _maxBranchLength; }
    public float GetBranchProbabilityPerCandidate() { return _branchProbabilityPerCandidate; }

    public List<EnemyDifficulty> GetEncountersOrder() { return encountersOrder; }

    public int GetNumEncounters() { return encountersOrder.Count; }

    public List<BoardTile> GetinteractableTiles() { return interactableTiles; }
    public List<EnemyType> GetEnemyEncounters() { return _enemyList; }
    public List<EnemyType> GetBossEncounters() { return _bossList; }
}
