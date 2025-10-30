using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Spell", menuName = "ScriptableObjects/Spells", order = 2)]
public class Spell : ScriptableObject
{
    public string Name;
    public List<Element> Recipe;

    public bool Matches(List<Element> path)
    {
        if (path == null) {Debug.LogWarning("Be carful! tried to match an empty path"); return false; }

        if (path.Count != Recipe.Count) return false;

        for (int i = 0; i < Recipe.Count; i++)
        {
            if (path[i] == null) return false;

            if (path[i].Type != Recipe[i].Type) return false;
        }

        return true;
    }
}