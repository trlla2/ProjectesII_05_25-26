using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class SpecialTileEntry
{
    public BoardTileData tileData;

    [Min(0f)] public float weight;
    [Min(0f)] public int maxInstances; // 0 = unlimited
}

[CreateAssetMenu(fileName = "NewStageLevelData", menuName = "Game/Stage Level Data")]
public class StageLevelData : ScriptableObject
{
    [Header("Board")]
    [SerializeField] int _numOfEmptyTiles;
    [SerializeField] int _numOfEnemyTiles;
    [SerializeField] int _numOfMiniBossTiles;
    [SerializeField] int _numOfChestTiles;

    [Header("Special Tiles Pool")]
    [SerializeField] private List<SpecialTileEntry> _specialTilePool = new List<SpecialTileEntry>();

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

    private const float ShopCommonChance = 40f;
    private const float ShopRareChance = 30f;
    private const float ShopEpicChance = 15f;
    private const float ShopLegendaryChance = 5f;

    public BoardTileData RollSpecialTile(Dictionary<BoardTileData, int> placedCounts = null)
    {
        if (_specialTilePool == null || _specialTilePool.Count == 0) return null;

        List<SpecialTileEntry> available = _specialTilePool
            .Where(e =>
                e.tileData != null &&
                e.weight > 0f &&
                (e.maxInstances == 0 ||
                 placedCounts == null ||
                 !placedCounts.ContainsKey(e.tileData) ||
                 placedCounts[e.tileData] < e.maxInstances))
            .ToList();

        if (available.Count == 0) return null;

        float totalWeight = available.Sum(e => e.weight);
        if (totalWeight <= 0f) return null;

        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (SpecialTileEntry entry in available)
        {
            cumulative += entry.weight;
            if (roll < cumulative)
                return entry.tileData;
        }

        return available[available.Count - 1].tileData;
    }

    public bool HasSpecialTilePool() =>
        _specialTilePool != null && _specialTilePool.Any(e => e.tileData != null && e.weight > 0f);


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

    public Item GetShopItem()
    {
        if (_shopItemPool == null || _shopItemPool.Count == 0) return null;

        ItemRarity rarity = RollShopRarity();
        List<Item> candidates = _shopItemPool.FindAll(i => i != null && i.Rarity == rarity);

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
