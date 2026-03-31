public static class ItemEffectFactory //static so we can call it anywhere
{
    public static IItemEffect Create(
        ItemEffectType effectType,
        ExtraDamageEffect extraDamageEffect,
        FireRingEffect fireRingEffect,
        LightningRingEffect lightningRingEffect,
        MoonRingEffect moonRingEffect,
        ShamrockRingEffect shamrockRingEffect,
        FireInstanceEffect fireInstanceEffect,
        MoonInstanceEffect moonInstanceEffect,
        LightningInstanceEffect lightningInstanceEffect,
        ShamrockInstanceEffect shamrockInstanceEffect,
        FireObliterateEffect fireObliterateEffect)
    {
        switch (effectType)
        {
            case ItemEffectType.ExtraDamage:        return extraDamageEffect;
            case ItemEffectType.FireRing:            return fireRingEffect;
            case ItemEffectType.LightningRing:       return lightningRingEffect;
            case ItemEffectType.MoonRing:            return moonRingEffect;
            case ItemEffectType.ShamrockRing:        return shamrockRingEffect;
            case ItemEffectType.FireInstance:        return fireInstanceEffect;
            case ItemEffectType.MoonInstance:        return moonInstanceEffect;
            case ItemEffectType.LightningInstance:   return lightningInstanceEffect;
            case ItemEffectType.ShamrockInstance:    return shamrockInstanceEffect;
            case ItemEffectType.FireObliterate:      return fireObliterateEffect;
            default:                                 return null;
        }
    }
}