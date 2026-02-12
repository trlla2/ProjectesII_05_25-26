using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class DefenseEfffect : IItemEffect, ISerializationCallbackReceiver
{
    private Dictionary<ItemLevel, float> amounts = new Dictionary<ItemLevel, float>();

    [SerializeField]
    private List<ItemLevelData<float>> inspectorAmount = new List<ItemLevelData<float>>();

    public void Equip(PlayerStats target, ItemLevel level)
    {
        target.ModifyDefense(amounts[level]);
    }

    public void UnEquip(PlayerStats target, ItemLevel level)
    {
        target.ModifyDefense(-amounts[level]);
    }

    public void OnBeforeSerialize() { }

    public void OnAfterDeserialize()
    {
        amounts = new Dictionary<ItemLevel, float>();

        foreach (var a in inspectorAmount)
        {
            if (!amounts.ContainsKey(a.level))
            {
                amounts[a.level] = a.amount;
            }
        }
    }
}
