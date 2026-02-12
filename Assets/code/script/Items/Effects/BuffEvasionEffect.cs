using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BuffEvasionEffect : IItemEffect
{
    [SerializeField]
    private Dictionary<ItemLevel, int> amount = new Dictionary<ItemLevel, int>();
    public void Equip(PlayerStats target, ItemLevel level)
    {
        target.ModifyEvasion(amount[level]);
    }

    public void UnEquip(PlayerStats target, ItemLevel level)
    {
        target.ModifyEvasion(-amount[level]);
    }
}
