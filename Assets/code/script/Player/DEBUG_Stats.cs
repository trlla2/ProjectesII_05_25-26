using System.Collections.Generic;
using UnityEngine;

public class DEBUG_Stats : MonoBehaviour
{
    [Header("DEBUG ONLY")]
    [SerializeField]
    private List<ItemsType> items = new List<ItemsType>();
    [Header(" ONLY PUT 4 SPELLS")]
    [SerializeField]
    private List<Spell> spells = new List<Spell>();

    private void Start()
    {
        foreach (Spell spell in spells)
            GameManager.Instance.AddSpellToPlayerSpellBook(spell);

        foreach (ItemsType item in items)
            GameManager.Instance.AddItemToPlayerInventory(item);
        

    }
}
