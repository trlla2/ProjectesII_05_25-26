using UnityEngine;

public enum ItemRarity
{
    COMMON = 0,
    RARE = 1,
    EPIC = 2,
    LEGENDARY = 3
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
}