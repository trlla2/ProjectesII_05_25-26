using UnityEngine;

public class ColumnDestroyLvl2Spell : TimedDestroySpellBase
{
    public ColumnDestroyLvl2Spell(float damage = 4f, float delay = 5f, float countdown = 15f, bool excludeBelow = true)
        : base(delay, countdown, damage, excludeBelow)
    {
        spellName = "Column Destroy Lvl2";
    }

    protected override void SelectTargets(Grid grid)
    {
        int targetColumn = Random.Range(0, Grid.GridSize);

        for (int row = 0; row < Grid.GridSize; row++)
        {
            Vector2Int pos = new Vector2Int(targetColumn, row);
            if (!grid.IsPositionValid(pos)) continue;

            Element element = grid.GetElementAt(pos);
            if (element != null && element != grid.EmptyElement)
            {
                targetedPositions.Add(pos);
                ElementVisual visual = grid.GetElementVisualAt(pos);
                if (visual != null)
                {
                    visual.SetTargetedByEnemy(true);
                    visual.UpdateTargetVisual(1, 1, grid.EnemyTargetAnimDuration, grid.EnemyTargetFillCurve);
                }
            }
        }
    }
}