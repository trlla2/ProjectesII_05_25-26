using UnityEngine;

public class ExtraDamageEffect : IItemEffect
{
    [SerializeField]
    private int amount = 1;
    public void Equip(PlayerStats target)
    {
        target.ModifyExtraDamage(amount);
    }

    public void UnEquip(PlayerStats target)
    {
        target.ModifyExtraDamage(-amount);
    }
}
