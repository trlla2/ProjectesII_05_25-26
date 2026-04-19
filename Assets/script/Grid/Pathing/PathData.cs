using System.Collections.Generic;
using UnityEngine;
public class PathData
{
    private List<Vector2Int> positions;
    private int maxLength;

    public int Count => positions.Count;
    public int MaxLength => maxLength;
    public bool IsFull => positions.Count >= maxLength;
    public Vector2Int[] Positions => positions.ToArray();

    public PathData(int maxPathLength)
    {
        maxLength = maxPathLength;
        positions = new List<Vector2Int>();
    }

    public bool TryAdd(Vector2Int position)
    {
        if (IsFull || positions.Contains(position))
            return false;

        positions.Add(position);
        return true;
    }

    public bool TryRemoveLast(out Vector2Int removedPosition)
    {
        removedPosition = Vector2Int.zero;

        if (positions.Count == 0)
            return false;

        removedPosition = positions[positions.Count - 1];
        positions.RemoveAt(positions.Count - 1);
        return true;
    }

    public Vector2Int GetLast()
    {
        if (positions.Count == 0)
            return Vector2Int.zero;

        return positions[positions.Count - 1];
    }

    public Vector2Int GetSecondToLast()
    {
        if (positions.Count < 2)
            return Vector2Int.zero;

        return positions[positions.Count - 2];
    }

    public bool Contains(Vector2Int position)
    {
        return positions.Contains(position);
    }

    public void Clear()
    {
        positions.Clear();
    }

    public List<Element> GetElements(Grid grid)
    {
        List<Element> elements = new List<Element>();

        foreach (Vector2Int pos in positions)
        {
            Element element = grid.GetElementAt(pos);
            if (element != null)
                elements.Add(element);
        }

        return elements;
    }
}