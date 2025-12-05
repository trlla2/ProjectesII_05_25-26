using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OLD_PlayerStats : MonoBehaviour
{
    public static OLD_PlayerStats Instance { get; private set; }
    [Header("Stats")]

    [SerializeField]
    private float maxHp = 100.0f;
    private float hp;

    private int experiencePoints = 0;
    private int level = 1;

    [SerializeField]
    private int money = 10;

    [SerializeField]
    private List<Spell> spellBook = new List<Spell>();


    [SerializeField]
    private List<ItemsType> inventory = new List<ItemsType>();


    // Events
    public delegate void GetOnHitRecived(float health);
    public event GetOnHitRecived OnHitRecived;

    public delegate void GetOnHitEvaded();
    public event GetOnHitEvaded OnHitEvaded;

    public delegate void GetOnLevelUp();
    public event GetOnLevelUp OnLevelUp;

    public delegate void GetOnEXPChange();
    public event GetOnEXPChange OnEXPChange;

    public delegate void GetOnMoneyChange();
    public event GetOnMoneyChange OnMoneyChange;

    public delegate void GetOnInventoryChange();
    public event GetOnInventoryChange OnInventoryChange;

    // Add Crit and other stuff...
    private Dictionary<ElementType, int> elementalLevels = new Dictionary<ElementType, int>() // forced DELETE!@!!!!
    { 
        { ElementType.Green, 1}, 
        { ElementType.Red, 1}, 
        { ElementType.Yellow, 1}, 
        { ElementType.Blue, 1}
    };

    private int elementalLevelMultiplyer = 4;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start() // FORCED DELETE !!@@!@!@!!
    {
        hp = maxHp;
    }


    public void Attack(Spell spell)
    {
        List<DamageType> damages = new List<DamageType>();
        foreach (Element e in spell.Recipe)
        {
            damages.Add(new DamageType(e.Type, elementalLevelMultiplyer * elementalLevels[e.Type]));
        }


        //GameManager.Instance.AttackEnemy(damages);
    }

    public void DealDamage(float damage)
    {
        // --------------------------- FORCED DELETE
        float evasionChance = 0;

        foreach(ItemsType item in inventory)
            evasionChance += item.evasionBonus;

        if (UnityEngine.Random.Range(0f, 100f) < evasionChance)
        {
            OnHitEvaded?.Invoke();
            return;
        }

        // -----------------------------------------

        hp -= damage;
        
        OnHitRecived?.Invoke(hp);
        
        if (hp <= 0)
        {
            GameManager.Instance.OnPlayerDead();
        }
    }

    public void OnWinBattle(int exp, int m)
    {
        money += m;

        OnMoneyChange?.Invoke();

        experiencePoints += exp;
        OnEXPChange?.Invoke();

        if (experiencePoints >= GetEXPForNextLevel())
        {
            OnLevelUp?.Invoke();
        }
    }

    public void UpgradeElement(ElementType type)
    {
        level++;

        elementalLevels[type]++;
    }

    public void AddItemToInventory(ItemsType item)
    {
        inventory.Add(item);
        OnInventoryChange?.Invoke();
    }


    public void Heal(int health)
    {
        if (hp + health <= maxHp)
        {
            Mathf.Clamp(hp += health, 0, maxHp);
            OnHitRecived?.Invoke(hp);
        }
        else
        {
            hp = maxHp;
            OnHitRecived?.Invoke(hp);
        }
    }

    public bool UseMoney(int amount)
    {
        if (money >= amount)
        {
            money = money - amount;
            OnMoneyChange?.Invoke();
            return true;
        }
        return false;
    }

    public void Destroy()
    {
        Destroy(this.gameObject);
    }

    public List<Spell> GetSpellBook() { return spellBook; }
    public int GetEXP() { return experiencePoints; }
    public int GetMoney() { return money; }
    public float GetMaxHP() { return maxHp; }
    
    public float GetCurrentHP() { return hp; }
    public int GetLevel() { return level; }
    public int GetEXPForNextLevel () { return (int)Math.Pow(4, level); }
    public List<ItemsType> GetInventory() { return inventory; }



}
