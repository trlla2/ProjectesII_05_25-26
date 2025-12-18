using System.Collections.Generic;
using UnityEngine;

public interface ISpellInserter
{
    bool TryInsertSpell(GridData gridData, List<Element> spellRecipe);
}

public class SpellInserter : ISpellInserter
{
    public SpellInserter() { }

    public bool TryInsertSpell(GridData gridData, List<Element> spellRecipe)
    {
        if (spellRecipe == null || spellRecipe.Count == 0)
            return false;

        // ry random positions multiple times
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
        Vector2Int currentPos = startPos;
        List<Vector2Int> positions = new List<Vector2Int> { currentPos };

        for (int i = 1; i < spellRecipe.Count; i++)
        {
            Vector2Int direction = GetRandomDirection();
            Vector2Int nextPos = currentPos + direction;

            if (!gridData.IsPositionValid(nextPos) || positions.Contains(nextPos))
                return false;

            positions.Add(nextPos);
            currentPos = nextPos;
        }

        //Implace the spell elements
        for (int i = 0; i < spellRecipe.Count; i++)
        {
            gridData.SetElementAt(positions[i], spellRecipe[i]);

            ElementVisual visual = gridData.GetVisualAt(positions[i]);
            if (visual != null)
                visual.UpdateVisual(spellRecipe[i]);
        }

        return true;
    }

    private Vector2Int GetRandomDirection()
    {
        Vector2Int[] directions = new Vector2Int[]
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        return directions[Random.Range(0, directions.Length)];
    }
}