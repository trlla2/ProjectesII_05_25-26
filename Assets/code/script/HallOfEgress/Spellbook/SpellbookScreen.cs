using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellbookScreen : MonoBehaviour
{
    [Header("Content")]
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
        foreach (Spell spell in G.GM.PlayerStats.KnownSpells)
        {
            Button newButton = Instantiate(_spellButtonPrefab, _scrollviewContent.transform);
            newButton.GetComponent<SpellButtonBehaviour>().SetButtonToSpellValues(spell);

            newButton.onClick.AddListener(() =>
            {
                _spellSlotSelected?.SetNewSpell(spell);
            });
        }

        SpellSlotBehaviour previuSlot = null;

        for (int i = G.GM.PlayerStats.SpellBookSize - 1; i >= 0 ; i--)
        {
            GameObject temp = Instantiate(_spellSlotPrefab, _slotsParent.transform);
            SpellSlotBehaviour slot = temp.GetComponent<SpellSlotBehaviour>();
            slot.Initialize(i, previuSlot);
            previuSlot = slot;
            if (i == 0)// select the first slot by default
            { 
                _spellSlotSelected = slot;
                slot.Select(); 

            }


            temp.GetComponent<Button>().onClick.AddListener(() =>
            {
                _spellSlotSelected?.UnSelect();
                asdfasdf
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
