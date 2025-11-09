using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }
    [Header("Stats")]

    [SerializeField]
    private float maxHp = 100.0f;
    private float hp;

    private int experiencePoints = 0;
    private int level = 1;
    private int money = 0;

    public delegate void GetOnHitRecived(float health);
    public event GetOnHitRecived OnHitRecived;

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


        GameManager.Instance.AttackEnemy(damages);
    }

    public void DealDamage(float damage)
    {
        hp -= damage;
        Debug.Log("Player has taken " + damage + " damage points with " + hp + " remaining health points");
        OnHitRecived?.Invoke(hp);
        if (hp <= 0)
        {
            Debug.Log("------------------------  PLAYER IS DEAD ---------------------------------");
            GameManager.Instance.OnPlayerDead();
        }
    }

    public void OnWinBattle(int exp, int m)
    {
        money += m;

        experiencePoints += exp;

        if(experiencePoints >= Math.Pow(4, level))
        {
            level++;

        }
    }

    public int GetEXP() { return experiencePoints; }
    public int GetMoney() { return money; }
    public float GetMaxHP() { return maxHp; }
}
