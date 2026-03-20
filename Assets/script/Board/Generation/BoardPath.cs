using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardPath
{
    public List<BoardTile> Tiles { get; private set; } = new List<BoardTile>();
    public BoardTile OriginTile => Tiles.Count > 0 ? Tiles[0] : null;
    public BoardTile EndTile => Tiles.Count > 0 ? Tiles[Tiles.Count - 1] : null;
    public bool IsHidden { get; set; }
    public bool IsMain { get; set; }

    public BoardPath(bool isMain = false, bool isHidden = false)
    {
        IsMain = isMain;
        IsHidden = isHidden;
    }

    public void AddTile(BoardTile tile) => Tiles.Add(tile);

    public bool Contains(BoardTile tile) => Tiles.Contains(tile);

    public IEnumerable<BoardTile> GetStartZone(int startZoneCount)
    {
        int count = Mathf.Clamp(startZoneCount, 1, Tiles.Count);
        return Tiles.Take(count);
    }

    public HashSet<BoardTile> GetEndZone(int endZoneCount)
    {
        return Tiles
            .Skip(Mathf.Max(0, Tiles.Count - endZoneCount))
            .ToHashSet();
    }

    public bool IsTerminal(BoardManager boardManager)
    {
        if (Tiles == null || Tiles.Count == 0) return false;

        boardManager.UpdateNeighbors(EndTile);
        List<BoardTile> neighbors = EndTile.neighbors.Where(n => n != null).ToList();

        if (neighbors.Count == 0) return true;

        Vector2Int prevPos = Tiles.Count >= 2
            ? Tiles[Tiles.Count - 2].GetGridPosition()
            : new Vector2Int(int.MinValue, int.MinValue);

        foreach (BoardTile n in neighbors)
        {
            if (n.GetGridPosition() != prevPos)
                return false;
        }
        return true;
    }

    public void SwapTilePositions(int indexA, int indexB, BoardManager boardManager)
    {
        BoardTile tileA = Tiles[indexA];
        BoardTile tileB = Tiles[indexB];

        Vector2Int posA = tileA.GetGridPosition();
        Vector2Int posB = tileB.GetGridPosition();

        tileA.SetGridPosition(posB);
        tileB.SetGridPosition(posA);

        tileA.transform.position = new Vector3(posB.x, tileA.transform.position.y, posB.y);
        tileB.transform.position = new Vector3(posA.x, tileB.transform.position.y, posA.y);

        Tiles[indexA] = tileB;
        Tiles[indexB] = tileA;

        tileA.neighbors.Clear();
        tileB.neighbors.Clear();
        boardManager.UpdateNeighbors(tileA);
        boardManager.UpdateNeighbors(tileB);

        foreach (BoardTile neighbor in tileA.neighbors)
            if (neighbor != null) boardManager.UpdateNeighbors(neighbor);

        foreach (BoardTile neighbor in tileB.neighbors)
            if (neighbor != null) boardManager.UpdateNeighbors(neighbor);

        Debug.Log($"BoardPath.SwapTilePositions: intercambiadas posiciones {posA} y {posB}.");
    }

    public void UpdateTileReference(BoardTile oldTile, BoardTile newTile)
    {
        for (int i = 0; i < Tiles.Count; i++)
        {
            if (Tiles[i] == oldTile || Tiles[i].GetGridPosition() == newTile.GetGridPosition())
                Tiles[i] = newTile;
        }
    }
}
