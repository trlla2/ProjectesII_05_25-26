using UnityEngine;

public enum ElementType
{
    Red,
    Blue,
    Green,
    Yellow,
    EMPTY
}

[CreateAssetMenu(fileName = "Element", menuName = "ScriptableObjects/Element", order = 1)]
public class Element : ScriptableObject
{
    public string Name;
    public ElementType Type;
    public Sprite Sprite;
    public Color Color;
}