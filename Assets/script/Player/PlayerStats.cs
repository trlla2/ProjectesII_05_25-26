using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats
{

    // combat stats
    private const float baseMaxHP = 40f;
    private float maxHp;
    public float MaxHp => maxHp;
    private float hp;
    public float Hp => hp;

    private int _evasionBaseUpgrades = 0;
    private int _evasionModifier = 0;
    private int evasionPoints => 0 + _evasionModifier + (1 * _evasionBaseUpgrades);

    private int _maxHealthBaseUpgrades = 0;
    private float _maxHealthModifier = 0;
    private float extraHealth => 0 + _maxHealthModifier + (2 * _maxHealthBaseUpgrades);

    private int _damageBaseUpgrades = 0;
    private int _damageModifier = 0;
    private int extraDamage => 0 + _damageModifier + (1 * _damageBaseUpgrades);

    private int _defenseBaseUpgrades = 0;
    private float _defenseModifier = 0;
    private float defense => 0 + _defenseModifier + (1 * _defenseBaseUpgrades);

    private int _critBaseUpgrades = 0;
    private int _critPercentModifier = 0;
    private int critPercentage => 0 + _critPercentModifier + (1 * _critBaseUpgrades);
    private const float critMultiplayer = 1.5f;
    private int spikeDamage;
    public int SpikeDamage => spikeDamage;

    // Combay Stats 
    private int experiencePoints;
    public int ExperiencePoints => experiencePoints;
    private int level;
    public int Level => level;
    private Dictionary<ElementType, int> elementalLevels;
    private Dictionary<ElementType, int> elementalUpgrades;
    private int elementalLevelMultiplyer;

    // currency 
    private Dictionary<CurrencyType, int> currencyAmounts = new Dictionary<CurrencyType, int>()
    {
        { CurrencyType.GOLD, 0 },
        { CurrencyType.RUBY, 0 },
        { CurrencyType.EXP, 0 }
    };
    private int battlePassLevel;
    public int BattlePassLevel => battlePassLevel;


    // spells  & Items
    private const int spellBookSize = 4;
    public int SpellBookSize => spellBookSize;
    private Spell[] spellBook;
    public Spell[] SpellBook => spellBook;
    private List<Spell> knownSpells;
    public List<Spell> KnownSpells => knownSpells;

    private List<Item> inventory;
    public List<Item> Inventory => inventory;
    private int limitInventory;

    //Skins
    private HashSet<SkinType> unlockedSkins = new HashSet<SkinType>() { SkinType.Default };
    public HashSet<SkinType> UnlockedSkins => unlockedSkins;
    private SkinType currentSkin = SkinType.Default;
    public SkinType CurrentSkin => currentSkin;

    //Events
    public event System.Action OnEXPChange;
    public event System.Action OnLevelUp;
    public event System.Action<CurrencyType, int> OnCurrencyChange;
    public event System.Action OnInventoryChange;
    public event System.Action OnSpellBookChange;
    public event System.Action OnSkinChange;
    public event System.Action OnUnlockedSkinsChange;
    public event System.Action OnBattlePassLevelUp;
    public event System.Action OnKnownSpellChange;



    public event System.Action<ModificableStat, int> OnStatUpgradeLevelChanged;

    public PlayerStats()
    {
        ResetCombatStats();
        ResetLevelStats();

        // currency 
        currencyAmounts[CurrencyType.GOLD] = 50;

        // spells  & Items
        spellBook = new Spell[spellBookSize];
        knownSpells = new List<Spell>();
        inventory = new List<Item>();
        limitInventory = 4;

        foreach (Item debugItem in GameManager.Instance._debugItemsInInventory)
        {
            AddItemToInventory(debugItem);
        }
    }
    public void ResetLevelStats()
    {
        experiencePoints = 0;
        level = 1;
        elementalLevels = new Dictionary<ElementType, int>()
        {
            { ElementType.Green, 1},
            { ElementType.Red, 1},
            { ElementType.Yellow, 1},
            { ElementType.Blue, 1}
        };

        elementalUpgrades = new Dictionary<ElementType, int>()
        {
            { ElementType.Green, 0},
            { ElementType.Red, 0},
            { ElementType.Yellow, 0},
            { ElementType.Blue, 0}
        };

        elementalLevelMultiplyer = 4;
    }
    private void ResetCombatStats()
    {
        maxHp = baseMaxHP;
        hp = maxHp;
        
        _evasionBaseUpgrades = 0;
        _maxHealthBaseUpgrades = 0;
        _damageBaseUpgrades = 0;
        _defenseBaseUpgrades = 0;

        _evasionModifier = 0;
        _maxHealthModifier = 0;
        _damageModifier = 0;
        _defenseModifier = 0;
        _critPercentModifier = 0;
        spikeDamage = 0;

        currencyAmounts[CurrencyType.GOLD] = 50;
    }
    public void ResetStatsOnDeath()
    {
        RemoveAllItems();
        ResetCombatStats();
        //ResetLevelStats();
    }


    public DamageType AttackWithElement(ElementType elementType)
    {
        DamageType totalDamage = new DamageType(elementType, elementalLevelMultiplyer * elementalLevels[elementType]);
        totalDamage.damage += extraDamage;

        if (UnityEngine.Random.Range(0f, 100f) < critPercentage)
        {
            totalDamage.damage *= critMultiplayer;
            totalDamage.IsCrit = true;
        }

        return totalDamage;
    }

    public bool EvadeAttack() { return UnityEngine.Random.Range(0f, 100f) < evasionPoints; }

    public void Damage(float damage)
    {
        float totalDamage = damage - defense;
        if (totalDamage <= 0f)
            return;
        HapticsManager.Instance.GenerateHapticFeedback(HapticsManager.HapticTypes.HeavyImpact);
        hp -= damage;
    }

    public void Heal(float amount)
    {
        hp += amount;
        if (hp > maxHp + extraHealth) hp = maxHp + extraHealth;
    }

    public void AddCurrency(CurrencyType currencyType, int amount) 
    {
        if (!currencyAmounts.ContainsKey(currencyType))
            currencyAmounts[currencyType] = 0;

        currencyAmounts[currencyType] += amount;
        OnCurrencyChange?.Invoke(currencyType ,amount);
    }
    public bool UseCurrency(CurrencyType currencyType, int amount)
    {
        if (currencyAmounts[currencyType] >= amount)
        {
            currencyAmounts[currencyType] -= amount;
            OnCurrencyChange?.Invoke(currencyType, -amount);
            return true;
        }
        return false;
    }

    public bool CanUseCurrency(CurrencyType currencyType, int amount)
    {
        return currencyAmounts[currencyType] >= amount;
    }

    public int GetCurrency(CurrencyType currencyType)
    {
        if (currencyAmounts.ContainsKey(currencyType))
            return currencyAmounts[currencyType];
        return 0;
    }

    public void AddBattlePassLevel()
    {
        battlePassLevel ++;
        OnBattlePassLevelUp?.Invoke();
    }

    public void AddExp(int amount)
    {
        experiencePoints += amount;

        OnEXPChange?.Invoke();
        if (CanLevelUp())
            OnLevelUp.Invoke();
            
    }
    

    private void LevelUp()
    {
        level++;
    }

    public void UpgradeElementalLevel(ElementType elementType)
    {
        elementalUpgrades[elementType]++;

        int currentLevel = elementalLevels[elementType];
        int upgradesRequired = (int)Math.Pow(2, currentLevel - 1);

        if (elementalUpgrades[elementType] >= upgradesRequired)
        {
            elementalLevels[elementType]++;
            elementalUpgrades[elementType] = 0;
            LevelUp();
        }
    }

    public bool AddItemToInventory(Item item)
    {
        if (inventory.Count >= limitInventory) return false;

        inventory.Add(item);
        item.ApplyEffects(this);

        OnBoardingManager.Instance.TriggerIfNewWithDelay(OnBoardingKey.FIRST_ITEM_PICKUP);

        OnInventoryChange?.Invoke();

        return true;// the item was added
    }

    public void RemoveItemFromInventory(Item item)
    {
        if (!inventory.Contains(item)) return;

        inventory.Remove(item);
        item.RemoveEffects(this);
        OnInventoryChange?.Invoke();
    }

    public void RemoveAllItems()
    {
        foreach (Item item in inventory)
        {
            item.RemoveEffects(this);
        }
        inventory.Clear();

        OnInventoryChange?.Invoke();
    }

    public bool UseItem(Item item, ItemContext context, Action onComplete = null)
    {
        if (!inventory.Contains(item))
        {
            Debug.LogWarning("Item not in inventory!");
            return false;
        }

        if (!item.IsActivatable)
        {
            Debug.LogWarning($"Item {item.NameItem} is not activatable!");
            return false;
        }

        if (!item.CanActivate(context))
        {
            Debug.LogWarning($"Cannot activate {item.NameItem} right now!");
            return false;
        }

        item.Activate(context, () =>
        {
            if (item.ConsumeOnUse)
            {
                RemoveItemFromInventory(item);
            }

            onComplete?.Invoke();
        });

        return true;
    }

    public void AddSpellToSpellBook(Spell spell, int slot)
    {
        if (spell == null || slot < 0 || slot > spellBookSize - 1) return;

        spellBook[slot] = spell;

        OnSpellBookChange?.Invoke();
    }

    public void ClearSpellBook()
    {
        for (int i = 0; i < spellBookSize; i++)
            spellBook[i] = null;

        OnSpellBookChange?.Invoke();
    }

    public bool ExistSpellOnSpellBook(Spell spell)
    {
        for (int i = 0; i < SpellBookSize; i++)
        {
            if (SpellBook[i] == spell)
                return true;
        }
        return false;
    }
    public List<Spell> GetSpellBook()
    {
        List<Spell> spelL = new List<Spell>();

        for (int i = 0; i < SpellBookSize; i++)
        {
            if (SpellBook[i] != null)
                spelL.Add(SpellBook[i]);
        }

        return spelL;
    }

    public void AddKnownSpell(Spell spell)
    {
        if (knownSpells.Contains(spell))
            return;

        knownSpells.Add(spell);
        OnKnownSpellChange?.Invoke();
    }

    public int GetEXPForLastLevel()
    {
        if (level - 1 > 0)
            return (int)Math.Pow(4, level - 1);

        return 0;
    }

    public bool AddNewSkin(SkinType skinType)
    {
        if (unlockedSkins.Contains(skinType))
            return false;
        unlockedSkins.Add(skinType);
        OnUnlockedSkinsChange?.Invoke();
        return true;
    }

    public void ChangeSkin(SkinType skinType)
    {
        if (!unlockedSkins.Contains(skinType))
        {
            Debug.LogWarning($"Skin {skinType} not unlocked!");
            return;
        }
        currentSkin = skinType;
        OnSkinChange?.Invoke();
    }

    public int GetElementLevel(ElementType t) { return elementalLevels[t]; }
    public int GetElementUpgradeLevel(ElementType t) { return elementalUpgrades[t]; }
    public int GetEXPForNextLevel() { return (int)Math.Pow(4, level); }
    public bool CanLevelUp() { return experiencePoints >= GetEXPForNextLevel(); }
    public bool IsAlive() { return hp > 0; }
    public float GetHPPercentage() => Hp / (MaxHp + extraHealth);
    public void ModifyExtraHealth(float amount) { _maxHealthModifier += amount; }
    public void ModifyEvasion(int amount) { _evasionModifier += amount; }
    public void ModifyExtraDamage(int amount) { _damageModifier += amount; }
    public void ModifyDefense(float amount) { _defenseModifier += amount; }
    public void ModifyCritPercentage(int amount) { _critPercentModifier += amount; }
    public void ModifySipkeDamage(int amount) { spikeDamage += amount; }
    public int GetCurrentElementUpgrades(ElementType elementType) { return elementalUpgrades[elementType]; }
    public int GetCurrentElementLevels(ElementType elementType) { return elementalLevels[elementType]; }

    public void ModifyEvasionUpgradesLvl(int amount)
    {
        _evasionBaseUpgrades += amount;
        OnStatUpgradeLevelChanged?.Invoke(ModificableStat.EVASION, _evasionBaseUpgrades);
        OnBoardingManager.Instance.TriggerIfNewWithDelay(OnBoardingKey.BUFFS_N_DEBUFFS);
    }

    public void ModifyExtraHealthUpgradesLvl(int amount)
    {
        _maxHealthBaseUpgrades += amount;
        OnStatUpgradeLevelChanged?.Invoke(ModificableStat.HEALTH, _maxHealthBaseUpgrades);
        OnBoardingManager.Instance.TriggerIfNewWithDelay(OnBoardingKey.BUFFS_N_DEBUFFS);
    }

    public void ModifyExtraDamageUpgradesLvl(int amount)
    {
        _damageBaseUpgrades += amount;
        OnStatUpgradeLevelChanged?.Invoke(ModificableStat.DAMAGE, _damageBaseUpgrades);
        OnBoardingManager.Instance.TriggerIfNewWithDelay(OnBoardingKey.BUFFS_N_DEBUFFS);
    }

    public void ModifyDefenseUpgradesLvl(int amount)
    {
        _defenseBaseUpgrades += amount;
        OnStatUpgradeLevelChanged?.Invoke(ModificableStat.DEFENSE, _defenseBaseUpgrades);
        OnBoardingManager.Instance.TriggerIfNewWithDelay(OnBoardingKey.BUFFS_N_DEBUFFS);
    }

    public void HealPercentage(float percentage)
    {
        Heal((maxHp + extraHealth) * percentage);
    }

    //public bool IsAnyStatUpgraded()
    //{
    //    return _evasionBaseUpgrades > 0 || _maxHealthBaseUpgrades > 0 || _damageBaseUpgrades > 0 || _defenseBaseUpgrades > 0;
    //}

}
