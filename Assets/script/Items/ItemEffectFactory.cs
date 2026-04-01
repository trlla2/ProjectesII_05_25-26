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
        OrangeInstanceEffect orangeInstanceEffect,
        MelonInstanceEffect melonInstanceEffect,
        OrangePileEffect orangePileEffect,
        MelonPileEffect melonPileEffect,
        FireObliterateEffect fireObliterateEffect,
        LightningObliterateEffect lightningObliterateEffect,
        ShamrockObliterateEffect shamrockObliterateEffect,
        MoonObliterateEffect moonObliterateEffect)
    {
        switch (effectType)
        {
            case ItemEffectType.ExtraDamage:            return extraDamageEffect;
            case ItemEffectType.FireRing:               return fireRingEffect;
            case ItemEffectType.LightningRing:          return lightningRingEffect;
            case ItemEffectType.MoonRing:               return moonRingEffect;
            case ItemEffectType.ShamrockRing:           return shamrockRingEffect;
            case ItemEffectType.FireInstance:           return fireInstanceEffect;
            case ItemEffectType.MoonInstance:           return moonInstanceEffect;
            case ItemEffectType.LightningInstance:      return lightningInstanceEffect;
            case ItemEffectType.ShamrockInstance:       return shamrockInstanceEffect;
            case ItemEffectType.OrangeInstance:         return orangeInstanceEffect;
            case ItemEffectType.MelonInstance:          return melonInstanceEffect;
            case ItemEffectType.OrangePile:             return orangePileEffect;
            case ItemEffectType.MelonPile:              return melonPileEffect;
            case ItemEffectType.FireObliterate:         return fireObliterateEffect;
            case ItemEffectType.LightningObliterate:    return lightningObliterateEffect;
            case ItemEffectType.ShamrockObliterate:     return shamrockObliterateEffect;
            case ItemEffectType.MoonObliterate:         return moonObliterateEffect;
            default:                                    return null;
        }
    }
}