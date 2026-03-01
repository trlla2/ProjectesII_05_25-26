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

    public ItemRarity GetPullItemRarity() 
    {
        float toatalRarity = commonRarityPercentage + rareRarityPercentage + epicRarityPercentage + legendaryRarityPercentage;

        float pullSeed = Random.Range(0, toatalRarity);

        if (pullSeed < commonRarityPercentage)
            return ItemRarity.COMMON;
        else if (pullSeed < commonRarityPercentage + rareRarityPercentage)
            return ItemRarity.RARE;
        else if (pullSeed < commonRarityPercentage + rareRarityPercentage)
            return ItemRarity.RARE;
        else 
            return ItemRarity.LEGENDARY;
    }



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
