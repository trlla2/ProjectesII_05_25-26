using System.Collections.Generic;
using UnityEngine;

public interface ISpellValidator
{
    bool HasValidSpell(GridData gridData, List<Spell> spells);
}

public class SpellValidator : ISpellValidator
{
    private static readonly Vector2Int[] Directions = 
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    public bool HasValidSpell(GridData gridData, List<Spell> spells)
    {
        if (spells == null || spells.Count == 0)
            return true;

        int size = gridData.GridSize;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Vector2Int startPos = new Vector2Int(x, y);
                if (FindSpellFromPosition(gridData, spells, startPos))
                    return true;
            }
        }

        return false;
    }

    private bool FindSpellFromPosition(GridData gridData, List<Spell> spells, Vector2Int startPos)
    {
        int maxLength = GetMaxSpellLength(spells);
        List<Vector2Int> path = new List<Vector2Int> { startPos };
        List<Element> elements = new List<Element> { gridData.GetElementAt(startPos) };

        return SearchPath(gridData, spells, path, elements, maxLength);
    }

    private bool SearchPath(GridData gridData, List<Spell> spells, List<Vector2Int> path, List<Element> elements, int maxLength)
    {
        if (MatchesAnySpell(elements, spells))
            return true;

        if (path.Count >= maxLength)
            return false;

        Vector2Int current = path[path.Count - 1];

        foreach (Vector2Int dir in Directions)
        {
            Vector2Int next = current + dir;

            if (!gridData.IsPositionValid(next) || path.Contains(next))
                continue;

            Element element = gridData.GetElementAt(next);
            if (element == null)
                continue;

            path.Add(next);
            elements.Add(element);

            if (SearchPath(gridData, spells, path, elements, maxLength))
                return true;

            path.RemoveAt(path.Count - 1);
            elements.RemoveAt(elements.Count - 1);
        }

        return false;
    }

    private bool MatchesAnySpell(List<Element> elements, List<Spell> spells)
    {
        foreach (Spell spell in spells)
        {
            if (spell.Matches(elements))
                return true;
        }
        return false;
    }

    private int GetMaxSpellLength(List<Spell> spells)
    {
        int max = 0;
        foreach (Spell spell in spells)
        {
            if (spell.Recipe != null && spell.Recipe.Count > max)
                max = spell.Recipe.Count;
        }
        return max;
    }
}