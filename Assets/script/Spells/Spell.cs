using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "Spell", menuName = "ScriptableObjects/Spells", order = 2)]
public class Spell : ScriptableObject, IDropeable
{
    public int id;
    public string Name;
    public string Description;
    public List<Element> Recipe;
    public UnityEvent OnCast;

    public string RewardName => Name;
    public RewardType Type => RewardType.SPELL;

    public Sprite Icon => null;
    public Sprite[] sprites
    {
        get
        {
            Sprite[] elementSprites = new Sprite[Recipe.Count];
            for (int i = 0; i < Recipe.Count; i++)
            {
                elementSprites[i] = Recipe[i].Sprite;
            }
            return elementSprites;
        }
    }
    public Color[] Colors
    {
        get { 
            Color[] elementColors = new Color[Recipe.Count];
            for (int i = 0; i < Recipe.Count; i++)
            {
                elementColors[i] = Recipe[i].Color;
            }
            return elementColors;
        }
    }



    public void ClaimReward()
    {
        GameManager.Instance.PlayerStats.AddKnownSpell(this);
    }

    private static bool TypeMatches(ElementType pathType, ElementType recipeType)
    {
        if (pathType == recipeType) return true;
        if (pathType == ElementType.Orange && (recipeType == ElementType.Red || recipeType == ElementType.Yellow))
            return true;
        if (pathType == ElementType.Melon && (recipeType == ElementType.Blue || recipeType == ElementType.Green))
            return true;
        return false;
    }

    public bool Matches(List<Element> path)
    {
        if (path == null) { Debug.LogWarning("Be careful! tried to match an empty path"); return false; }

        if (path.Count != Recipe.Count) return false;

        bool forwardMatch = true;
        for (int i = 0; i < Recipe.Count; i++)
        {
            if (path[i] == null || !TypeMatches(path[i].Type, Recipe[i].Type))
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
            if (path[i] == null || !TypeMatches(path[i].Type, Recipe[Recipe.Count - 1 - i].Type))
            {
                backwardMatch = false;
                break;
            }
        }

        return backwardMatch;
    }
}