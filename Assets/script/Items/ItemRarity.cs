using UnityEngine;

public enum ItemRarity
{
    COMMON = 0,
    RARE = 1,
    EPIC = 2,
    LEGENDARY = 3
}

public static class ItemRarityExtensions
{
    public static int GetShopPrice(this ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.COMMON: return 20;
            case ItemRarity.RARE: return 50;
            case ItemRarity.EPIC: return 100;
            case ItemRarity.LEGENDARY: return 200;
            default: return 20;
        }
    }
}

public enum ItemLevel
{
    LVL1 = 0,
    LVL2 = 1,
    LVL3 = 2,
    LVL4 = 3
}

public enum ItemActivationType
{
    Passive,
    Immediate,
    GridTarget
}

public enum ItemEffectType
{
    None,
    ExtraDamage,
    FireRing,
    LightningRing,
    MoonRing,
    ShamrockRing,
    FireInstance,
    MoonInstance,
    LightningInstance,
    ShamrockInstance,
    FireObliterate,
    LightningObliterate,
    ShamrockObliterate,
    MoonObliterate,
    OrangeInstance,
    MelonInstance,
    OrangePile,
    MelonPile
}