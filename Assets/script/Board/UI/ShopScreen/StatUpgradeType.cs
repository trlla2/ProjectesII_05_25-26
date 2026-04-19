using UnityEngine;

public enum StatUpgradeType
{
    ExtraDamage,
    MaxHealth,
    Evasion,
    Defense
}

[System.Serializable]
public struct StatUpgradeData
{
    public StatUpgradeType Type;
    public Sprite Icon;
    //public int Cost;
}