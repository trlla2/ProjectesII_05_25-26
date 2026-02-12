using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BuffMaxHealthEffect : IItemEffect
{
    [SerializeField]
    private Dictionary<ItemLevel, int> amount = new Dictionary<ItemLevel, int>();
    public void Equip(PlayerStats target, ItemLevel level)
    {
        target.ModifyExtraHealth(amount[level]);
        target.Heal(amount[level]);
    }

    public void UnEquip(PlayerStats target, ItemLevel level)
    {
        target.ModifyExtraHealth(-amount[level]);
    }
}
