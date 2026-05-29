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
        context.Enemy.FreezeForDuration(freezeDuration);
        
        // Play the same freeze visual effects (particles, audio, and vignette) as the spell
        SpellEffectHandler effectHandler = context.Player.gameObject.GetComponent<SpellEffectHandler>();
        if (effectHandler != null)
        {
            effectHandler.PlayEffectsForSpell("FREEZE");
        }
        
        onComplete?.Invoke();
    }
}