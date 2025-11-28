using TMPro;
using UnityEngine;

public class UISpell_Behaviour : MonoBehaviour
{
    
    [Header("Recipe")]
    [SerializeField]
    private Transform recipeParent;
    [SerializeField]
    private GameObject element;
    public void SetupSpellUI(Spell spell)
    {
        foreach (Element e in spell.Recipe)
        {
            GameObject o = Instantiate(element, recipeParent);
            o.GetComponent<UIElement_Behaviour>().SetupElementUI(e);
        }
    }
}
