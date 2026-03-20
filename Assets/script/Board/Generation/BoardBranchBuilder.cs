using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardBranchBuilder
{
    private readonly BoardManager _boardManager;
    private readonly BoardGenerationConfig _config;
    private readonly List<BoardPath> _allPaths;

    public BoardBranchBuilder(BoardManager boardManager, BoardGenerationConfig config, List<BoardPath> allPaths)
    {
        _boardManager = boardManager;
        _config = config;
        _allPaths = allPaths;
    }

    public void GenerateBranches(Transform parent, BoardPath mainPath, int minBranches, int maxBranches, int minBranchLength, int maxBranchLength)
    {
        if (maxBranches <= 0 || maxBranchLength <= 0) return;

        minBranches = Mathf.Clamp(minBranches, 0, maxBranches);

        int candidatesCount = Mathf.Max(0, mainPath.Tiles.Count - _config.EndZoneReservedTiles);
        HashSet<BoardTile> startZoneSet = mainPath.GetStartZone(_config.StartZoneReserveTiles).ToHashSet();

        List<BoardTile> candidates = mainPath.Tiles
            .Take(candidatesCount)
            .Where(t => !startZoneSet.Contains(t) && t is EmptyTile)
            .ToList();

        Shuffle(candidates);

        int branchesCreated = 0;
        foreach (BoardTile origin in candidates)
        {
            if (branchesCreated >= minBranches) break;

            BoardPath branch = BuildBranch(origin, parent, maxBranchLength, minBranchLength);
            if (branch != null)
                branchesCreated++;
        }
    }

    public BoardPath BuildBranch(BoardTile origin, Transform parent, int maxLength, int minLength)
    {
        BoardPath branch = new BoardPath(isMain: false, isHidden: true);
        branch.AddTile(origin);

        BoardTile current = origin;
        List<BoardTile> instanced = new List<BoardTile>();
        HashSet<Vector2Int> occupiedAll = new HashSet<Vector2Int>(
            _boardManager.generatedTiles.Select(t => t.GetGridPosition()));

        for (int i = 0; i < maxLength; i++)
        {
            Vector2Int basePos = current.GetGridPosition();
            List<Vector2Int> neighbors = GetAllowedBranchNeighbors(basePos);
            neighbors.RemoveAll(p => branch.Tiles.Skip(1).Any(b => b.GetGridPosition() == p));

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
                branch.AddTile(current);
                instanced.Add(current);
                occupiedAll.Add(nextPos);
            }
            else
            {
                if (branch.Tiles.Count - 1 >= minLength)
                {
                    origin.generatedBranch = branch.Tiles;
                    _allPaths.Add(branch);
                    return branch;
                }
                else
                {
                    DestroyBranchTiles(instanced);
                    return null;
                }
            }
        }

        if (branch.Tiles.Count - 1 >= minLength)
        {
            origin.generatedBranch = branch.Tiles;
            _allPaths.Add(branch);
            return branch;
        }

        DestroyBranchTiles(instanced);
        return null;
    }

    public bool HasMinSeparationFromOtherPaths(Vector2Int candidatePos, BoardPath currentBranch)
    {
        if (currentBranch.Tiles.Count <= 1) return true;

        HashSet<Vector2Int> currentBranchPositions = new HashSet<Vector2Int>(
            currentBranch.Tiles.Select(t => t.GetGridPosition()));

        foreach (BoardTile tile in _boardManager.generatedTiles)
        {
            Vector2Int otherPos = tile.GetGridPosition();
            if (currentBranchPositions.Contains(otherPos)) continue;

            int dist = Mathf.Abs(candidatePos.x - otherPos.x) + Mathf.Abs(candidatePos.y - otherPos.y);
            if (dist <= _config.MinDistBetweenPaths)
                return false;
        }
        return true;
    }

    private List<Vector2Int> GetAllowedBranchNeighbors(Vector2Int basePos)
    {
        var dirs = new List<Vector2Int>();
        if (_config.AllowBranchRight) dirs.Add(new Vector2Int(basePos.x + 1, basePos.y));
        if (_config.AllowBranchLeft)  dirs.Add(new Vector2Int(basePos.x - 1, basePos.y));
        if (_config.AllowBranchUp)    dirs.Add(new Vector2Int(basePos.x, basePos.y + 1));
        if (_config.AllowBranchDown)  dirs.Add(new Vector2Int(basePos.x, basePos.y - 1));
        return dirs;
    }

    private void DestroyBranchTiles(List<BoardTile> tiles)
    {
        foreach (BoardTile tile in tiles)
        {
            _boardManager.generatedTiles.Remove(tile);
            Object.Destroy(tile.gameObject);
        }
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
