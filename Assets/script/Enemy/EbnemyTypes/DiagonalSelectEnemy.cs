using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DiagonalDestroySpell : EnemySpellBase
{
    private bool useMainDiagonal;

    public DiagonalDestroySpell(float damage = 4f, int moves = 2, bool mainDiagonal = true)
    {
        damagePerElement = damage;
        remainingMoves = moves;
        initialMoves = moves;
        spellName = "Diagonal Destroy";
        useMainDiagonal = mainDiagonal;
    }

    public override void Execute(Grid grid)
    {
        base.Execute(grid);
        targetedPositions.Clear();

        if (grid == null)
            return;

        useMainDiagonal = Random.value > 0.5f;

        int size = Grid.GridSize;
        for (int i = 0; i < size; i++)
        {
            Vector2Int pos = useMainDiagonal ? new Vector2Int(i, i) : new Vector2Int(i, size - 1 - i);

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