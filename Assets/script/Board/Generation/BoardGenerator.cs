using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardGenerator : MonoBehaviour
{
    [SerializeField] private BoardManager _boardManager;
    [SerializeField] private BoardGenerationConfig _config;

    private int _numOfEmptyTiles;
    private int _numOfEnemyTiles;
    private int _numOfMiniBossTiles;
    private int _numOfChestTiles;
    private int _minBranches;
    private int _maxBranches;
    private int _minBranchLengthBeforeRejoin;
    private int _maxBranchLength;
    private float _branchProbabilityPerCandidate;

    private int _numOfTotalTilesInBoard;

    public List<BoardPath> Paths { get; private set; } = new List<BoardPath>();

    public List<List<BoardTile>> _paths => Paths.Select(p => p.Tiles).ToList();

    void Start()
    {
        SetBoardData(_boardManager.currentStageLevelData);
        _numOfTotalTilesInBoard = _numOfEmptyTiles + _numOfEnemyTiles + _numOfChestTiles;
        InstanceBoard(this.transform);
    }

    void SetBoardData(StageLevelData boardData)
    {
        _numOfEmptyTiles = boardData.GetNumOfEmptyTiles();
        _numOfEnemyTiles = boardData.GetNumOfEnemyTiles();
        _numOfMiniBossTiles = boardData.GetNumOfMiniBossTiles();
        _numOfChestTiles = boardData.GetNumOfChestTiles();

        _minBranches = boardData.GetMinBranches();
        _maxBranches = boardData.GetMaxBranches();
        _minBranchLengthBeforeRejoin = boardData.GetMinBranchLengthBeforeRejoin();
        _maxBranchLength = boardData.GetMaxBranchLength();
        _branchProbabilityPerCandidate = boardData.GetBranchProbabilityPerCandidate();
    }

    public void InstanceBoard(Transform parent)
    {
        if (parent.childCount > 1)
        {
            foreach (Transform child in parent)
            {
                if (child == _boardManager.startingTile.transform) continue;
                Destroy(child.gameObject);
            }
        }

        Paths.Clear();

        // Step 1 – Generate paths
        BoardPath mainPath = GenerateMainPath(parent);
        Paths.Add(mainPath);

        BoardBranchBuilder branchBuilder = new BoardBranchBuilder(_boardManager, _config, Paths);
        branchBuilder.GenerateBranches(parent, mainPath,
            _minBranches, _maxBranches,
            _minBranchLengthBeforeRejoin, _maxBranchLength);

        BoardTilePopulator populator = new BoardTilePopulator(_boardManager, _config, Paths);

        // Step 2 – Reserved zones (start zone & end zone)
        populator.AssignEndZones(parent);

        // Step 3 – Enemies
        populator.AssignEnemies(_numOfEnemyTiles);

        // Step 4 – One valid separator between every pair of consecutive enemies
        populator.AssignSeparatorsBetweenEnemies();

        // Step 5 – Fill remaining valid positions with special tiles
        populator.AssignSpecialTiles();

        // Step 6 – Gold tiles (independent spawn, random count from StageLevelData range)
        populator.AssignGoldTiles();

        populator.EnsureBranchEntriesAreEmpty();

        foreach (BoardPath path in Paths)
            if (!path.IsMain)
                _boardManager.PutFogOnBranch(path.Tiles);
    }

    private BoardPath GenerateMainPath(Transform parent)
    {
        BoardPath path = new BoardPath(isMain: true);
        BoardTile currentTile = _boardManager.startingTile;
        path.AddTile(currentTile);

        for (int i = 0; i < _numOfTotalTilesInBoard - 1; i++)
        {
            Vector2Int nextGridPosition = (i == _numOfTotalTilesInBoard - 2)
                ? currentTile.GetGridPosition() + new Vector2Int(0, 1)
                : GetRandomFreeNeighborPositionMain(currentTile);

            BoardTile nextTile = _boardManager.CreateNewTile(_boardManager.emptyTileData, nextGridPosition, parent);
            _boardManager.UpdateNeighbors(currentTile);
            _boardManager.UpdateNeighbors(nextTile);

            currentTile = nextTile;
            path.AddTile(currentTile);
        }

        return path;
    }

    private Vector2Int GetRandomFreeNeighborPositionMain(BoardTile current)
    {
        Vector2Int basePos = current.GetGridPosition();
        var candidates = new List<Vector2Int>
        {
            new Vector2Int(basePos.x + 1, basePos.y),
            new Vector2Int(basePos.x, basePos.y + 1)
        };

        HashSet<Vector2Int> occupied = new HashSet<Vector2Int>(
            _boardManager.generatedTiles.Select(t => t.GetGridPosition()));

        candidates.RemoveAll(p => occupied.Contains(p));
        if (candidates.Count == 0)
            candidates = new List<Vector2Int> { new Vector2Int(basePos.x + 1, basePos.y) };

        return candidates[Random.Range(0, candidates.Count)];
    }
}
