using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Tutorial/Grid Layout")]
public class TutorialGridLayout : ScriptableObject
{
    [Header("4x4 bottom to top")]
    public List<ElementRow> rows = new List<ElementRow>(4);

    public int GridSize = 4;

    [Header("Applied in order after each refill")]
    public List<TutorialRefillOverride> refillOverrides;

    public Element GetElement(int x, int y)
    {
        if (y < 0 || y >= rows.Count) return null;
        if (x < 0 || x >= rows[y].elements.Count) return null;
        return rows[y].elements[x];
    }
}

[System.Serializable]
public class ElementRow
{
    public List<Element> elements = new List<Element>(4);
}

[System.Serializable]
public class TutorialRefillOverride
{
    public List<ElementRow> rows = new List<ElementRow>(4);
}