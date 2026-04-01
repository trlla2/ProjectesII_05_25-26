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
public interface IReactiveEffect : IItemEffect
{
    void Subscribe(ItemLevel level);
    void Unsubscribe();
}

public interface IActivatableEffect : IItemEffect
{
    void Activate(ItemLevel level, ItemContext context, Action onComplete);
    bool CanActivate(ItemContext context);
}

public interface IGridModifierEffect : IActivatableEffect
{

}

public interface IOnSpellCastEffect : IItemEffect
{
    void OnSpellCast(Grid grid, ItemLevel level, Spell spell, List<Element> castElements,
                     List<Vector2Int> positions, SpellPathAnimator animator);
}

public struct ItemContext
{
    public Grid Grid;
    public PlayerStats Player;
    public EnemyBehaviour Enemy;
    public SpellPathAnimator SpellAnimator;
}

