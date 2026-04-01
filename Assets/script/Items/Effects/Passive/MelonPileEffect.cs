using UnityEngine;

[System.Serializable]
public class MelonPileEffect : IItemEffect
{
    [SerializeField] private float melonSpawnWeight = 0.6f;

    public void Equip(PlayerStats target, ItemLevel level)
    {
        GridSpawnRegistry.SetWeightOverride(ElementType.Melon, melonSpawnWeight);
    }

    public void UnEquip(PlayerStats target, ItemLevel level)
    {
        GridSpawnRegistry.RemoveWeightOverride(ElementType.Melon);
    }
}