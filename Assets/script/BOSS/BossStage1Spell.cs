using System.Collections.Generic;
using UnityEngine;

public class BossStage1Spell : EnemySpellBase
{
    public BossStage1Spell(float damage = 4f, int moves = 2)
    {
        damagePerElement = damage;
        remainingMoves = moves;
        initialMoves = moves;
        spellName = "BossStage1 Attack";
    }

    public override void Execute(Grid grid)
    {
        targetedPositions.Clear();
        if (grid == null) return;

        int size = Grid.GridSize;
        bool performColumnsAttack = Random.value > 0.5f;

        if (performColumnsAttack)
        {
            List<int> availableColumns = new List<int>();
            for (int i = 0; i < size; i++) availableColumns.Add(i);

            int col1 = availableColumns[Random.Range(0, availableColumns.Count)];
            availableColumns.Remove(col1);
            int col2 = availableColumns[Random.Range(0, availableColumns.Count)];

            int[] chosenColumns = { col1, col2 };

            foreach (int col in chosenColumns)
            {
                for (int row = 0; row < size; row++)
                {
                    Vector2Int pos = new Vector2Int(col, row);
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
        else // diagonals
        {
            for (int d = 0; d < 2; d++)
            {
                for (int i = 0; i < size; i++)
                {
                    Vector2Int pos = d == 0 ? new Vector2Int(i, i) : new Vector2Int(i, size - 1 - i);
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
    }
}