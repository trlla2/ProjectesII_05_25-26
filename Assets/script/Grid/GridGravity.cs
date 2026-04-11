using System.Collections.Generic;
using UnityEngine;

public class GridGravity
{
    private GridData gridData;
    private Element emptyElement;

    public GridGravity(GridData data, Element empty)
    {
        gridData = data;
        emptyElement = empty;
    }

    public List<Vector2Int> ApplyGravity()
    {
        List<Vector2Int> movedPositions = new List<Vector2Int>();

        for (int col = 0; col < gridData.GridSize; col++)
        {
            ProcessColumn(col, movedPositions);
        }

        return movedPositions;
    }

    private void ProcessColumn(int columnIndex, List<Vector2Int> movedPositions)
    {
        int gridSize = gridData.GridSize;

        // If the column contains a bomb in row 2 or lower (indices 0 or 1),
        // do not apply gravity to this column at all.
        for (int r = 0; r <= 1 && r < gridSize; r++)
        {
            Element el = gridData.GetElementAt(new Vector2Int(columnIndex, r));
            if (el != null && el.Type == ElementType.Bomb)
                return;
        }

        for (int row = 0; row < gridSize; row++)
        {
            Vector2Int currentPos = new Vector2Int(columnIndex, row);
            Element currentElement = gridData.GetElementAt(currentPos);

            if (currentElement == emptyElement || currentElement == null)
            {
                for (int aboveRow = row + 1; aboveRow < gridSize; aboveRow++)
                {
                    Vector2Int abovePos = new Vector2Int(columnIndex, aboveRow);

                    // Blocked elements and swap targeted elements act as anchors
                    ElementVisual aboveVisual = gridData.GetVisualAt(abovePos);
                    if (aboveVisual != null && (aboveVisual.IsBlocked || aboveVisual.IsSwapTargeted))
                        break;

                    Element aboveElement = gridData.GetElementAt(abovePos);

                    if (aboveElement != null && aboveElement != emptyElement)
                    {
                        // A bomb cannot fall into the bottom row
                        if (aboveElement.Type == ElementType.Bomb && row < 1)
                            break;

                        gridData.SetElementAt(currentPos, aboveElement);
                        gridData.SetElementAt(abovePos, emptyElement);

                        ElementVisual currentVisual = gridData.GetVisualAt(currentPos);

                        gridData.SetVisualAt(currentPos, aboveVisual);
                        gridData.SetVisualAt(abovePos, currentVisual);

                        if (aboveVisual != null)
                            aboveVisual.Initialize(currentPos, aboveElement);

                        movedPositions.Add(currentPos);
                        break;
                    }
                }
            }
        }
    }
}