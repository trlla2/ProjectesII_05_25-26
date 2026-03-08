using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardGenerator : MonoBehaviour
{
    [SerializeField] private BoardManager _boardManager;

    [Header("Main")]
    private int _numOfEmptyTiles;
    private int _numOfEnemyTiles;
    private int _numOfMiniBossTiles;
    private int _numOfChestTiles;

    [Header("Reserved Zones")]
    [SerializeField] int _endZoneReservedTiles;
    [SerializeField] int _bossTileDistanceFromEnd;
    private int _chestOfEgressTileDistFromEnd;
    [SerializeField] int _startZoneReserveTiles;

    [Header("Branched Paths")]
    [SerializeField, Range(0, 2)] int _minDistBetweenPaths;
    private int _minBranches;
    private int _maxBranches;
    private int _minBranchLengthBeforeRejoin;
    private int _maxBranchLength;
    private float _branchProbabilityPerCandidate;

    [Header("Branch Neighbor Tiles Spawn")]
    [SerializeField] bool AllowBranchUp;    // (0, +1)
    [SerializeField] bool AllowBranchDown;  // (0, -1)
    [SerializeField] bool AllowBranchLeft;  // (-1, 0)
    [SerializeField] bool AllowBranchRight; // (+1, 0)

    int _numOfTotalTilesInBoard;

    [SerializeField] public List<List<BoardTile>> _paths = new List<List<BoardTile>>();

    private void Awake()
    {
        _chestOfEgressTileDistFromEnd = _bossTileDistanceFromEnd - 2;
    }

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

        List<BoardTile> mainPath = GenerateMainPath(parent);
        _paths.Add(mainPath);

        GenerateBranches(parent, mainPath);

        AssignEndZonesPerPath(parent);

        AssignEnemiesAndChests();

        EnsureBranchEntriesAreEmpty();

        foreach (List<BoardTile> path in _paths)
            if (path != mainPath)
                _boardManager.PutFogOnBranch(path);
    }

    void EnsureBranchEntriesAreEmpty()
    {
        for (int b = 1; b < _paths.Count; b++)
        {
            List<BoardTile> branch = _paths[b];
            if (branch == null || branch.Count < 2) continue;

            BoardTile entry = branch[1];
            if (entry is EmptyTile) continue;

            int swapIdx = -1;
            for (int i = 2; i < branch.Count; i++)
            {
                if (branch[i] is EmptyTile)
                {
                    swapIdx = i;
                    break;
                }
            }

            if (swapIdx == -1) continue;

            SwapTilePositionsInBranch(branch, 1, swapIdx);
        }
    }

    void SwapTilePositionsInBranch(List<BoardTile> branch, int indexA, int indexB)
    {
        BoardTile tileA = branch[indexA]; 
        BoardTile tileB = branch[indexB];

        Vector2Int posA = tileA.GetGridPosition();
        Vector2Int posB = tileB.GetGridPosition();

        // Intercambiar posiciones de grid
        tileA.SetGridPosition(posB);
        tileB.SetGridPosition(posA);

        tileA.transform.position = new Vector3(posB.x, tileA.transform.position.y, posB.y);
        tileB.transform.position = new Vector3(posA.x, tileB.transform.position.y, posA.y);

        branch[indexA] = tileB;
        branch[indexB] = tileA;

        // Actualizar referencias en todos los paths
        foreach (List<BoardTile> path in _paths)
        {
            for (int i = 0; i < path.Count; i++)
            {
                if (path[i] == tileA) path[i] = tileA;
                if (path[i] == tileB) path[i] = tileB;
            }
        }

        tileA.neighbors.Clear();
        tileB.neighbors.Clear();
        _boardManager.UpdateNeighbors(tileA);
        _boardManager.UpdateNeighbors(tileB);

        foreach (BoardTile neighbor in tileA.neighbors)
        {
            if (neighbor != null) _boardManager.UpdateNeighbors(neighbor);
        }
        foreach (BoardTile neighbor in tileB.neighbors)
        {
            if (neighbor != null) _boardManager.UpdateNeighbors(neighbor);
        }

        Debug.Log($"EnsureBranchEntriesAreEmpty: intercambiadas posiciones {posA} y {posB} en branch.");
    }

    List<BoardTile> GenerateMainPath(Transform parent)
    {
        List<BoardTile> path = new List<BoardTile>();
        BoardTile currentTile = _boardManager.startingTile;
        path.Add(currentTile);

        for (int i = 0; i < _numOfTotalTilesInBoard - 1; i++)
        {
            Vector2Int nextGridPosition;
            if (i == _numOfTotalTilesInBoard - 2) // stairs tile
            {
                nextGridPosition = currentTile.GetGridPosition() + new Vector2Int(0, 1);
            }
            else
            {
                nextGridPosition = GetRandomFreeNeighborPositionMain(currentTile);
            }

            BoardTile nextTile = _boardManager.CreateNewTile(_boardManager.emptyTileData, nextGridPosition, parent);

            _boardManager.UpdateNeighbors(currentTile);
            _boardManager.UpdateNeighbors(nextTile);

            currentTile = nextTile;
            path.Add(currentTile);
        }

        return path;
    }

    Vector2Int GetRandomFreeNeighborPositionMain(BoardTile current)
    {
        Vector2Int basePos = current.GetGridPosition();
        var candidates = new List<Vector2Int>
        {
            new Vector2Int(basePos.x + 1, basePos.y),
            new Vector2Int(basePos.x, basePos.y + 1)
        };

        HashSet<Vector2Int> occupied = new HashSet<Vector2Int>(_boardManager.generatedTiles.Select(t => t.GetGridPosition()));
        candidates.RemoveAll(p => occupied.Contains(p));
        if (candidates.Count == 0)
            candidates = new List<Vector2Int> { new Vector2Int(basePos.x + 1, basePos.y) };

        return candidates[Random.Range(0, candidates.Count)];
    }

    void GenerateBranches(Transform parent, List<BoardTile> mainPath)
    {
        if (_maxBranches <= 0 || _maxBranchLength <= 0) return;

        _minBranches = Mathf.Clamp(_minBranches, 0, _maxBranches);

        int candidatesCount = Mathf.Max(0, mainPath.Count - _endZoneReservedTiles);

        HashSet<BoardTile> startZoneSet = GetStartZoneTiles(mainPath).ToHashSet();

        List<BoardTile> candidates = mainPath
            .Take(candidatesCount)
            .Where(t => !startZoneSet.Contains(t) && t is EmptyTile)
            .ToList();

        Shuffle(candidates);

        int branchesCreated = 0;

        foreach (BoardTile origin in candidates)
        {
            if (branchesCreated >= _minBranches) break;

            List<BoardTile> branch = BuildBranch(origin, parent, _maxBranchLength);
            if (branch.Count > _minBranchLengthBeforeRejoin)
            {
                if (!_paths.Contains(branch))
                    _paths.Add(branch);
                branchesCreated++;
            }
            else
            {
                foreach (BoardTile tile in branch)
                {
                    if (tile == origin) continue;
                    _boardManager.generatedTiles.Remove(tile);
                    Destroy(tile.gameObject);
                }
            }
        }

    }

    List<Vector2Int> GetAllowedBranchNeighbors(Vector2Int basePos)
    {
        var dirs = new List<Vector2Int>();
        if (AllowBranchRight) dirs.Add(new Vector2Int(basePos.x + 1, basePos.y));
        if (AllowBranchLeft) dirs.Add(new Vector2Int(basePos.x - 1, basePos.y));
        if (AllowBranchUp) dirs.Add(new Vector2Int(basePos.x, basePos.y + 1));
        if (AllowBranchDown) dirs.Add(new Vector2Int(basePos.x, basePos.y - 1));
        return dirs;
    }

    List<BoardTile> BuildBranch(BoardTile origin, Transform parent, int maxLength)
    {
        List<BoardTile> branch = new List<BoardTile> { origin };
        BoardTile current = origin;

        List<BoardTile> instanced = new List<BoardTile>();

        HashSet<Vector2Int> occupiedAll = new HashSet<Vector2Int>(_boardManager.generatedTiles.Select(t => t.GetGridPosition()));

        for (int i = 0; i < maxLength; i++)
        {
            Vector2Int basePos = current.GetGridPosition();

            List<Vector2Int> neighbors = GetAllowedBranchNeighbors(basePos);
            neighbors.RemoveAll(p => branch.Skip(1).Any(b => b.GetGridPosition() == p));

            List<Vector2Int> freeNeighbors = neighbors.Where(pos =>
                !occupiedAll.Contains(pos) &&
                HasMinSeparationFromOtherPaths(pos, branch)
            ).ToList();

            if (freeNeighbors.Count > 0)
            {
                Vector2Int nextPos = freeNeighbors[Random.Range(0, freeNeighbors.Count)];
                BoardTile nextTile = _boardManager.CreateNewTile(_boardManager.emptyTileData, nextPos, parent);
                _boardManager.UpdateNeighbors(current);
                _boardManager.UpdateNeighbors(nextTile);

                current = nextTile;
                branch.Add(current);
                instanced.Add(current);
                occupiedAll.Add(nextPos);
                continue;
            }
            else
            {
                // La branch no puede continuar: delete si es demasiado corta
                if (branch.Count - 1 >= _minBranchLengthBeforeRejoin)
                {
                    branch[0].generatedBranch = branch;
                    _paths.Add(branch);
                }
                else
                {
                    foreach (BoardTile tile in instanced)
                    {
                        _boardManager.generatedTiles.Remove(tile);
                        Destroy(tile.gameObject);
                    }
                }

                break;
            }
        }

        return branch;
    }

    bool HasMinSeparationFromOtherPaths(Vector2Int candidatePos, List<BoardTile> currentBranch, bool allowEqualPosition = false)
    {
        HashSet<Vector2Int> currentBranchPositions = new HashSet<Vector2Int>(currentBranch.Select(t => t.GetGridPosition()));

        BoardTile firstTileOfPath = currentBranch.First();
        if (currentBranch.Count <= 1)
            return true;

        foreach (BoardTile tile in _boardManager.generatedTiles)
        {
            Vector2Int otherPos = tile.GetGridPosition();

            if (currentBranchPositions.Contains(otherPos))
            {
                if (allowEqualPosition && otherPos == candidatePos) continue;
                else continue;
            }

            int distBetweenPaths = Mathf.Abs(candidatePos.x - otherPos.x) + Mathf.Abs(candidatePos.y - otherPos.y);
            if (distBetweenPaths <= _minDistBetweenPaths)
                return false;
        }
        return true;
    }

    bool PathIsTerminal(List<BoardTile> path)
    {
        if (path == null || path.Count == 0) return false;

        BoardTile endTile = path[path.Count - 1];
        _boardManager.UpdateNeighbors(endTile);
        List<BoardTile> neighbors = endTile.neighbors.Where(n => n != null).ToList();

        if (neighbors.Count == 0) return true;

        Vector2Int prevPos = path.Count >= 2 ? path[path.Count - 2].GetGridPosition() : new Vector2Int(int.MinValue, int.MinValue);

        foreach (var n in neighbors)
        {
            if (n.GetGridPosition() != prevPos)
                return false;
        }
        return true;
    }

    void AssignEndZonesPerPath(Transform parent)
    {
        HashSet<BoardTile> startZoneSet = (_paths.Count > 0) ? GetStartZoneTiles(_paths[0]).ToHashSet() :
            new HashSet<BoardTile>();

        foreach (List<BoardTile> path in _paths)
        {
            if (path == null || path.Count == 0) continue;
            CreateEndZoneForPath(path, parent, startZoneSet);
        }
    }

    void CreateEndZoneForPath(List<BoardTile> path, Transform parent, HashSet<BoardTile> startZoneSet)
    {
        if (path.Count < _endZoneReservedTiles) return;

        BoardTile endTile = path[path.Count - 1];
        if (startZoneSet.Contains(endTile)) return;

        if (!PathIsTerminal(path)) return;

        // escalera en neighbor(0,1) de endTile
        Vector2Int stairsPos = endTile.GetGridPosition() + new Vector2Int(0, 1);

        BoardTile stairsTarget = _boardManager.generatedTiles.FirstOrDefault(t => t.GetGridPosition() == stairsPos);
        if (stairsTarget == null)
            stairsTarget = _boardManager.CreateNewTile(_boardManager.emptyTileData, stairsPos, parent);

        _boardManager.ReplaceTileType(stairsTarget, _boardManager.stairsTileData);

        _boardManager.UpdateNeighbors(endTile);
        _boardManager.UpdateNeighbors(stairsTarget);

        List<BoardTile> validNeighbors = stairsTarget.neighbors.Where(n => n != null).ToList();
        int externalNeighbors = validNeighbors.Count(n => !path.Contains(n));
        if (externalNeighbors > 0)
        {
            _boardManager.ReplaceTileType(stairsTarget, _boardManager.emptyTileData);
            _boardManager.UpdateNeighbors(stairsTarget);
            return;
        }

        // instance bossTile
        int bossIndex = path.Count - _bossTileDistanceFromEnd;
        if (bossIndex >= 0 && bossIndex < path.Count)
        {
            BoardTile bossTile = path[bossIndex];

            if (!startZoneSet.Contains(bossTile) &&
                bossTile.GetData() != _boardManager.stairsTileData &&
                bossTile.GetData() != _boardManager.bossTileData)
            {
                ReplaceTileAndUpdatePaths(bossTile, _boardManager.bossTileData);
                _boardManager.UpdateNeighbors(bossTile);
            }
        }

        // INSTANCE CHEST OF EGRESS TILE
        int egressIndex = path.Count - _chestOfEgressTileDistFromEnd;
        if (egressIndex >= 0 && egressIndex < path.Count)
        {
            BoardTile egressTile = path[egressIndex];

            if (!startZoneSet.Contains(egressTile) &&
                egressTile.GetData() != _boardManager.stairsTileData &&
                egressTile.GetData() != _boardManager.bossTileData &&
                egressTile.GetData() != _boardManager.chestOfEgressTileData)
            {
                ReplaceTileAndUpdatePaths(egressTile, _boardManager.chestOfEgressTileData);
                _boardManager.UpdateNeighbors(egressTile);
            }
        }
    }

    void AssignEnemiesAndChests()
    {
        if (_paths == null || _paths.Count == 0) return;

        List<BoardTile> mainPath = _paths[0];
        HashSet<BoardTile> startZoneSet = GetStartZoneTiles(mainPath).ToHashSet();

        List<BoardTile> mainCandidates = GetValidEnemyPositionsForPath(mainPath, startZoneSet, null);
        Debug.Log($"Candidatos válidos en main: {mainCandidates.Count}");
        PlaceEnemiesOnPath(mainCandidates, _numOfEnemyTiles);
        Debug.Log($"Total enemigos colocados en main: {mainPath.Count(t => IsEnemyTile(t))}");

        for (int b = 1; b < _paths.Count; b++)
        {
            List<BoardTile> branch = _paths[b];
            if (branch == null || branch.Count <= 1) continue;

            BoardTile originOnMain = branch[0];
            int originIdx = mainPath.IndexOf(originOnMain);
            if (originIdx < 0) originIdx = 0;

            int enemiesInSharedSegment = 0;
            for (int i = 0; i <= originIdx && i < mainPath.Count; i++)
            {
                if (IsEnemyTile(mainPath[i]))
                    enemiesInSharedSegment++;
            }

            int enemiesNeeded = _numOfEnemyTiles - enemiesInSharedSegment;
            Debug.Log($"Branch {b}: tramo compartido tiene {enemiesInSharedSegment} enemigos. Necesita {enemiesNeeded} más.");

            if (enemiesNeeded <= 0) continue;

            List<BoardTile> branchCandidates = GetValidEnemyPositionsForPath(branch, startZoneSet, null)
                .Where(t => t != originOnMain)
                .ToList();

            PlaceEnemiesOnPath(branchCandidates, enemiesNeeded);
        }

        PlaceChests(startZoneSet);
    }

    bool IsEnemyTile(BoardTile tile)
    {
        if (tile == null) return false;
        var data = tile.GetData();
        if (data == null) return false;

        return data == _boardManager.enemyTileData;
    }

    void PlaceEnemiesOnPath(List<BoardTile> candidates, int count)
    {
        if (candidates.Count == 0 || count <= 0) return;

        float avgStep = (float)candidates.Count / count;
        int minStep = Mathf.Max(1, Mathf.FloorToInt(avgStep * 0.5f)); 
        int maxStep = Mathf.Max(2, Mathf.CeilToInt(avgStep * 1.5f));  

        int placed = 0;
        int currentIndex = Random.Range(0, Mathf.Min(3, candidates.Count));

        while (currentIndex < candidates.Count && placed < count)
        {
            BoardTile tile = candidates[currentIndex];

            if (!IsEnemyTile(tile) && !HasSpecialNeighbor(tile))
            {
                BoardTile newTile = ReplaceTileAndUpdatePaths(tile, _boardManager.enemyTileData);
                candidates[currentIndex] = newTile;
                placed++;
            }

            int step = Random.Range(minStep, maxStep + 1);
            currentIndex += step;
        }

        if (placed < count)
        {
            List<int> availableIndices = new List<int>();
            for (int i = 0; i < candidates.Count; i++)
            {
                BoardTile tile = candidates[i];
                if (!IsEnemyTile(tile) && !HasSpecialNeighbor(tile))
                    availableIndices.Add(i);
            }

            Shuffle(availableIndices);

            foreach (int idx in availableIndices)
            {
                if (placed >= count) break;

                BoardTile tile = candidates[idx];
                if (IsEnemyTile(tile)) continue;

                BoardTile newTile = ReplaceTileAndUpdatePaths(tile, _boardManager.enemyTileData);
                candidates[idx] = newTile;
                placed++;
            }
        }
    }

    BoardTile ReplaceTileAndUpdatePaths(BoardTile oldTile, BoardTileData newData)
    {
        Vector2Int gridPos = oldTile.GetGridPosition();

        _boardManager.ReplaceTileType(oldTile, newData);

        BoardTile newTile = _boardManager.generatedTiles.FirstOrDefault(t => t.GetGridPosition() == gridPos);

        foreach (var path in _paths)
        {
            for (int i = 0; i < path.Count; i++)
            {
                if (path[i] == oldTile || path[i].GetGridPosition() == gridPos)
                {
                    path[i] = newTile;
                }
            }
        }

        return newTile;
    }

    List<BoardTile> GetValidEnemyPositionsForPath(List<BoardTile> path, HashSet<BoardTile> startZoneSet, HashSet<BoardTile> reservedEndSet)
    {
        int endReserveCount = _endZoneReservedTiles;
        HashSet<BoardTile> pathEndZone = path
            .Skip(Mathf.Max(0, path.Count - endReserveCount))
            .ToHashSet();

        if (reservedEndSet != null)
            pathEndZone.UnionWith(reservedEndSet);

        return path.Where(t =>
            t != null &&
            !startZoneSet.Contains(t) &&
            !pathEndZone.Contains(t) &&
            !IsEnemyTile(t) &&
            t.GetData() != _boardManager.stairsTileData &&
            t.GetData() != _boardManager.bossTileData &&
            t.IsWalkable()
        ).ToList();
    }

    void PlaceChests(HashSet<BoardTile> startZoneSet)
    {
        HashSet<BoardTile> allEndZones = new HashSet<BoardTile>();

        foreach (var path in _paths)
        {
            if (path == null || path.Count == 0) continue;
            var endZone = path.Skip(Mathf.Max(0, path.Count - _endZoneReservedTiles));
            allEndZones.UnionWith(endZone);
        }

        List<BoardTile> available = _boardManager.generatedTiles
            .Where(t =>
                !startZoneSet.Contains(t) &&
                !allEndZones.Contains(t) &&
                t != _boardManager.startingTile &&
                t.GetData() == _boardManager.emptyTileData &&
                t.IsWalkable())
            .ToList();

        Shuffle(available);

        int placed = 0;
        for (int i = 0; i < available.Count && placed < _numOfChestTiles; i++)
        {
            BoardTile tile = available[i];
            if (HasSpecialNeighbor(tile)) continue;

            ReplaceTileAndUpdatePaths(tile, _boardManager.chestTileData);
            placed++;
        }

        Debug.Log($"Cofres colocados: {placed}");
    }

    IEnumerable<BoardTile> GetStartZoneTiles(List<BoardTile> mainPath)
    {
        int count = Mathf.Clamp(_startZoneReserveTiles, 1, mainPath.Count);
        return mainPath.Take(count);
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }

    bool HasSpecialNeighbor(BoardTile tile)
    {
        _boardManager.UpdateNeighbors(tile);

        foreach (BoardTile neighbor in tile.neighbors)
        {
            if (neighbor == null) continue;

            var data = neighbor.GetData();
            if (data != _boardManager.emptyTileData)
            {
                return true;
            }
        }
        return false;
    }
}
