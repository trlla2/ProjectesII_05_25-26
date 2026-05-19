using System.Collections.Generic;
using UnityEngine;

public class HalfScreenHiddenDestroySpell : HiddenDestroySpell
{
    public enum HalfScreenPattern { Left, Right, Top, Bottom }

    private HalfScreenPattern? forcedPattern;

    public HalfScreenHiddenDestroySpell(
        float damage = 4f,
        float attackTimer = 6f,
        float visibilityDuration = 2.5f,
        Sprite hiddenSprite = null,
        float bonusTimeOnClear = 1.5f,
        HalfScreenPattern? forcedPattern = null)
        : base(8, damage, attackTimer, visibilityDuration, hiddenSprite, bonusTimeOnClear)
    {
        this.forcedPattern = forcedPattern;
        spellName = "Half Screen Hidden Destroy";
    }

    protected override void SelectTargets(Grid grid)
    {
        if (grid == null) return;

        HalfScreenPattern pattern = forcedPattern ?? (HalfScreenPattern)Random.Range(0, 4);

        int size = grid.GridSize;
        int half = size / 2;

        List<Vector2Int> candidates = new List<Vector2Int>();

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                bool inHalf = pattern switch
                {
                    HalfScreenPattern.Left   => x < half,
                    HalfScreenPattern.Right  => x >= half,
                    HalfScreenPattern.Bottom => y < half,
                    HalfScreenPattern.Top    => y >= half,
                    _                        => false
                };

                if (!inHalf) continue;

                Vector2Int pos = new Vector2Int(x, y);
                Element element = grid.GetElementAt(pos);
                if (element != null && element != grid.EmptyElement)
                    candidates.Add(pos);
            }
        }

        foreach (var pos in candidates)
        {
            targetedPositions.Add(pos);

            ElementVisual visual = grid.GetElementVisualAt(pos);
            if (visual != null)
                visual.SetTargetedByEnemy(true);
        }
    }
}