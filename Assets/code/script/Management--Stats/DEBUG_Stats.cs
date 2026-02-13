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

    private void Start()
    {
        if (spells.Count > 0)
        {
            for (int i = 0; i < GameManager.Instance.PlayerSpellBookSize; i++)
            {
                if (spells[i] == null)
                    break;

                GameManager.Instance.AddSpellToPlayerSpellBook(spells[i], i);
            }
        }


        foreach (Item item in items)
            GameManager.Instance.AddItemToPlayerInventory(item);

        Debug.Log(GameManager.Instance.GetPlayerInventory().Count);
    }
}
