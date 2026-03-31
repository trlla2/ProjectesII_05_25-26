using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardTilePopulator
{
    private readonly BoardManager _boardManager;
    private readonly BoardGenerationConfig _config;
    private readonly List<BoardPath> _allPaths;

    // Caché de posiciones ocupadas por tiles especiales (no-empty), actualizado
    // cada vez que se coloca una tile. Es la fuente de verdad para evitar solapamientos.
    private readonly HashSet<Vector2Int> _occupiedBySpecial = new HashSet<Vector2Int>();

    public BoardTilePopulator(BoardManager boardManager, BoardGenerationConfig config, List<BoardPath> allPaths)
    {
        _boardManager = boardManager;
        _config = config;
        _allPaths = allPaths;
    }

    // — End Zones ——————————————————————————————————————————————————————————————

    public void AssignEndZones(Transform parent)
    {
        HashSet<BoardTile> startZoneSet = _allPaths.Count > 0
            ? _allPaths[0].GetStartZone(_config.StartZoneReserveTiles).ToHashSet()
            : new HashSet<BoardTile>();

        foreach (BoardPath path in _allPaths)
        {
            if (path == null || path.Tiles.Count == 0) continue;
            CreateEndZoneForPath(path, parent, startZoneSet);
        }
    }

    private void CreateEndZoneForPath(BoardPath path, Transform parent, HashSet<BoardTile> startZoneSet)
    {
        if (path.Tiles.Count < _config.EndZoneReservedTiles) return;
        if (startZoneSet.Contains(path.EndTile)) return;
        if (!path.IsTerminal(_boardManager)) return;

        Vector2Int stairsPos = path.EndTile.GetGridPosition() + new Vector2Int(0, 1);
        BoardTile stairsTarget = _boardManager.generatedTiles.FirstOrDefault(t => t.GetGridPosition() == stairsPos)
            ?? _boardManager.CreateNewTile(_boardManager.emptyTileData, stairsPos, parent);

        ReplaceTileAndUpdatePaths(stairsTarget, _boardManager.stairsTileData);
        _boardManager.UpdateNeighbors(path.EndTile);

        // Buscar la nueva stairs tras el reemplazo
        BoardTile newStairs = _boardManager.generatedTiles.FirstOrDefault(t => t.GetGridPosition() == stairsPos);
        if (newStairs == null) return;

        _boardManager.UpdateNeighbors(newStairs);

        List<BoardTile> validNeighbors = newStairs.neighbors.Where(n => n != null).ToList();
        int externalNeighbors = validNeighbors.Count(n => !path.Contains(n));
        if (externalNeighbors > 0)
        {
            ReplaceTileAndUpdatePaths(newStairs, _boardManager.emptyTileData);
            _boardManager.UpdateNeighbors(_boardManager.generatedTiles.FirstOrDefault(t => t.GetGridPosition() == stairsPos));
            return;
        }

        TryPlaceSpecialTileAt(path, path.Tiles.Count - _config.BossTileDistanceFromEnd,
            _boardManager.bossTileData, startZoneSet);

        TryPlaceSpecialTileAt(path, path.Tiles.Count - _config.ChestOfEgressTileDistFromEnd,
            _boardManager.chestOfEgressTileData, startZoneSet);
    }

    private void TryPlaceSpecialTileAt(BoardPath path, int index, BoardTileData data, HashSet<BoardTile> startZoneSet)
    {
        if (index < 0 || index >= path.Tiles.Count) return;

        BoardTile tile = path.Tiles[index];
        if (startZoneSet.Contains(tile)) return;
        if (tile.GetData() == _boardManager.stairsTileData) return;
        if (tile.GetData() == _boardManager.bossTileData) return;
        if (tile.GetData() == _boardManager.chestOfEgressTileData) return;

        ReplaceTileAndUpdatePaths(tile, data);
        if (index < path.Tiles.Count && path.Tiles[index] != null)
            _boardManager.UpdateNeighbors(path.Tiles[index]);
    }

    // — Enemies & Chests ———————————————————————————————————————————————————————

    public void AssignEnemiesAndChests(int numEnemies, int numChests)
    {
        if (_allPaths == null || _allPaths.Count == 0) return;

        // Inicializar el caché con las tiles especiales ya existentes (stairs, boss, etc.)
        RebuildOccupiedCache();

        BoardPath mainPath = _allPaths[0];
        HashSet<BoardTile> startZoneSet = mainPath.GetStartZone(_config.StartZoneReserveTiles).ToHashSet();

        List<BoardTile> mainCandidates = GetValidEnemyPositions(mainPath, startZoneSet);
        PlaceEnemiesOnPath(mainCandidates, numEnemies);

        for (int b = 1; b < _allPaths.Count; b++)
        {
            BoardPath branch = _allPaths[b];
            if (branch == null || branch.Tiles.Count <= 1) continue;

            BoardTile originOnMain = branch.OriginTile;
            int originIdx = mainPath.Tiles.IndexOf(originOnMain);
            if (originIdx < 0) originIdx = 0;

            int enemiesInSharedSegment = mainPath.Tiles
                .Take(originIdx + 1)
                .Count(t => IsEnemyTile(t));

            int enemiesNeeded = numEnemies - enemiesInSharedSegment;
            if (enemiesNeeded <= 0) continue;

            List<BoardTile> branchCandidates = GetValidEnemyPositions(branch, startZoneSet)
                .Where(t => t != originOnMain)
                .ToList();

            PlaceEnemiesOnPath(branchCandidates, enemiesNeeded);
        }

        PlaceChests(startZoneSet, numChests);
    }

    /// <summary>
    /// Garantiza que la primera tile de cada branch (Tiles[1]) sea EmptyTile.
    /// </summary>
    public void EnsureBranchEntriesAreEmpty()
    {
        for (int b = 1; b < _allPaths.Count; b++)
        {
            BoardPath branch = _allPaths[b];
            if (branch == null || branch.Tiles.Count < 2) continue;

            BoardTile entry = branch.Tiles[1];
            if (entry is EmptyTile || entry is BranchOriginTile) continue;

            int swapIdx = -1;
            for (int i = 2; i < branch.Tiles.Count; i++)
            {
                if (branch.Tiles[i] is EmptyTile)
                {
                    swapIdx = i;
                    break;
                }
            }

            if (swapIdx != -1)
                branch.SwapTilePositions(1, swapIdx, _boardManager);
        }
    }

    // — Helpers ————————————————————————————————————————————————————————————————

    /// <summary>
    /// Reconstruye el set de posiciones con tiles especiales a partir de generatedTiles.
    /// Se llama antes de asignar enemigos y cofres para tener un estado limpio.
    /// </summary>
    private void RebuildOccupiedCache()
    {
        _occupiedBySpecial.Clear();
        foreach (BoardTile tile in _boardManager.generatedTiles)
        {
            if (tile == null) continue;
            if (!IsPlainEmptyTile(tile))
                _occupiedBySpecial.Add(tile.GetGridPosition());
        }
    }

    /// <summary>
    /// Devuelve true si la tile es una EmptyTile pura (no BranchOriginTile ni ningún tipo especial).
    /// </summary>
    private bool IsPlainEmptyTile(BoardTile tile)
    {
        return tile is EmptyTile && tile.GetData() == _boardManager.emptyTileData;
    }

    private void PlaceEnemiesOnPath(List<BoardTile> candidates, int count)
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
            currentIndex += Random.Range(minStep, maxStep + 1);
        }

        if (placed >= count) return;

        List<int> remaining = Enumerable.Range(0, candidates.Count)
            .Where(i => !IsEnemyTile(candidates[i]) && !HasSpecialNeighbor(candidates[i]))
            .ToList();
        Shuffle(remaining);

        foreach (int idx in remaining)
        {
            if (placed >= count) break;
            BoardTile newTile = ReplaceTileAndUpdatePaths(candidates[idx], _boardManager.enemyTileData);
            candidates[idx] = newTile;
            placed++;
        }
    }

    private void PlaceChests(HashSet<BoardTile> startZoneSet, int numChests)
    {
        HashSet<BoardTile> allEndZones = new HashSet<BoardTile>();
        foreach (BoardPath path in _allPaths)
        {
            if (path == null || path.Tiles.Count == 0) continue;
            allEndZones.UnionWith(path.GetEndZone(_config.EndZoneReservedTiles));
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
        foreach (BoardTile tile in available)
        {
            if (placed >= numChests) break;
            if (HasSpecialNeighbor(tile)) continue;
            ReplaceTileAndUpdatePaths(tile, _boardManager.chestTileData);
            placed++;
        }

        Debug.Log($"Cofres colocados: {placed}");
    }

    private List<BoardTile> GetValidEnemyPositions(BoardPath path, HashSet<BoardTile> startZoneSet)
    {
        HashSet<BoardTile> endZone = path.GetEndZone(_config.EndZoneReservedTiles);
        return path.Tiles.Where(t =>
            t != null &&
            !startZoneSet.Contains(t) &&
            !endZone.Contains(t) &&
            !IsEnemyTile(t) &&
            t.GetData() != _boardManager.stairsTileData &&
            t.GetData() != _boardManager.bossTileData &&
            t is EmptyTile  // solo EmptyTile; BranchOriginTile queda excluida
        ).ToList();
    }

    public BoardTile ReplaceTileAndUpdatePaths(BoardTile oldTile, BoardTileData newData)
    {
        Vector2Int gridPos = oldTile.GetGridPosition();
        _boardManager.ReplaceTileType(oldTile, newData);
        BoardTile newTile = _boardManager.generatedTiles.FirstOrDefault(t => t.GetGridPosition() == gridPos);

        foreach (BoardPath path in _allPaths)
            path.UpdateTileReference(oldTile, newTile);

        return newTile;
    }

    private bool IsEnemyTile(BoardTile tile)
    {
        if (tile == null) return false;
        return tile.GetData() == _boardManager.enemyTileData;
    }

    private bool HasSpecialNeighbor(BoardTile tile)
    {
        _boardManager.UpdateNeighbors(tile);
        foreach (BoardTile neighbor in tile.neighbors)
        {
            if (neighbor == null) continue;
            if (neighbor.GetData() != _boardManager.emptyTileData)
                return true;
        }
        return false;
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }
}
