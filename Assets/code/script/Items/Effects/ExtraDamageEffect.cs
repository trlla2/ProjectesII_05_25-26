using System.Collections.Generic;
using UnityEngine;

public class ExtraDamageEffect : IItemEffect
{
    [SerializeField]
    private Dictionary<ItemLevel, int> amount = new Dictionary<ItemLevel, int>();
    public void Equip(PlayerStats target, ItemLevel level)
    {
        target.ModifyExtraDamage(amount[level]);
    }

    public void UnEquip(PlayerStats target, ItemLevel level)
    {
        target.ModifyExtraDamage(-amount[level]);
    }
}
