using UnityEngine;

public interface IPathValidator
{
    bool CanAddToPath(Vector2Int position, Vector2Int[] currentPath);
    bool IsAdjacentWorld(ElementVisual visualA, ElementVisual visualB);
}

public class WorldSpacePathValidator : IPathValidator
{
    private readonly Grid grid;
    private readonly float adjacencyDistance;

    public WorldSpacePathValidator(Grid gridController, float worldAdjacencyDistance)
    {
        grid = gridController;
        adjacencyDistance = worldAdjacencyDistance;
    }

    public bool CanAddToPath(Vector2Int position, Vector2Int[] currentPath)
    {
        if (currentPath == null || currentPath.Length == 0)
            return true;

        Vector2Int lastPosition = currentPath[currentPath.Length - 1];

        ElementVisual visualA = grid.GetElementVisualAt(position);
        ElementVisual visualB = grid.GetElementVisualAt(lastPosition);

        if (visualA == null || visualB == null)
            return false;

        return IsAdjacentWorld(visualA, visualB);
    }

    public bool IsAdjacentWorld(ElementVisual visualA, ElementVisual visualB)
    {
        if (visualA == null || visualB == null)
            return false;

        Vector3 posA = visualA.GetWorldPosition();
        Vector3 posB = visualB.GetWorldPosition();

        float distance = Vector2.Distance(posA, posB);

        return distance <= adjacencyDistance;
    }
}