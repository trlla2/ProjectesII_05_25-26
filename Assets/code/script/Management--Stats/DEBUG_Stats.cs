using System.Collections.Generic;
using UnityEngine;

public class DEBUG_Stats : MonoBehaviour
{
    [Header("DEBUG ONLY")]
    [SerializeField]
    private List<Item> items = new List<Item>();
    [Header(" ONLY PUT 4 SPELLS")]
    [SerializeField]
    private List<Spell> spells = new List<Spell>();

    [SerializeField]
    private List<Spell> knownSpells = new List<Spell>();

    private void Awake()
    {
        if (spells.Count > 0)
        {
            for (int i = 0; i < G.GM.PlayerStats.SpellBookSize; i++)
            {
                if (spells[i] == null)
                    break;

                G.GM.PlayerStats.AddSpellToSpellBook(spells[i], i);
            }
        }


        foreach(Spell s in knownSpells)
        {
            G.GM.PlayerStats.AddKnownSpell(s);
        }

        foreach (Item item in items)
            G.GM.PlayerStats.AddItemToInventory(item);

    }
}
