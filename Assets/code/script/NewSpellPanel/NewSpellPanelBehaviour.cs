using TMPro;
using UnityEngine;

public class NewSpellPanelBehaviour : MonoBehaviour
{

    [Header("Setup")]
    [SerializeField]
    private TMP_Text spellName;
    [SerializeField]
    private TMP_Text description;

    [Header("Recipie")]
    [SerializeField]
    private Transform recipeParent;
    [SerializeField]
    private GameObject element;

    private NewSpellTile spellTile;

    private TranslatorUI translatorUI;



    private void Awake() { translatorUI = GetComponent<TranslatorUI>(); }

    public void SetupSpellUI(Spell spell)
    {
        ClearRecipeDisplay();

        foreach (Element e in spell.Recipe)
        {
            GameObject o = Instantiate(element, recipeParent);
            o.GetComponent<UIElement_Behaviour>().SetupElementUI(e);
        }

        spellName.text = spell.Name;
        description.text = spell.Description;
    }

    public void ClearRecipeDisplay()
    {
        foreach (Transform child in recipeParent)
        {
            Destroy(child.gameObject);
        }
    }

    public void AcceptSpell()
    {
        GameManager.Instance.AddSpellToPlayerSpellBook(spellTile.Spell);
        spellTile.Replace();
        spellTile = null;
    }

    public void Show(NewSpellTile tile) 
    { 
        spellTile = tile;
        translatorUI.ToTarget();
    }

}
