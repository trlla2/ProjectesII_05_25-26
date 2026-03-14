using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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

        for (int row = 0; row < gridSize; row++)
        {
            Vector2Int currentPos = new Vector2Int(columnIndex, row);
            Element currentElement = gridData.GetElementAt(currentPos);

            if (currentElement == emptyElement || currentElement == null)
            {
                for (int aboveRow = row + 1; aboveRow < gridSize; aboveRow++)
                {
                    Vector2Int abovePos = new Vector2Int(columnIndex, aboveRow);
                    Element aboveElement = gridData.GetElementAt(abovePos);

                    if (aboveElement != null && aboveElement != emptyElement)
                    {
                        gridData.SetElementAt(currentPos, aboveElement);
                        gridData.SetElementAt(abovePos, emptyElement);

                        ElementVisual currentVisual = gridData.GetVisualAt(currentPos);
                        ElementVisual aboveVisual = gridData.GetVisualAt(abovePos);

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