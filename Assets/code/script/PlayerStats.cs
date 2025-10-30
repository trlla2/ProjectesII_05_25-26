using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Stats")]

    [SerializeField]
    private float maxHp = 100.0f;
    private float hp;


    // Add Crit and other stuff...
    private Dictionary<ElementType, float> elementDamages = new Dictionary<ElementType, float>() // forced DELETE!@!!!!
    { 
        { ElementType.Green, 4.0f}, 
        { ElementType.Red, 4.0f}, 
        { ElementType.Yellow, 4.0f}, 
        { ElementType.Blue, 4.0f}, 
        
    };

    private void Start()
    {
        hp = maxHp;

        GameManager.Instance.SetPlayerRef(this); // forced DELETE!@!!!!! 
    }


    public void Attack(Spell spell)
    {
        List<DamageType> damages = new List<DamageType>();
        foreach (Element e in spell.Recipe)
        {
            damages.Add(new DamageType(e.Type, elementDamages[e.Type]));
        }


        GameManager.Instance.AttackEnemy(damages);
    }

    public void DealDamage(float damage)
    {
        hp -= damage;
        Debug.Log("Player has taken " + damage + " damage points with " + hp + " remaining health points");

        if (hp <= 0)
        {
            Debug.Log("------------------------  PLAYER IS DEAD ---------------------------------");
            GameManager.Instance.OnPlayerDead();
        }
    }

}
