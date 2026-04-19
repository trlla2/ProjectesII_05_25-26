
using System.Collections.Generic;
using UnityEngine;

public class RandomDestroySpell : EnemySpellBase
{
    private int elementCount;

    public RandomDestroySpell(int count = 3, float damage = 4f, int moves = 2)
    {
        elementCount = count;
        damagePerElement = damage;
        remainingMoves = moves;
        initialMoves = moves;
        spellName = "Random Destroy";
    }

    public override void Execute(Grid grid)
    {
        base.Execute(grid);
        targetedPositions.Clear();

        if (grid == null)
            return;

        List<Vector2Int> allPositions = new List<Vector2Int>();

        for (int x = 0; x < grid.GridSize; x++)
        {
            for (int y = 0; y < grid.GridSize; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                Element element = grid.GetElementAt(pos);
                if (element != null && element != grid.EmptyElement)
                {
                    allPositions.Add(pos);
                }
            }
        }

        int targetCount = Mathf.Min(elementCount, allPositions.Count);
        for (int i = 0; i < targetCount; i++)
        {
            if (allPositions.Count == 0)
                break;

            int randomIndex = Random.Range(0, allPositions.Count);
            Vector2Int chosen = allPositions[randomIndex];
            targetedPositions.Add(chosen);

            ElementVisual visual = grid.GetElementVisualAt(chosen);
            if (visual != null)
            {
                visual.SetTargetedByEnemy(true);
                visual.UpdateTargetVisual(remainingMoves, initialMoves, grid.EnemyTargetAnimDuration, grid.EnemyTargetFillCurve);
            }

            allPositions.RemoveAt(randomIndex);
        }
    }
}