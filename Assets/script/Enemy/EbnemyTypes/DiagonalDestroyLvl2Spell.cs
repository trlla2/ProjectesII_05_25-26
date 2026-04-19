    using UnityEngine;

public class DiagonalDestroyLvl2Spell : TimedDestroySpellBase
{
    public DiagonalDestroyLvl2Spell(float countdown = 8f, float damage = 4f, float bonusTimeOnClear = 2f)
        : base(countdown, damage)
    {
        spellName = "Diagonal Destroy Lvl2";
        bonusTimePerClearedTarget = bonusTimeOnClear;
        shouldRerollAfterAttack = true;
    }

    protected override void SelectTargets(Grid grid)
    {
        bool useMainDiagonal = Random.value > 0.5f;
        int size = grid.GridSize;

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
                }
            }
        }
    }
}