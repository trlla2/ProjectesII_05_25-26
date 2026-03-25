using System;
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

    [Header("Activation")]
    [SerializeField] private ItemActivationType activationType;
    [SerializeField] private bool consumeOnUse = true; // Default to true for IActivatableEffect items
    [SerializeField] private float cooldown;

    private float lastUsedTime = -999f;

    public string NameItem => nameItem;
    public string Description => description;
    public ItemRarity Rarity => rarity;
    public ItemLevel Level => level;
    public Sprite Sprite => sprite;

    public ItemActivationType ActivationType => activationType;
    public bool ConsumeOnUse => consumeOnUse;
    public float Cooldown => cooldown;

    public bool IsActivatable => effect is IActivatableEffect && activationType != ItemActivationType.Passive;

    public void ApplyEffects(PlayerStats player)
    {
        if (effect != null) effect.Equip(player, level);
    }

    public void RemoveEffects(PlayerStats player)
    {
        if (effect != null) effect.UnEquip(player, level);
    }

    public bool CanActivate(ItemContext context)
    {
        if (!IsActivatable) return false;
        
        // Check cooldown
        if (Time.time - lastUsedTime < cooldown) return false;

        // Check if effect can be activated
        if (effect is IActivatableEffect activatable)
        {
            return activatable.CanActivate(context);
        }

        return false;
    }

    public void Activate(ItemContext context, Action onComplete)
    {
        if (!IsActivatable)
        {
            Debug.LogWarning($"Item {nameItem} is not activatable!");
            return;
        }

        if (effect is IActivatableEffect activatable)
        {
            lastUsedTime = Time.time;
            activatable.Activate(level, context, onComplete);
        }
    }

    public float GetCooldownRemaining()
    {
        return Mathf.Max(0f, cooldown - (Time.time - lastUsedTime));
    }

    public bool IsOnCooldown()
    {
        return GetCooldownRemaining() > 0f;
    }
}
