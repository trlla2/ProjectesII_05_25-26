using System.Collections.Generic;
using UnityEngine;

public class ColumnDestroySpell : EnemySpellBase
{
    private int columnIndex;

    public ColumnDestroySpell(float damage = 4f, int moves = 2, int column = -1)
    {
        damagePerElement = damage;
        remainingMoves = moves;
        initialMoves = moves;
        columnIndex = column;
        spellName = "Column Destroy";
    }

    public override void Execute(Grid grid)
    {
        base.Execute(grid);
        targetedPositions.Clear();

        if (grid == null)
            return;

        int targetColumn;
        targetColumn = columnIndex >= 0 ? columnIndex : Random.Range(0, Grid.GridSize);

        for (int row = 0; row < Grid.GridSize; row++)
        {
            Vector2Int pos = new Vector2Int(targetColumn, row);
            if (grid.IsPositionValid(pos))
            {
                Element element = grid.GetElementAt(pos);
                if (element != null && element != grid.EmptyElement)
                {
                    targetedPositions.Add(pos);
                    ElementVisual visual = grid.GetElementVisualAt(pos);
                    if (visual != null)
                    {
                        visual.SetTargetedByEnemy(true);
                        visual.UpdateTargetVisual(remainingMoves, initialMoves, grid.EnemyTargetAnimDuration, grid.EnemyTargetFillCurve);
                    }
                }
            }
        }
    }

}