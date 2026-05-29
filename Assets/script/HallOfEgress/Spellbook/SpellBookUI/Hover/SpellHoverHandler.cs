using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

[RequireComponent(typeof(TranslatorUI))]
public class SpellHoverHandler : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField]
    private TMP_Text spellName;
    [SerializeField]
    private TMP_Text description;
    [SerializeField]
    private string spellNamesTableName = "SpellNames";
    [SerializeField]
    private string spellDescriptionTableName = "SpellDescriptions";

    [Header("Recipie")]
    [SerializeField]
    private Transform recipeParent;
    [SerializeField]
    private GameObject element;

    private TranslatorUI translatorUI;

    private void Awake() { translatorUI = GetComponent<TranslatorUI>(); }

    private void Start()
    {
        GameManager.Instance.CombatHandler.SetHoverHandlerRef(this);
    }
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
        spellName.text = LocalizationSettings.StringDatabase.GetLocalizedString(spellNamesTableName, spell.Name);
        description.text = LocalizationSettings.StringDatabase.GetLocalizedString(spellDescriptionTableName, spell.Name);
    }

    public void ClearRecipeDisplay()
    {
        foreach (Transform child in recipeParent)
        {
            Destroy(child.gameObject);
        }
    }

    public void Show() { translatorUI.ToTarget(); }
    public void UnShow() { translatorUI.ToOrigin(); }

    private void OnDestroy()
    {
        if (GameManager.Instance != null && GameManager.Instance.CombatHandler != null)
            GameManager.Instance.CombatHandler.DeleteEnemyRef();
    }
}
