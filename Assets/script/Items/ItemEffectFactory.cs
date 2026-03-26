public static class ItemEffectFactory //static so we can call it anywhere
{
    public static IItemEffect Create(
        ItemEffectType effectType,
        ExtraDamageEffect extraDamageEffect,
        FireRingEffect fireRingEffect,
        LightningRingEffect lightningRingEffect,
        MoonRingEffect moonRingEffect,
        ShamrockRingEffect shamrockRingEffect)
    {
        switch (effectType)
        {
            case ItemEffectType.ExtraDamage: return extraDamageEffect;
            case ItemEffectType.FireRing:    return fireRingEffect;
            case ItemEffectType.LightningRing:    return lightningRingEffect;
            case ItemEffectType.MoonRing:    return moonRingEffect;
            case ItemEffectType.ShamrockRing:    return shamrockRingEffect;
            default:                         return null;
        }
    }
}