using System;
using UnityEngine;

[System.Serializable]
public class FreezeEffect : ActivatableEffectBase
{
    [SerializeField] private float freezeDuration = 4f;

    public override bool CanActivate(ItemContext context)
    {
        return context.Enemy != null && context.Player != null && 
            (context.Enemy.ActiveSpell is TimedDestroySpellBase || 
             context.Enemy.ActiveSpell is BlockerSpell || 
             context.Enemy.ActiveSpell is MultipleTimedSpellBase ||
             context.Enemy.ActiveSpell is SwapperSpell);
    }

    public override void Activate(ItemLevel level, ItemContext context, Action onComplete)
    {
        context.Player.GetComponentInParent<SpellEffectHandler>().PlayEffectsForSpell("FREEZE");
        context.Enemy.FreezeForDuration(freezeDuration);
        onComplete?.Invoke();
    }
}