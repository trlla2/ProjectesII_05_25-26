using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerStats
{

    // combat stats
    private const float baseMaxHP = 40f;
    private float maxHp;
    public float MaxHp => maxHp;
    private float hp;
    public float Hp => hp;

    private int evasionPoints;
    private int extraHealth;
    public int ExtraHealth => extraHealth;
    private int extraDamage;
    private float defense;
    private int critPercentage;
    private const float critMultiplayer = 1.5f;
    private int spikeDamage;
    public int SpikeDamage => spikeDamage;

    // level stats
    private int experiencePoints;
    public int ExperiencePoints => experiencePoints;
    private int level;
    public int Level => level;
    private Dictionary<ElementType, int> elementalLevels;
    private Dictionary<ElementType, int> elementalUpgrades;
    private int elementalLevelMultiplyer;

    // currency 
    private int money;
    public int Money => money;

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


    //Events
    public event System.Action OnEXPChange;
    public event System.Action OnLevelUp;
    public event System.Action OnMoneyChange;
    public event System.Action OnInventoryChange;

    public PlayerStats()
    {
        ResetCombatStats();
        ResetLevelStats();


        // currency 
        money = 1000;

        // spells  & Items
        spellBook = new Spell[spellBookSize];
        knownSpells = new List<Spell>();
        inventory = new List<Item>();
        limitInventory = 16; // 3

        foreach (Item debugItem in GameManager.Instance._debugItemsInInventory)
        {
            AddItemToInventory(debugItem);
        }
    }
    private void ResetLevelStats()
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

        evasionPoints = 0;
        extraHealth = 0;
        extraDamage = 0;
        defense = 0;
        critPercentage = 0;
        spikeDamage = 0;
    }
    public void ResetStatsOnDeath()
    {
        RemoveAllItems();
        ResetCombatStats();
        ResetLevelStats();
    }

    public List<DamageType> AttackWithSpell(Spell spell)
    {
        List<DamageType> damages = new List<DamageType>();
        foreach (Element e in spell.Recipe)
        {
            damages.Add(AttackWithElement(e.Type));
        }

        return damages;
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

    public void AddMoney(int amount) 
    {
        money += amount;
        OnMoneyChange?.Invoke();
    }
    public bool UseMoney(int amount)
    {
        if (money >= amount)
        {
            money -= amount;
            OnMoneyChange?.Invoke();
            return true;
        }
        return false;
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
            // Remove item if consumable
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

        Debug.Log("Spell Added");
    }

    public void ClearSpellBook()
    {
        for (int i = 0; i < spellBookSize; i++)
            spellBook[i] = null;
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
    }

    public int GetEXPForLastLevel()
    {
        if (level - 1 > 0)
            return (int)Math.Pow(4, level - 1);

        return 0;
    }
    public int GetElementLevel(ElementType t) { return elementalLevels[t]; }
    public int GetElementUpgradeLevel(ElementType t) { return elementalUpgrades[t]; }
    public int GetEXPForNextLevel() { return (int)Math.Pow(4, level); }
    public bool CanLevelUp() { return experiencePoints >= GetEXPForNextLevel(); }
    public bool IsAlive() { return hp > 0; }
    public float GetHPPercentage() => Hp / (MaxHp + ExtraHealth);
    public void ModifyEvasion(int amount) { evasionPoints += amount; }
    public void ModifyExtraHealth(int amount) { extraHealth += amount; }
    public void ModifyExtraDamage(int amount) { extraDamage += amount; }
    public void ModifyDefense(float amount) { defense += amount; }
    public void ModifyCritPercentage(int amount) { critPercentage += amount; }
    public void ModifySipkeDamage(int amount) { spikeDamage += amount; }
    public int GetCurrentElementUpgrades(ElementType elementType) { return elementalUpgrades[elementType]; }
    public int GetCurrentElementLevels(ElementType elementType) { return elementalLevels[elementType]; }
}
