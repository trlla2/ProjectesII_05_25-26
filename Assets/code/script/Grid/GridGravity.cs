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

        //Buscar la fila más baja donde haya un elemento targeted
        int blockedRow = -1;
        for (int row = 0; row < gridSize; row++)
        {
            Vector2Int pos = new Vector2Int(columnIndex, row);
            ElementVisual e = gridData.GetVisualAt(pos);

            if (e != null && e.IsTargetedByEnemy)
            {
                blockedRow = row;
                break;
            }
        }

        //Procesar gravedad SOLO por encima de blockedRow (o toda la columna si blockedRow = -1)
        int maxProcessRow = (blockedRow == -1) ? gridSize - 1 : blockedRow - 1;

        for (int row = 0; row < maxProcessRow; row++)
        {
            Vector2Int currentPos = new Vector2Int(columnIndex, row);
            Element currentElement = gridData.GetElementAt(currentPos);

            if (currentElement == emptyElement || currentElement == null)
            {
                //Buscar elemento que pueda caer
                for (int aboveRow = row + 1; aboveRow <= maxProcessRow; aboveRow++)
                {
                    Vector2Int abovePos = new Vector2Int(columnIndex, aboveRow);
                    Element aboveElement = gridData.GetElementAt(abovePos);
                    ElementVisual aboveElementVisual = gridData.GetVisualAt(abovePos);
                    //No pueden caer elementos targeted
                    if (aboveElement != null && aboveElement != emptyElement && !aboveElementVisual.IsTargetedByEnemy)
                    {
                        // SWAP lógico
                        gridData.SetElementAt(currentPos, aboveElement);
                        gridData.SetElementAt(abovePos, emptyElement);

                        // SWAP visual
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