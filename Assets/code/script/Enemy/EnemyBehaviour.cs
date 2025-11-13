using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class EnemyBehaviour : MonoBehaviour
{
    [Header("EnemyType")]
    [SerializeField]
    private EnemyType enemyType;
    [Header("DeathParticles")]
    [SerializeField]
    private GameObject deathParticles;

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
    
    public void SetEnemyType(EnemyType newType)
    {
        enemyType = newType;
        spriteRenderer.sprite = enemyType.GetSprite();
        health = enemyType.GetHealth();
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

        List<DamageType> finalDamageList = new List<DamageType>();

        foreach (DamageType damageType in damageList)
        {
            finalDamage += enemyType.CalculateDamageTaken(damageType.element, damageType.damage);
            finalDamageList.Add(new DamageType(damageType.element, enemyType.CalculateDamageTaken(damageType.element, damageType.damage)));
        }

        health -= finalDamage;
        OnDamageList?.Invoke(finalDamageList);
        OnHitRecived?.Invoke(health);
        Debug.Log("Enemy " + enemyType.name + " has taken " + finalDamage + " damage points with " + health + " remaining health points");

        if(health <= 0)
        {
            GameObject part = Instantiate(deathParticles, transform.position, Quaternion.identity);
            part.transform.DetachChildren();
            GameManager.Instance.OnEnemyDead();
            Destroy(this.gameObject);
        }
    }
    
    public void StopEnemy()
    {
        inBattle = false;
    }

    public float GetMaxHP() { return enemyType.GetHealth(); }
    public float GetFireRate() { return enemyType.GetFireRate(); }

    public EnemyType GetEnemyType() { return enemyType; }
}
