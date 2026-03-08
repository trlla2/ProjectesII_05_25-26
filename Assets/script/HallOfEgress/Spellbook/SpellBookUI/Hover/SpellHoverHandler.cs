using TMPro;
using UnityEngine;

[RequireComponent(typeof(TranslatorUI))]
public class SpellHoverHandler : MonoBehaviour
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

    public void Show() { translatorUI.ToTarget(); }
    public void UnShow() { translatorUI.ToOrigin(); }

    private void OnDestroy()
    {
        GameManager.Instance.CombatHandler.DeleteEnemyRef();
    }
}
