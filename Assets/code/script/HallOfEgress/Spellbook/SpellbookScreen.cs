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
                _spellSlotSelected?.UnSelect();

                if (_spellSlotSelected?.NextSlot != null)
                {
                    Debug.Log($"Next Slot Selected {_spellSlotSelected.NextSlot.name}");
                    _spellSlotSelected = _spellSlotSelected.NextSlot;
                }

                _spellSlotSelected?.Select();
            });
        }

        int bookSize = G.GM.PlayerStats.SpellBookSize;
        SpellSlotBehaviour[] createdSlots = new SpellSlotBehaviour[bookSize];

        for (int i = 0; i < bookSize; i++)
        {
            GameObject temp = Instantiate(_spellSlotPrefab, _slotsParent.transform);
            SpellSlotBehaviour slot = temp.GetComponent<SpellSlotBehaviour>();
            createdSlots[i] = slot;

            temp.GetComponent<Button>().onClick.AddListener(() =>
            {
                _spellSlotSelected?.UnSelect();
                _spellSlotSelected = slot;
                _spellSlotSelected?.Select();
            });
        }

        for (int i = 0; i < bookSize; i++)
        {
            SpellSlotBehaviour nextSlot = (i + 1 < bookSize) ? createdSlots[i + 1] : null;

            createdSlots[i].Initialize(i, nextSlot);

            if (i == 0) 
            {
                _spellSlotSelected = createdSlots[i];
                createdSlots[i].Select();
            }
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
