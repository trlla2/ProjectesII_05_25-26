using UnityEngine;

public enum ElementType
{
    Red = 0,
    Blue = 1,
    Green = 2,
    Yellow = 3,
    EMPTY,
    Bomb,
    Blocked,
    Orange,
    Melon
}

[System.Serializable]
public struct DamageType
{
    public ElementType element;
    public float damage;
    public bool IsCrit;

    public DamageType(ElementType element, float damage)
    {
        this.element = element;
        this.damage = damage;
        this.IsCrit = false;
    }

    public DamageType(ElementType element, float damage, bool isCrit)
    {
        this.element = element;
        this.damage = damage;
        this.IsCrit = isCrit;
    }
}

[CreateAssetMenu(fileName = "Element", menuName = "ScriptableObjects/Element", order = 1)]
public class Element : ScriptableObject
{
    public string Name;
    public ElementType Type;
    public Sprite Sprite;
    public Color Color;
    public bool spawnableInGrid = true;

    public float spawnWeight = 1f;
}