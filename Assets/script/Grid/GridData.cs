using UnityEngine;
public class GridData
{
    private Element[,] elements;
    private ElementVisual[,] visuals;
    private int gridSize;

    public int GridSize => gridSize;

    public GridData(int size)
    {
        gridSize = size;
        elements = new Element[size, size];
        visuals = new ElementVisual[size, size];
    }

    public Element GetElementAt(Vector2Int position)
    {
        if (!IsPositionValid(position))
            return null;

        return elements[position.x, position.y];
    }

    public void SetElementAt(Vector2Int position, Element element)
    {
        if (!IsPositionValid(position))
            return;

        elements[position.x, position.y] = element;
    }

    public ElementVisual GetVisualAt(Vector2Int position)
    {
        if (!IsPositionValid(position))
            return null;

        return visuals[position.x, position.y];
    }

    public void SetVisualAt(Vector2Int position, ElementVisual visual)
    {
        if (!IsPositionValid(position))
            return;

        visuals[position.x, position.y] = visual;
    }

    public Element[,] GetAllElements()
    {
        return (Element[,])elements.Clone();
    }

    public bool IsPositionValid(Vector2Int position)
    {
        return position.x >= 0 && position.x < gridSize &&
               position.y >= 0 && position.y < gridSize;
    }

    public void SwapElements(Vector2Int posA, Vector2Int posB)
    {
        if (!IsPositionValid(posA) || !IsPositionValid(posB))
            return;

        Element tempElement = elements[posA.x, posA.y];
        elements[posA.x, posA.y] = elements[posB.x, posB.y];
        elements[posB.x, posB.y] = tempElement;

        ElementVisual tempVisual = visuals[posA.x, posA.y];
        visuals[posA.x, posA.y] = visuals[posB.x, posB.y];
        visuals[posB.x, posB.y] = tempVisual;

        if (visuals[posA.x, posA.y] != null)
            visuals[posA.x, posA.y].Initialize(posA, elements[posA.x, posA.y]);

        if (visuals[posB.x, posB.y] != null)
            visuals[posB.x, posB.y].Initialize(posB, elements[posB.x, posB.y]);
    }
}