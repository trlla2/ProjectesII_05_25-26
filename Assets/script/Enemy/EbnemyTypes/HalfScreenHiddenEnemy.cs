using System.Collections.Generic;
using UnityEngine;

public class HalfScreenHiddenDestroySpell : HiddenDestroySpell
{
    public enum HalfScreenPattern { TopLeft, TopRight, BottomLeft, BottomRight }

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

        int size = grid.GridSize;

        // Define all 4 corner 2x2 origins
        Vector2Int[] cornerOrigins = new Vector2Int[]
        {
            new Vector2Int(0,        size - 2), // TopLeft
            new Vector2Int(size - 2, size - 2), // TopRight
            new Vector2Int(0,        0),         // BottomLeft
            new Vector2Int(size - 2, 0),         // BottomRight
        };

        // Pick any 2 distinct corners
        int firstIndex = forcedPattern.HasValue
            ? (int)forcedPattern.Value
            : Random.Range(0, 4);

        int secondIndex;
        do { secondIndex = Random.Range(0, 4); } while (secondIndex == firstIndex);

        Vector2Int[] selectedOrigins = new Vector2Int[] { cornerOrigins[firstIndex], cornerOrigins[secondIndex] };

        foreach (Vector2Int origin in selectedOrigins)
        {
            for (int dx = 0; dx < 2; dx++)
            {
                for (int dy = 0; dy < 2; dy++)
                {
                    Vector2Int pos = new Vector2Int(origin.x + dx, origin.y + dy);

                    if (pos.x < 0 || pos.x >= size || pos.y < 0 || pos.y >= size) continue;

                    Element element = grid.GetElementAt(pos);
                    if (element == null || element == grid.EmptyElement) continue;

                    targetedPositions.Add(pos);

                    ElementVisual visual = grid.GetElementVisualAt(pos);
                    if (visual != null)
                        visual.SetTargetedByEnemy(true);
                }
            }
        }
    }
}