using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

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

        //spellName.text = spell.Name;
        //description.text = spell.Description;
        spellName.text = LocalizationSettings.StringDatabase.GetLocalizedString("SpellNames", spell.Name);
        description.text = LocalizationSettings.StringDatabase.GetLocalizedString("SpellDescriptions", spell.Name);
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
        PlayerStats stats = GameManager.Instance.PlayerStats;
        int emptySlot = -1;

        for (int i = 0; i < stats.SpellBookSize; i++)
        {
            if (stats.SpellBook[i] == null)
            {
                emptySlot = i;
                break;
            }
        }

        if (emptySlot >= 0)
        {
            stats.AddSpellToSpellBook(spellTile.Spell, emptySlot);
        }
        else
        {
            Debug.LogWarning("Spell book is full! Cannot add new spell.");
        }

        spellTile.Replace();
        spellTile = null;
        translatorUI.ToOrigin();
        Camera.main.GetComponent<TileClickHandler>().HandleBoardRaycast(true);
    }

    public void Show(NewSpellTile tile) 
    { 
        spellTile = tile;
        SetupSpellUI(tile.Spell);
        translatorUI.ToTarget();
    }

}
