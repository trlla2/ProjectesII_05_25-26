using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    public static List<BoardTile> FindPath(BoardTile start, BoardTile goal)
    {
        if (start == null || goal == null)
            return null;

        List<BoardTile> openSet = new List<BoardTile> { start };
        HashSet<BoardTile> closedSet = new HashSet<BoardTile>();

        Dictionary<BoardTile, float> gScore = new Dictionary<BoardTile, float> { [start] = 0f };
        Dictionary<BoardTile, float> fScore = new Dictionary<BoardTile, float> { [start] = Heuristic(start, goal) };

        Dictionary<BoardTile, BoardTile> cameFrom = new Dictionary<BoardTile, BoardTile>();

        while (openSet.Count > 0)
        {
            BoardTile current = openSet.OrderBy(t => fScore.ContainsKey(t) ? fScore[t] : float.MaxValue).First();

            if (current == goal)
                return ReconstructPath(cameFrom, current);

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (var neighbor in current.neighbors)
            {
                if (neighbor == null || closedSet.Contains(neighbor))
                    continue;

                if (!neighbor.GetData().walkable && neighbor != goal)
                    continue;

                float tentativeG = gScore[current] + Heuristic(current, neighbor);

                if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + Heuristic(neighbor, goal);

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return null;
    }

    private static float Heuristic(BoardTile a, BoardTile b)
    {
        return Vector2Int.Distance(a.GetGridPosition(), b.GetGridPosition());
    }


    private static List<BoardTile> ReconstructPath(Dictionary<BoardTile, BoardTile> cameFrom, BoardTile current)
    {
        List<BoardTile> path = new List<BoardTile> { current };

        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }

        return path;
    }
}
