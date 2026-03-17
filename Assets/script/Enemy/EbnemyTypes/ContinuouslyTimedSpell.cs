using UnityEngine;

public class ContinuouslyTimedSpell : MultipleTimedSpellBase
{
    public ContinuouslyTimedSpell(
        float selectionInterval = 2.5f,
        float timer = 8f,
        float damage = 4f,
        float bonusTime = 2f)
        : base(selectionInterval, timer, damage, bonusTime)
    {
        spellName = "Continuously Timed Target";
    }

    protected override Vector2Int PickTargetPosition(Grid grid)
    {
        return PickRandomValidPosition(grid);
    }
}
