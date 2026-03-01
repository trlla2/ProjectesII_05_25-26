using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpellbookScreen : MonoBehaviour
{
    [Header("Content")]
    [SerializeField] private Button _spellButtonPrefab;

    [SerializeField] private GameObject _scrollviewContent;

    [SerializeField]
    private TMP_Text errorText;
    [SerializeField]
    private TranslatorUI errorTextTranslator;

    [Header("Slots")]
    [SerializeField]
    private GameObject _slotsParent;
    [SerializeField] 
    private GameObject _spellSlotPrefab;

    private SpellSlotBehaviour _spellSlotSelected;


    private TranslatorUI translator;
    private void Awake() 
    { 
        translator = GetComponent<TranslatorUI>();
        errorText.text = string.Empty;
    }
    public void OpenSpellBook()
    { 
        translator.ToTarget(); 
        
    }

    public void CreateSpellsButtons()
    {
        foreach (Spell spell in GameManager.Instance.PlayerStats.KnownSpells)
        {
            Button newButton = Instantiate(_spellButtonPrefab, _scrollviewContent.transform);
            newButton.GetComponent<SpellButtonBehaviour>().SetButtonToSpellValues(spell);

            newButton.onClick.AddListener(() =>
            {
                _spellSlotSelected?.SetNewSpell(spell);
                if(_spellSlotSelected.CurrentSpell == null)
                {
                    errorText.text = "You cant have repeated spells on your spellbook.";
                    errorTextTranslator.ToTarget();
                    return;
                }

                _spellSlotSelected?.UnSelect();

                if (_spellSlotSelected?.NextSlot != null)
                {
                    _spellSlotSelected = _spellSlotSelected.NextSlot;
                }

                _spellSlotSelected?.Select();
            });
        }

        int bookSize = GameManager.Instance.PlayerStats.SpellBookSize;
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
