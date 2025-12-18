using UnityEngine;

public enum ElementType
{
    Red = 0,
    Blue = 1,
    Green = 2,
    Yellow = 3,
    EMPTY
}

[System.Serializable]
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

[CreateAssetMenu(fileName = "Element", menuName = "ScriptableObjects/Element", order = 1)]
public class Element : ScriptableObject
{
    public string Name;
    public ElementType Type;
    public Sprite Sprite;
    public Color Color;
}