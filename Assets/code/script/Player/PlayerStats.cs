using System;
using System.Collections.Generic;


class PlayerStats
{
    public float MaxHp;
    public float Hp;
    public int ExperiencePoints;
    public int Level;
    public int Money;
    public List<Spell> SpellBook;
    public List<ItemsType> Inventory;
    public Dictionary<ElementType, int> ElementalLevels;
    public int ElementalLevelMultiplyer;

    public PlayerStats()
    {
        MaxHp = 100.0f;
        Hp = MaxHp;
        ExperiencePoints = 0;
        Level = 1;
        Money = 10;
        SpellBook = new List<Spell>();
        Inventory = new List<ItemsType>();
        ElementalLevels = new Dictionary<ElementType, int>()
        {
            { ElementType.Green, 1},
            { ElementType.Red, 1},
            { ElementType.Yellow, 1},
            { ElementType.Blue, 1}
        };
        ElementalLevelMultiplyer = 4;
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
        return new DamageType(elementType, ElementalLevelMultiplyer * ElementalLevels[elementType]);
    }

    
    public bool EvadeAttack()
    {
        float evasionChance = 0;

        foreach (ItemsType item in Inventory)
            evasionChance += item.evasionBonus;

        return UnityEngine.Random.Range(0f, 100f) < evasionChance;
    }

    public void Damage(float dT) { Hp -= dT; }

    public void Heal(float amount)
    {
        Hp += amount;
        if (Hp > MaxHp)
            Hp = MaxHp;
    }

    public void AddMoney(int amount) { Money += amount; }
    public bool UseMoney(int amount) 
    {
        if (Money >= amount)
        {
            Money = Money - amount;
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

    public void AddItemToInventory(ItemsType item) { Inventory.Add(item); }

    public void AddSpellToSpellBook(Spell spell)
    {
        if(SpellBook.Count < 4)
            SpellBook.Add(spell);
    }

    public int GetEXPForNextLevel() { return (int)Math.Pow(4, Level); }
    public bool CanLevelUp() { return ExperiencePoints >= GetEXPForNextLevel(); }
    public bool IsAlive() { return Hp > 0; }

    }
