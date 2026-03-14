using System.Collections.Generic;
using UnityEngine;

public class TutorialStage2TimedSpell : TimedDestroySpellBase
{
    private static Vector2Int[] FixedTarget = { new Vector2Int(2, 2), new Vector2Int(1, 1) };

    private int executeCount;

    public TutorialStage2TimedSpell(float timer = 5f, float damage = 4f, bool excludeBelowTargets = true) : base(timer, damage, excludeBelowTargets)
    {
        spellName = "Tutorial Stage 2 Timed Target";
        shouldRerollAfterAttack = false;
    }

    protected override void SelectTargets(Grid grid)
    {
        Debug.Log(executeCount);
        Vector2Int target = executeCount < 2 ? FixedTarget[executeCount] : PickRandom(grid);
        executeCount++;
        Debug.Log(executeCount);

        if (target.x == -1)
            return;

        if (!grid.IsPositionValid(target))
            return;

        Element element = grid.GetElementAt(target);
        if (element == null || element == grid.EmptyElement)
            return;

        targetedPositions.Add(target);
        grid.GetElementVisualAt(target)?.SetTargetedByEnemy(true);
    }

    private Vector2Int PickRandom(Grid grid)
    {
        List<Vector2Int> candidates = new List<Vector2Int>();

        for (int x = 0; x < Grid.GridSize; x++)
        {
            for (int y = 0; y < Grid.GridSize; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (!grid.IsPositionValid(pos))
                    continue;

                Element element = grid.GetElementAt(pos);
                if (element != null && element != grid.EmptyElement)
                    candidates.Add(pos);
            }
        }

        return candidates.Count > 0 ? candidates[Random.Range(0, candidates.Count)] : new Vector2Int(-1, -1);
    }
}