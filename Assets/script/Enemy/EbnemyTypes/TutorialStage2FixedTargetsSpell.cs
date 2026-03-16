using UnityEngine;

public class TutorialStage2FixedTargetsSpell : EnemySpellBase
{
    private static Vector2Int[] FixedTargetPattern = new Vector2Int[]
    {
        new Vector2Int(0, 3),
        new Vector2Int(1, 2),
        new Vector2Int(2, 1),
        new Vector2Int(3, 0)
    };

    public TutorialStage2FixedTargetsSpell(float damage = 4f, int moves = 4)
    {
        damagePerElement = damage;
        remainingMoves = moves;
        initialMoves = moves;
        spellName = "Tutorial Stage 2 - Fixed Targets";
    }

    public override void Execute(Grid grid)
    {
        targetedPositions.Clear();

        if (grid == null)
            return;

        for (int i = 0; i < FixedTargetPattern.Length; i++)
        {
            Vector2Int pos = FixedTargetPattern[i];

            if (!grid.IsPositionValid(pos))
                continue;

            Element element = grid.GetElementAt(pos);
            if (element == null || element == grid.EmptyElement)
                continue;

            targetedPositions.Add(pos);

            ElementVisual visual = grid.GetElementVisualAt(pos);
            if (visual != null)
            {
                visual.SetTargetedByEnemy(true);
                visual.UpdateTargetVisual(
                    remainingMoves,
                    initialMoves,
                    grid.EnemyTargetAnimDuration,
                    grid.EnemyTargetFillCurve
                );
            }
        }
    }
}