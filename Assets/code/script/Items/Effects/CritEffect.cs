using System.Collections.Generic;
using UnityEngine;

public class CritEffect : IItemEffect
{
    [SerializeField]
    private Dictionary<ItemLevel, int> amount = new Dictionary<ItemLevel, int>();
    public void Equip(PlayerStats target, ItemLevel level)
    {
        target.ModifyCritPercentage(amount[level]);
    }

    public void UnEquip(PlayerStats target, ItemLevel level)
    {
        target.ModifyCritPercentage(-amount[level]);
    }
}
