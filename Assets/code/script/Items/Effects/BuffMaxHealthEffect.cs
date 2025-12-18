using UnityEngine;

[System.Serializable]
public class BuffMaxHealthEffect : IItemEffect
{
    [SerializeField]
    private int amount = 3;
    public void Equip(PlayerStats target)
    {
        target.ModifyExtraHealth(amount);
        target.Heal(amount);
    }

    public void UnEquip(PlayerStats target)
    {
        target.ModifyExtraHealth(-amount);
    }
}
