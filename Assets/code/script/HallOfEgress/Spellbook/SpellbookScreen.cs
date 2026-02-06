using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
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
    private GameObject _slotsParent;
    [SerializeField] 
    private GameObject _spellSlotPrefab;

    private SpellSlotBehaviour _spellSlotSelected;

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
                _spellSlotSelected?.SetNewSpell(spell);
            });
        }

        for (int i = 0; i < GameManager.Instance.PlayerSpellBookSize; i++)
        {
            GameObject temp = Instantiate(_spellSlotPrefab, _slotsParent.transform);
            SpellSlotBehaviour slot = temp.GetComponent<SpellSlotBehaviour>();
            
            slot.Initialize(i);

            if (i == 0)// select the first slot by default
            { 
                _spellSlotSelected = slot;
                slot.Select(); 

            }


            temp.GetComponent<Button>().onClick.AddListener(() =>
            {
                _spellSlotSelected?.UnSelect();

                _spellSlotSelected = slot;  

                _spellSlotSelected?.Select();
            });

        }
    }
    
    public void DeleteSpellsButtonListAndSlotsList()
    {
        Button[] spellsButtons = _scrollviewContent.GetComponentsInChildren<Button>();
        foreach (Button button in spellsButtons)
            Destroy(button.gameObject);

        SpellSlotBehaviour[] spellSlots = _slotsParent.GetComponentsInChildren<SpellSlotBehaviour>();
        foreach (SpellSlotBehaviour slot in spellSlots)
            Destroy(slot.gameObject);
    }
}
