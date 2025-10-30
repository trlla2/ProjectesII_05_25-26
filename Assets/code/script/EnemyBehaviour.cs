using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class EnemyBehaviour : MonoBehaviour
{
    [Header("Setup")]

    [SerializeField]
    private EnemyType enemyType;

    private SpriteRenderer spriteRenderer;

    private bool inBattle = false;

    // Actual stats of the enemy
    private float health;
    private float attackTimer = 0f;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.sprite = enemyType.GetSprite();
        health = enemyType.GetHealth();


        GameManager.Instance.SetEnemyRef(this); // forced DELETE!@!!!!! 
        InitBatle(); // forced DELETE!@!!!!! 
    }


    public void InitBatle()
    {
        inBattle = true;
        StartCoroutine(Attack());
    }

    private IEnumerator Attack()
    {
        while (inBattle)
        {
            if (attackTimer < enemyType.GetFireRate())
            {
                attackTimer += Time.deltaTime;
            }
            else
            {
                attackTimer = 0f;
                GameManager.Instance.AttackPlayer(enemyType.GetDamage());
            }
            yield return null;
        }
    }

    public void DealDamage(List<DamageType> damageList)
    {
        float finalDamage = 0;

        foreach(DamageType damageType in damageList)
        {
            finalDamage += enemyType.CalculateDamageTaken(damageType.element, damageType.damage);
        }

        health -= finalDamage;

        Debug.Log("Enemy " + enemyType.name + " has taken " + finalDamage + " damage points with " + health + " remaining health points");
    }
    
    public void StopEnemy()
    {
        inBattle = false;
    }
}
