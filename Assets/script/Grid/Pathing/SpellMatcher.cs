using System.Collections.Generic;
public interface ISpellMatcher
{
    Spell FindMatchingSpell(List<Element> pathElements, List<Spell> availableSpells);
}

public class SpellMatcher : ISpellMatcher
{
    public Spell FindMatchingSpell(List<Element> pathElements, List<Spell> availableSpells)
    {
        if (pathElements == null || pathElements.Count == 0)
            return null;

        if (availableSpells == null || availableSpells.Count == 0)
            return null;

        foreach (Spell spell in availableSpells)
        {
            if (spell.Matches(pathElements))
            {
                return spell;
            }
        }

        return null;
    }
}