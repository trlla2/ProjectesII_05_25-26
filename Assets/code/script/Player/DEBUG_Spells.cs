using System.Collections.Generic;
using UnityEngine;

public class DEBUG_Spells : MonoBehaviour
{
    [Header("DEBUG ONLY (ONLY PUT 4 SPELLS)")]
    [SerializeField]
    private List<Spell> spellList = new List<Spell>();

    private void Start()
    {
        foreach (Spell spell in spellList)
            GameManager.Instance.AddSpellToPlayerSpellBook(spell);
    }
}
