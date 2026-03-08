using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItemType", menuName = "Game/Item Type")]
public class Item : ScriptableObject
{
    [Header("SETUP")]
    [SerializeField]
    private string nameItem;
    [SerializeField]
    private string description;
    [SerializeField]
    private ItemRarity rarity;
    private ItemLevel level = ItemLevel.LVL1;

    [Header("Art Stuff")]
    [SerializeField]
    private Sprite sprite;

    [Header("Behaviour")]
    [SerializeReference]
    private IItemEffect effect;

    [SerializeField] private bool AffectGrid;
    [SerializeField] private bool Consumable;

    public string NameItem =>nameItem;
    public string Description =>description;
    public ItemRarity Rarity =>rarity;
    public ItemLevel Level =>level;
    public Sprite Sprite =>sprite;
    public void ApplyEffects(PlayerStats player)
    {
        if (effect != null) effect.Equip(player, level);
    }

    public void RemoveEffects(PlayerStats player)
    {
        if (effect != null) effect.UnEquip(player, level);
    }

}
