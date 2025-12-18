using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "Spell", menuName = "ScriptableObjects/Spells", order = 2)]
public class Spell : ScriptableObject
{
    public string Name;
    public string Description;
    public List<Element> Recipe;
    public UnityEvent OnCast;

    public bool Matches(List<Element> path)
    {
        if (path == null) { Debug.LogWarning("Be careful! tried to match an empty path"); return false; }

        if (path.Count != Recipe.Count) return false;

        bool forwardMatch = true;
        for (int i = 0; i < Recipe.Count; i++)
        {
            if (path[i] == null || path[i].Type != Recipe[i].Type)
            {
                forwardMatch = false;
                break;
            }
        }

        if (forwardMatch)
        {
            return true;
        }

        bool backwardMatch = true;
        for (int i = 0; i < Recipe.Count; i++)
        {
            if (path[i] == null || path[i].Type != Recipe[Recipe.Count - 1 - i].Type)
            {
                backwardMatch = false;
                break;
            }
        }
        
        return backwardMatch;
    }
}