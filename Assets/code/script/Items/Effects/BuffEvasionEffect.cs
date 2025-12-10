using UnityEngine;

[System.Serializable]
public class BuffEvasionEffect : IItemEffect
{
    [SerializeField]
    private int amount = 3;
    public void Equip(PlayerStats target)
    {
        target.ModifyEvasion(amount);
    }

    public void UnEquip(PlayerStats target)
    {
        target.ModifyEvasion(-amount);
    }
}
