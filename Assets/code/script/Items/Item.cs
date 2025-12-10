using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItemType", menuName = "Game/Item Type")]
public class Item : ScriptableObject
{
    [Header("SETUP")]
    public string nameItem;
    public string description;


    [Header("Art Stuff")]
    public Sprite enemySprite;

    [Header("Behaviour")]
    [SerializeReference]
    public List<IItemEffect> effects = new List<IItemEffect>();

    public void ApplyEffects(PlayerStats player)
    {
        foreach (var effect in effects)
        {
            if (effect != null) effect.Equip(player);
        }
    }

    public void RemoveEffects(PlayerStats player)
    {
        foreach (var effect in effects)
        {
            if (effect != null) effect.UnEquip(player);
        }
    }
}
