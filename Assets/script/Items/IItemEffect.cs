using System;
using System.Collections.Generic;
using UnityEngine;

public interface IItemEffect 
{
    public void Equip(PlayerStats target, ItemLevel level); 
    public void UnEquip(PlayerStats target, ItemLevel level); 
}


[Serializable] 
public struct ItemLevelData<T>
{
    public ItemLevel level;
    public T amount;

    public ItemLevelData(ItemLevel key, T amount)
    {
        this.level = key;
        this.amount = amount;
    }
}
public interface IActivatableEffect : IItemEffect
{
    void Activate(ItemLevel level, ItemContext context, Action onComplete);
    bool CanActivate(ItemContext context);
}

public interface IGridModifierEffect : IActivatableEffect
{

}
public struct ItemContext
{
    public Grid Grid;
    public PlayerPathing Player;
    public EnemyBehaviour Enemy;
    public SpellPathAnimator SpellAnimator;
}

