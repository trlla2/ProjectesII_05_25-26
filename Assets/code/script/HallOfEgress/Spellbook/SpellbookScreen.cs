using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.InputSystem.OnScreen.OnScreenStick;

public class SpellbookScreen : MonoBehaviour
{
    [Header("Content")]
    [SerializeField] private List<Spell> _knownSpells;

    [SerializeField] private Button _spellButtonPrefab;

    [SerializeField] private GameObject _scrollviewContent;

    [Header("Slots")]
    [SerializeField] 
    private GameObject _spellSlotPrefab;
    private SpellSlotBehaviour _spellSlotSelected;

    private int _maxSpells = 4;

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
                //SET SPELL TO THE SLOT

            });
        }
    }
    
    public void DeleteSpellsButtonList()
    {
        Button[] spellsButtons = _scrollviewContent.GetComponentsInChildren<Button>();
        foreach (Button button in spellsButtons)
            Destroy(button.gameObject);
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
