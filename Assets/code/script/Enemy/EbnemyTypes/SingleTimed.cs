using System.Collections.Generic;
using UnityEngine;

public class SingleElementTimedSpell : TimedDestroySpellBase
{
    public SingleElementTimedSpell(float timer = 4f, float damage = 4f, bool excludeBelowTargets = true)
        : base(0f, timer, damage, excludeBelowTargets)
    {
        spellName = "Single Timed Target";
        shouldRerollAfterAttack = false;
    }

    protected override void SelectTargets(Grid grid)
    {
        Vector2Int pick = PickRandomValidPosition(grid);
        if (pick.x == -1)
            return;

        targetedPositions.Add(pick);

        ElementVisual visual = grid.GetElementVisualAt(pick);
        if (visual != null)
        {
            visual.SetTargetedByEnemy(true);
            visual.UpdateTargetVisual(1, 1, grid.EnemyTargetAnimDuration, grid.EnemyTargetFillCurve);
        }
    }

    private Vector2Int PickRandomValidPosition(Grid grid)
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
                {
                    candidates.Add(pos);
                }
            }
        }

        if (candidates.Count == 0)
            return new Vector2Int(-1, -1);

        int i = Random.Range(0, candidates.Count);
        return candidates[i];
    }
}