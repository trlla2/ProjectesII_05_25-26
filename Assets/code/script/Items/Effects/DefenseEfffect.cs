using System.Collections.Generic;
using UnityEngine;

public class DefenseEfffect : IItemEffect
{
    [SerializeField]
    private Dictionary<ItemLevel, float> amount = new Dictionary<ItemLevel, float>();
    public void Equip(PlayerStats target, ItemLevel level)
    {
        target.ModifyDefense(amount[level]);
    }

    public void UnEquip(PlayerStats target, ItemLevel level)
    {
        target.ModifyDefense(-amount[level]);
    }
}
