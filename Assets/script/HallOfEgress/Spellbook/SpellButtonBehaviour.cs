using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class SpellButtonBehaviour : MonoBehaviour
{
    [SerializeField] private TMP_Text _spellname;
    [SerializeField] private TMP_Text _spellDescription;
    [SerializeField] private GameObject recipieParent;
    [SerializeField] private GameObject ElementPrefab;
   
    public void SetButtonToSpellValues(Spell spell)
    {
        _spellname.text = spell.Name;
        _spellDescription.text = spell.Description;
        for (int i = 0; i < spell.Recipe.Count; i++)
        {
            GameObject temp = Instantiate(ElementPrefab, recipieParent.transform);
            temp.GetComponent<Image>().sprite = spell.Recipe[i].Sprite;
        }
    }
}
