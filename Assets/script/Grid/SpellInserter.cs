using System.Collections.Generic;
using UnityEngine;

public interface ISpellInserter
{
    bool TryInsertSpell(GridData gridData, List<Element> spellRecipe);
}

public class SpellInserter : ISpellInserter
{
    private static readonly Vector2Int[] HorizontalDirections =
    {
        Vector2Int.left,
        Vector2Int.right
    };

    public bool TryInsertSpell(GridData gridData, List<Element> spellRecipe)
    {
        if (spellRecipe == null || spellRecipe.Count == 0)
            return false;

        int maxAttempts = 50;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector2Int startPos = new Vector2Int(
                Random.Range(0, gridData.GridSize),
                Random.Range(0, gridData.GridSize)
            );

            if (TryPlaceSpellAtPosition(gridData, spellRecipe, startPos))
                return true;
        }

        return false;
    }

    private bool TryPlaceSpellAtPosition(GridData gridData, List<Element> spellRecipe, Vector2Int startPos)
    {
        if (spellRecipe.Count > gridData.GridSize)
            return false;

        int requiredSpace = spellRecipe.Count - 1;
        int spaceLeft = startPos.x;
        int spaceRight = gridData.GridSize - 1 - startPos.x;

        if (spaceLeft + spaceRight < requiredSpace)
            return false;

        Vector2Int direction = HorizontalDirections[Random.Range(0, HorizontalDirections.Length)];
        List<Vector2Int> positions = BuildHorizontalPath(startPos, direction, spellRecipe.Count, gridData.GridSize);

        if (positions == null || positions.Count != spellRecipe.Count)
            return false;

        for (int i = 0; i < spellRecipe.Count; i++)
        {
            gridData.SetElementAt(positions[i], spellRecipe[i]);

            ElementVisual visual = gridData.GetVisualAt(positions[i]);
            if (visual != null)
                visual.UpdateVisual(spellRecipe[i]);
        }

        return true;
    }

    private List<Vector2Int> BuildHorizontalPath(Vector2Int start, Vector2Int direction, int length, int gridSize)
    {
        List<Vector2Int> positions = new List<Vector2Int> { start };
        Vector2Int current = start;

        for (int i = 1; i < length; i++)
        {
            Vector2Int next = current + direction;

            if (next.x < 0 || next.x >= gridSize)
            {
                direction = -direction;
                next = current + direction;
            }

            if (next.x < 0 || next.x >= gridSize || positions.Contains(next))
                return null;

            positions.Add(next);
            current = next;
        }

        return positions;
    }
}