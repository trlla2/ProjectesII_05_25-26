using System.Collections.Generic;
using UnityEngine;

public class RandomDestroyLvl2Spell : TimedDestroySpellBase
{
    private readonly int elementCount;

    public RandomDestroyLvl2Spell(int count = 3, float countdown = 8f, float damage = 4f, float bonusTimeOnClear = 2f)
        : base(countdown, damage)
    {
        elementCount = count;
        spellName = "Random Destroy Lvl2";
        bonusTimePerClearedTarget = bonusTimeOnClear;
        shouldRerollAfterAttack = true;
    }

    protected override void SelectTargets(Grid grid)
    {
        List<Vector2Int> allPositions = new List<Vector2Int>();

        for (int x = 0; x < Grid.GridSize; x++)
            for (int y = 0; y < Grid.GridSize; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                Element element = grid.GetElementAt(pos);
                if (element != null && element != grid.EmptyElement)
                    allPositions.Add(pos);
            }

        int targetCount = Mathf.Min(elementCount, allPositions.Count);
        for (int i = 0; i < targetCount; i++)
        {
            if (allPositions.Count == 0) break;

            int randomIndex = Random.Range(0, allPositions.Count);
            Vector2Int chosen = allPositions[randomIndex];
            targetedPositions.Add(chosen);

            ElementVisual visual = grid.GetElementVisualAt(chosen);
            if (visual != null)
            {
                visual.SetTargetedByEnemy(true);
            }

            allPositions.RemoveAt(randomIndex);
        }
    }
}