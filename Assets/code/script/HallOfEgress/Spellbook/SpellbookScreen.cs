using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.InputSystem.OnScreen.OnScreenStick;

public class SpellbookScreen : MonoBehaviour
{
    [SerializeField] private List<Spell> _knownSpells;

    [SerializeField] private Button _spellButtonPrefab;

    [SerializeField] private GameObject _scrollviewContent;

    private int _maxSpells = 4;
    [SerializeField] private TMP_Text _spellCountText;

    private TranslatorUI translator;
    private void Awake() { translator = GetComponent<TranslatorUI>(); }
    public void OpenSpellBook()
    { 
        translator.ToTarget(); 
        
    }

    public void CreateSpellsButtons()
    {
        foreach (Spell spell in _knownSpells)
        {
            Button newButton = Instantiate(_spellButtonPrefab, _scrollviewContent.transform);
            newButton.GetComponent<SpellButtonBehaviour>().SetButtonToSpellValues(spell);

            newButton.onClick.AddListener(() =>
            {
                UpdateSpellCount();
            });
        }

        UpdateSpellCount();
    }
    private void UpdateSpellCount()
    {
        int count = 0;
        SpellButtonBehaviour[] spellsButtons = _scrollviewContent.GetComponentsInChildren<SpellButtonBehaviour>();

        foreach (SpellButtonBehaviour button in spellsButtons)
        {
            if (button.IsSelected())
                count++;
        }

        _spellCountText.text = count.ToString() + " out of " + _maxSpells + " Spells";
    }

    public void DeleteSpellsButtonList()
    {
        Button[] spellsButtons = _scrollviewContent.GetComponentsInChildren<Button>();
        foreach (Button button in spellsButtons)
            Destroy(button.gameObject);

        _spellCountText.text = "0" + " out of " + _maxSpells + " Spells";
    }

    public void SetSelectedSpells()
    {
        GameManager.Instance.GetPlayerSpellBook().Clear();

        SpellButtonBehaviour[] spellsButtons = _scrollviewContent.GetComponentsInChildren<SpellButtonBehaviour>();
        foreach (Spell spell in _knownSpells)
        {
            foreach(SpellButtonBehaviour button in spellsButtons)
            {
                if (spell.name == button.GetSpellName() && button.IsSelected())
                    GameManager.Instance.AddSpellToPlayerSpellBook(spell);
            }
        }
    }
}
