using UnityEngine;

public class DiagonalDestroyLvl2Spell : TimedDestroySpellBase
{
    public DiagonalDestroyLvl2Spell(float damage = 4f, float delay = 5f, float countdown = 15f, bool excludeBelow = true, int moveCount = 3)
        : base(delay, countdown, damage, excludeBelow)
    {
        spellName = "Diagonal Destroy Lvl2";
        useMoveTicks = true;
        SetMoveCount(moveCount);
    }

    protected override void SelectTargets(Grid grid)
    {
        bool useMainDiagonal = Random.value > 0.5f;
        int size = Grid.GridSize;

        for (int i = 0; i < size; i++)
        {
            Vector2Int pos = useMainDiagonal ? new Vector2Int(i, i) : new Vector2Int(i, size - 1 - i);

            if (!grid.IsPositionValid(pos)) continue;

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