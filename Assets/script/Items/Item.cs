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
    [SerializeField]
    private Sprite TargetSprite; // Used for targeting visuals in grid

    [Header("Behaviour")]
    [SerializeField] private ItemEffectType effectType;
    [SerializeField] private ExtraDamageEffect extraDamageEffect;
    [SerializeField] private FireRingEffect fireRingEffect;

    [Header("Activation")]
    [SerializeField] private ItemActivationType activationType;
    [SerializeField] private bool consumeOnUse = true; // Default to true for IActivatableEffect items
    [SerializeField] private float cooldown;

    private float lastUsedTime = -999f;
    private IItemEffect _effect;

    //Cerate the effec when used and save it 
    public IItemEffect Effect => _effect == null ? ItemEffectFactory.Create(effectType, extraDamageEffect, fireRingEffect) : _effect;

    public string NameItem => nameItem;
    public string Description => description;
    public ItemRarity Rarity => rarity;
    public ItemLevel Level => level;
    public Sprite Sprite => sprite;

    public ItemActivationType ActivationType => activationType;
    public bool ConsumeOnUse => consumeOnUse;
    public float Cooldown => cooldown;

    public bool IsActivatable => Effect is IActivatableEffect && activationType != ItemActivationType.Passive;

    private void OnValidate()
    {
        _effect = null; // Invalidate the saved item when inspector values change
        //yeah... figure out the bug they said
    }

    private void OnEnable()
    {
        // Reset runtime state; SOs persist in memory between
        //editor and play sessions, so lastUsedTime must be cleared.
        lastUsedTime = -999f;
    }

    public void ApplyEffects(PlayerStats player)
    {
        if (Effect != null) Effect.Equip(player, level);
    }

    public void RemoveEffects(PlayerStats player)
    {
        if (Effect != null) Effect.UnEquip(player, level);
    }

    public bool CanActivate(ItemContext context)
    {
        if (!IsActivatable) return false;
        
        // Check cooldown
        if (Time.time - lastUsedTime < cooldown) return false;

        // Check if effect can be activated
        if (Effect is IActivatableEffect activatable)
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

        if (Effect is IActivatableEffect activatable)
        {
            if(Effect is FireRingEffect fireRingEffect)
            {
                fireRingEffect.xSprite = TargetSprite;
            }
            lastUsedTime = Time.time;
            activatable.Activate(level, context, onComplete);
        }
    }

    public float GetCooldownRemaining()
    {
        Debug.Log(cooldown);
        return Mathf.Max(0f, cooldown - (Time.time - lastUsedTime));
    }

    public bool IsOnCooldown()
    {
        return GetCooldownRemaining() > 0f;
    }
}
