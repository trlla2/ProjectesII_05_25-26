using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpikyEffect : IItemEffect, ISerializationCallbackReceiver
{
    private Dictionary<ItemLevel, int> amounts = new Dictionary<ItemLevel, int>();

    [SerializeField]
    private List<ItemLevelData<int>> inspectorAmount = new List<ItemLevelData<int>>();
    public void Equip(PlayerStats target, ItemLevel level)
    {
        target.ModifySipkeDamage(amounts[level]);
    }

    public void UnEquip(PlayerStats target, ItemLevel level)
    {
        target.ModifySipkeDamage(-amounts[level]);
    }

    public void OnBeforeSerialize() { }

    public void OnAfterDeserialize()
    {
        amounts = new Dictionary<ItemLevel, int>();

        foreach (var a in inspectorAmount)
        {
            if (!amounts.ContainsKey(a.level))
            {
                amounts[a.level] = a.amount;
            }
        }
    }
}
