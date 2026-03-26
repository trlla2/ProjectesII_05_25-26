public static class ItemEffectFactory //static so we can call it anywhere
{
    public static IItemEffect Create(
        ItemEffectType effectType,
        ExtraDamageEffect extraDamageEffect,
        FireRingEffect fireRingEffect)
    {
        switch (effectType)
        {
            case ItemEffectType.ExtraDamage: return extraDamageEffect;
            case ItemEffectType.FireRing:    return fireRingEffect;
            default:                         return null;
        }
    }
}