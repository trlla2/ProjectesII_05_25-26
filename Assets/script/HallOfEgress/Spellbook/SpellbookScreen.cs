using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Settings;
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

    public UnityEvent OnCloseButtonPressed;

    [SerializeField] private string spellbookErrorsTableName = "Spellbook-ErrorTexts";

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
                if(GameManager.Instance.PlayerStats.ExistSpellOnSpellBook(spell))
                {
                    //SendErrorMessage("You cant have repeated spells on your spellbook.");
                    string key = "spellsRepeated";
                    SendErrorMessage(LocalizationSettings.StringDatabase.GetLocalizedString(spellbookErrorsTableName, key));
                    return;
                }

                _spellSlotSelected?.SetNewSpell(spell);
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

    private void SendErrorMessage(string m)
    {
        if(errorText.text == string.Empty)
            errorTextTranslator.ToTarget();

        errorText.text = m;
    }

    public void CloseButton()
    {

        bool thereIsBlue = false;
        bool thereIsGreen = false;
        bool thereIsRed = false;
        bool thereIsYellow = false;

        foreach (Spell s in GameManager.Instance.PlayerStats.SpellBook)
        {
            if (s == null)
            {
                //SendErrorMessage("You must have 4 spells.");
                string key = "4Spells";
                SendErrorMessage(LocalizationSettings.StringDatabase.GetLocalizedString(spellbookErrorsTableName, key));
                return;
            }

            foreach(Element e in s.Recipe)
            {
                switch (e.Type)
                {
                    case ElementType.Red:
                        thereIsRed = true;
                        break;
                    case ElementType.Blue:
                        thereIsBlue = true;
                        break;
                    case ElementType.Green:
                        thereIsGreen = true;
                        break;
                    case ElementType.Yellow:
                        thereIsYellow = true;
                        break;
                }
            }
        }



        if(thereIsRed && thereIsBlue && thereIsGreen && thereIsYellow)
            OnCloseButtonPressed.Invoke();
        else
        {
            string key = "allElements";
            SendErrorMessage(LocalizationSettings.StringDatabase.GetLocalizedString(spellbookErrorsTableName, key));

        }
            
    }
}
