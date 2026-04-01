using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItemType", menuName = "Game/Item Type")]
public class Item : ScriptableObject
{
    [Header("SETUP")]
    [SerializeField] private string nameItem;
    [SerializeField] private string description;
    [SerializeField] private ItemRarity rarity;
    private ItemLevel level = ItemLevel.LVL1;

    [Header("Art Stuff")]
    [SerializeField] private Sprite sprite;
    [SerializeField] private Sprite TargetSprite;

    [Header("Behaviour")]
    [SerializeField] private ItemEffectType effectType;
    [SerializeField] private ExtraDamageEffect extraDamageEffect;
    [SerializeField] private FireRingEffect fireRingEffect;
    [SerializeField] private LightningRingEffect lightningRingEffect;
    [SerializeField] private MoonRingEffect moonRingEffect;
    [SerializeField] private ShamrockRingEffect shamrockRingEffect;
    [SerializeField] private FireInstanceEffect fireInstanceEffect;
    [SerializeField] private MoonInstanceEffect moonInstanceEffect;
    [SerializeField] private LightningInstanceEffect lightningInstanceEffect;
    [SerializeField] private ShamrockInstanceEffect shamrockInstanceEffect;
    [SerializeField] private OrangeInstanceEffect orangeInstanceEffect;
    [SerializeField] private MelonInstanceEffect melonInstanceEffect;
    [SerializeField] private OrangePileEffect orangePileEffect;
    [SerializeField] private MelonPileEffect melonPileEffect;
    [SerializeField] private FireObliterateEffect fireObliterateEffect;
    [SerializeField] private LightningObliterateEffect lightningObliterateEffect;
    [SerializeField] private ShamrockObliterateEffect shamrockObliterateEffect;
    [SerializeField] private MoonObliterateEffect moonObliterateEffect;

    [Header("Activation")]
    [SerializeField] private ItemActivationType activationType;
    [SerializeField] private bool consumeOnUse = true;
    [SerializeField] private float cooldown;

    private float lastUsedTime = -999f;
    private IItemEffect _effect;

    public IItemEffect Effect => _effect == null
        ? ItemEffectFactory.Create(effectType, extraDamageEffect, fireRingEffect, lightningRingEffect,
            moonRingEffect, shamrockRingEffect, fireInstanceEffect, moonInstanceEffect,
            lightningInstanceEffect, shamrockInstanceEffect, orangeInstanceEffect, melonInstanceEffect,
            orangePileEffect, melonPileEffect,
            fireObliterateEffect, lightningObliterateEffect, shamrockObliterateEffect, moonObliterateEffect)
        : _effect;

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
        if (Time.time - lastUsedTime < cooldown) return false;

        if (Effect is IActivatableEffect activatable)
            return activatable.CanActivate(context);

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
            SetActivationItemReferences();
            lastUsedTime = Time.time;
            activatable.Activate(level, context, onComplete);
        }
    }

    private void SetActivationItemReferences()
    {
        if (Effect is ActivatableEffectBase baseEffect)
        {
            baseEffect.xSprite = TargetSprite;
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
