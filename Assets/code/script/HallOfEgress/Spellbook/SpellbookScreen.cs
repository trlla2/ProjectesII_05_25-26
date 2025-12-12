using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class SpellbookScreen : MonoBehaviour
{
    [SerializeField] private List<Spell> _knownSpells;

    [SerializeField] private Button _spellButtonPrefab;

    [SerializeField] private GameObject _scrollviewContent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void CreateSpellsButtons()
    {
        //if (_scrollviewContent.GetComponentsInChildren<Button>() != null)
        //    foreach (Button spellButton in _scrollviewContent.GetComponentsInChildren<Button>())
        //        Destroy(spellButton);

        foreach (Spell spell in _knownSpells)
        {
            //Button newButton = Instantiate(_spellButtonPrefab, nextButtonPos, _scrollviewContent.transform.rotation, _scrollviewContent.transform);
            Button newButton = Instantiate(_spellButtonPrefab, _scrollviewContent.transform);
            newButton.GetComponent<SpellButtonBehaviour>().SetButtonToSpellValues(spell);
            //newButton.transform.position -= new Vector3(0, 1.5f * index, 0);
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
