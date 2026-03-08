using System;

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
