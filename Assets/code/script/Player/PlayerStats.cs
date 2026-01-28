using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats
{
    

    public float MaxHp;
    public float Hp;

    public int ExperiencePoints;
    public int Level;
    public Dictionary<ElementType, int> ElementalLevels;
    public int ElementalLevelMultiplyer;

    public int Money;
    public List<Spell> SpellBook;

    public List<Item> Inventory;
    public int LimitInventory;

    public int EvasionPoints;
    public int ExtraHealth;
    public int ExtraDamage;
    public PlayerStats()
    {
        MaxHp = 40.0f;
        Hp = MaxHp;
        ExperiencePoints = 0;
        Level = 1;
        Money = 10;
        SpellBook = new List<Spell>();
        Inventory = new List<Item>();
        LimitInventory = 3;
        ElementalLevels = new Dictionary<ElementType, int>()
        {
            { ElementType.Green, 1},
            { ElementType.Red, 1},
            { ElementType.Yellow, 1},
            { ElementType.Blue, 1}
        };
        ElementalLevelMultiplyer = 4;
    
        ExtraHealth = 0;
        EvasionPoints = 0;
    }

    public List<DamageType> AttackWithSpell(Spell spell)
    {
        List<DamageType> damages = new List<DamageType>();
        foreach (Element e in spell.Recipe)
        {
            damages.Add(new DamageType(e.Type, ElementalLevelMultiplyer * ElementalLevels[e.Type]));
        }

        return damages;
    }

    public DamageType AttackWithElement(ElementType elementType)
    {
        DamageType totalDamage = new DamageType(elementType, ElementalLevelMultiplyer * ElementalLevels[elementType]);
        totalDamage.damage += ExtraDamage;

        return totalDamage;
    }
    
    public bool EvadeAttack() { return UnityEngine.Random.Range(0f, 100f) < EvasionPoints; }

    public void Damage(float dT) { Hp -= dT; }

    public void Heal(float amount)
    {
        Hp += amount;
        if (Hp > MaxHp + ExtraHealth)   Hp = MaxHp + ExtraHealth;
    }

    public void AddMoney(int amount) { Money += amount; }
    public bool UseMoney(int amount) 
    {
        if (Money >= amount)
        {
            Money -= amount;
            return true;
        }
        return false;
    }

    public bool AddExp(int amount) 
    { 
        ExperiencePoints += amount; 

        if (ExperiencePoints >= GetEXPForNextLevel())
            return true; // Level up

        return false;
    }

    public void LevelUp()
    {
        Level ++;
    }

    public void UpgradeElementalLevel(ElementType elementType)
    {
        ElementalLevels[elementType] += 1;
    }

    public bool AddItemToInventory(Item item) 
    {
        if (Inventory.Count >= LimitInventory) return false;

        Inventory.Add(item);
        item.ApplyEffects(this);

        return true;// the item was added
    }

    public void RemoveItemFromInventory(Item item)
    {
        if (!Inventory.Contains(item)) return;
        
        Inventory.Remove(item);
        item.RemoveEffects(this);
        
    }

    public void AddSpellToSpellBook(Spell spell)
    {
        if(SpellBook.Count < 4)
            SpellBook.Add(spell);
    }

    public void ClearSpellBook()  { SpellBook.Clear(); }

    public int GetEXPForLastLevel() 
    { 
        if(Level - 1 > 0)
            return (int)Math.Pow(4, Level - 1);

        return 0;
    }
    public int GetEXPForNextLevel() { return (int)Math.Pow(4, Level); }
    public bool CanLevelUp() { return ExperiencePoints >= GetEXPForNextLevel(); }
    public bool IsAlive() { return Hp > 0; }
    public void ModifyEvasion(int amount) { EvasionPoints += amount; }
    public void ModifyExtraHealth(int amount) { ExtraHealth += amount; }
    public void ModifyExtraDamage(int amount) { ExtraDamage += amount; }

    }
