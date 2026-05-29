using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DEBUG_Stats : MonoBehaviour
{
    [Header("DEBUG ONLY")]
    [SerializeField]
    private List<Item> items = new List<Item>();
    [SerializeField]
    private bool ForceSpells = false;
    [Header(" ONLY PUT 4 SPELLS")]
    [SerializeField]
    private List<Spell> spells = new List<Spell>();
    public List<Spell> Spells => spells;

    [SerializeField]
    private List<Spell> knownSpells = new List<Spell>();

    private void Awake()
    {
        foreach (Spell s in knownSpells)
        {
            GameManager.Instance.PlayerStats.AddKnownSpell(s);
        }



        if (spells.Count > 0 && ForceSpells && this.isActiveAndEnabled)
        {
            GameManager.Instance.PlayerStats.ClearSpellBook();

            for (int i = 0; i < GameManager.Instance.PlayerStats.SpellBookSize && i < spells.Count; i++)
            {

                if (spells[i] == null)
                    break;

                GameManager.Instance.PlayerStats.AddSpellToSpellBook(spells[i], i);

            }
        }


        foreach (Item item in items)
            GameManager.Instance.PlayerStats.AddItemToInventory(item);


    }

    public bool HasDebugSpells() => spells.Count > 0 && ForceSpells;

    public void SetDebugSpellList(List<Spell> spellList)
    {
        spells = spellList;
    }

}
