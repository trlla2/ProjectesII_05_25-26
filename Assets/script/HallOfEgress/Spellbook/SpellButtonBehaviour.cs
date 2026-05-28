using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Localization.Settings;

public class SpellButtonBehaviour : MonoBehaviour
{
    [SerializeField] private TMP_Text _spellname;
    [SerializeField] private TMP_Text _spellDescription;
    [SerializeField] private GameObject recipieParent;
    [SerializeField] private GameObject ElementPrefab;
    [SerializeField] private GameObject warning;

    [SerializeField] private string spellNamesTableName;
    [SerializeField] private string spellDescriptionTableName;

    public void SetButtonToSpellValues(Spell spell)
    {
        _spellname.text = LocalizationSettings.StringDatabase.GetLocalizedString(spellNamesTableName, spell.Name);
        _spellDescription.text = LocalizationSettings.StringDatabase.GetLocalizedString(spellDescriptionTableName, spell.Name);
        for (int i = 0; i < spell.Recipe.Count; i++)
        {
            GameObject temp = Instantiate(ElementPrefab, recipieParent.transform);
            temp.GetComponent<UIElement_Behaviour>().SetupElementUI(spell.Recipe[i]);
        }

        if(!SaveManager.Instance.HasUsedSpell(spell))
        {
            warning.SetActive(true);

            this.GetComponent<Button>().onClick.AddListener(() => {
                SaveManager.Instance.AddUsedSpell(spell);
                warning.SetActive(false);
            });
        }
        else
        {
            warning.SetActive(false);
        }
    }
}
