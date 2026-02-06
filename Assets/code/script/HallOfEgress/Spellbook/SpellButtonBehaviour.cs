using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class SpellButtonBehaviour : MonoBehaviour
{
    [SerializeField] TMP_Text _spellname;
    [SerializeField] List<Image> _elementsImages;
    [SerializeField] TMP_Text _spellDescription;
   
    public void SetButtonToSpellValues(Spell spell)
    {
        _spellname.text = spell.Name;
        _spellDescription.text = spell.Description;
        for (int i = 0; i < spell.Recipe.Count; i++)
        {
            _elementsImages[i].sprite = spell.Recipe[i].Sprite;
        }
    }
}
