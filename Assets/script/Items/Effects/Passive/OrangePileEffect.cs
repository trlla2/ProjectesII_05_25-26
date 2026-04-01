using UnityEngine;

[System.Serializable]
public class OrangePileEffect : IItemEffect
{
    [SerializeField] private float orangeSpawnWeight = 0.6f;

    public void Equip(PlayerStats target, ItemLevel level)
    {
        GridSpawnRegistry.SetWeightOverride(ElementType.Orange, orangeSpawnWeight);
    }

    public void UnEquip(PlayerStats target, ItemLevel level)
    {
        GridSpawnRegistry.RemoveWeightOverride(ElementType.Orange);
    }
}