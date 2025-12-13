using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(EventTrigger))]
public class UISpell_Behaviour : MonoBehaviour
{
    
    [Header("Recipe")]
    [SerializeField]
    private Transform recipeParent;
    [SerializeField]
    private GameObject element;

    Spell currentSpell;
    private EventTrigger trigger;

    public void SetupSpellUI(Spell spell)
    {
        currentSpell = spell;

        foreach (Element e in spell.Recipe)
        {
            GameObject o = Instantiate(element, recipeParent);
            o.GetComponent<UIElement_Behaviour>().SetupElementUI(e);
        }
    }

    public void HoverSpell() { GameManager.Instance.HoverSpell(currentSpell); }
    public void UnHoverSpell() { GameManager.Instance.UnHoverSpell(); }
}
