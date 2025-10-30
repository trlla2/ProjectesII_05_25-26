using UnityEngine;

public enum ElementType
{
    Fire = 0,
    Water = 1,
    Plant = 2,
    Energy = 3
}

public struct DamageType
{
    public ElementType element;
    public float damage;

    public DamageType(ElementType element, float damage)
    {
        this.element = element;
        this.damage = damage;
    }
}