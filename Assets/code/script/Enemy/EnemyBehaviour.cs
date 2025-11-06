using System.Collections;
using System.Collections.Generic;
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
    private float attackTime = 0f;

    public delegate void GetOnHitRecived(float health);
    public event GetOnHitRecived OnHitRecived;
    
    public delegate void GetOnDamageList(List<DamageType> damageList);
    public event GetOnDamageList OnDamageList;

    public delegate void GetOnAttackTimeChange(float attackTime);
    public event GetOnAttackTimeChange OnAttackTimeChange;

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

            if (attackTime < enemyType.GetFireRate())
            {
                attackTime += Time.deltaTime;
            }
            else
            {
                attackTime = 0f;
                GameManager.Instance.AttackPlayer(enemyType.GetDamage());
            }
            OnAttackTimeChange?.Invoke(attackTime);

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
        OnDamageList?.Invoke(damageList);
        OnHitRecived?.Invoke(health);
        Debug.Log("Enemy " + enemyType.name + " has taken " + finalDamage + " damage points with " + health + " remaining health points");
    }
    
    public void StopEnemy()
    {
        inBattle = false;
    }

    public float GetMaxHP() { return enemyType.GetHealth(); }
    public float GetFireRate() { return enemyType.GetFireRate(); }
}
