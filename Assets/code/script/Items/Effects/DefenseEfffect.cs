using UnityEngine;

public class DefenseEfffect : IItemEffect
{
    [SerializeField]
    private float amount = 1;
    public void Equip(PlayerStats target)
    {
        target.ModifyDefense(amount);
    }

    public void UnEquip(PlayerStats target)
    {
        target.ModifyDefense(-amount);
    }
}
