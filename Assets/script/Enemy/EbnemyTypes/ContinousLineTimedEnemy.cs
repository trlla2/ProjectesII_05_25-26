using System.Collections.Generic;
using UnityEngine;

public class ContinousLineTimedEnemy : MultipleTimedSpellBase
{
    public ContinousLineTimedEnemy(
        float selectionInterval = 2.5f,
        float timer = 8f,
        float damage = 4f,
        float bonusTime = 3f)
        : base(selectionInterval, timer, damage, bonusTime)
    {
        spellName = "Continuous Line Timed Target";
    }


    private static readonly Vector2Int[] Cardinals = new Vector2Int[]
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    // Tracks the last 4 targeted positions to exclude as the "snake"
    private Queue<Vector2Int> recentPositions = new Queue<Vector2Int>();
    private const int SnakeMemory = 4;

    protected override Vector2Int PickTargetPosition(Grid grid)
    {
        //If there's a previous position, try to step cardinally from it
        if (recentPositions.Count > 0)
        {
            Vector2Int lastPos = GetLastPosition();
            List<Vector2Int> candidates = new List<Vector2Int>();

            foreach (Vector2Int dir in Cardinals)
            {
                Vector2Int candidate = lastPos + dir;

                if (!grid.IsPositionValid(candidate)) continue;
                if (recentPositions.Contains(candidate)) continue;
                if (targetedPositions.Contains(candidate)) continue;

                Element element = grid.GetElementAt(candidate);
                if (element != null && element != grid.EmptyElement)
                    candidates.Add(candidate);
            }

            if (candidates.Count > 0)
            {
                Vector2Int chosen = candidates[Random.Range(0, candidates.Count)];
                TrackPosition(chosen);
                return chosen;
            }
        }

        // No valid neighbour; fall back to a random valid position
        Vector2Int fallback = PickRandomValidPosition(grid);
        if (fallback.x != -1)
            TrackPosition(fallback);

        return fallback;
    }

    private Vector2Int GetLastPosition()
    {
        // Queue doesn't expose a "peek last" directly, so we count it
        Vector2Int last = Vector2Int.zero;
        foreach (Vector2Int pos in recentPositions)
            last = pos;
        return last;
    }

    private void TrackPosition(Vector2Int pos)
    {
        recentPositions.Enqueue(pos);
        if (recentPositions.Count > SnakeMemory)
            recentPositions.Dequeue();
    }
}
