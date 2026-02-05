using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class SpellButtonBehaviour : MonoBehaviour
{
    [SerializeField] TMP_Text _spellname;
    [SerializeField] List<Image> _elementsImages;
    [SerializeField] TMP_Text _spellDescription;

    private Image _self;
    private bool _isSelected = false;
    [SerializeField] private Color _selectedColor;
    private Color _defaultColor;

    private void Awake()
    {
        _self = GetComponent<Image>();
        _defaultColor = _self.color;
    }

    private void Update()
    {
        if (_isSelected)
            _self.color = _selectedColor;
        else
            _self.color = _defaultColor;

    }

    public void SetButtonToSpellValues(Spell spell)
    {
        _spellname.text = spell.Name;
        _spellDescription.text = spell.Description;
        for (int i = 0; i < spell.Recipe.Count; i++)
        {
            _elementsImages[i].sprite = spell.Recipe[i].Sprite;
        }
    }

    public bool IsSelected() { return _isSelected; }

    public void HandleSelected() { _isSelected = !_isSelected; }

    public string GetSpellName() { return this._spellname.text; }
}
