using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class EnemyBehaviour : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private EnemyType enemyType;
    [SerializeField] private Grid grid;

    [Header("Visual Effects")]
    [SerializeField] private GameObject deathParticles;


    private SpriteRenderer spriteRenderer;

    private float currentHealth;
    private bool inBattle = false;
    private bool isCastingSpell = false;

    private IEnemySpell activeSpell;

    public event System.Action<float> OnHealthChanged;
    public event System.Action<List<DamageType>> OnDamageReceived;
    public event System.Action<float> OnAttackTimerChanged;
    public event System.Action OnDeath;

    //Legacy System
    public delegate void GetOnHitRecived(float health);
    public event GetOnHitRecived OnHitRecived;

    public delegate void GetOnDamageList(List<DamageType> damageList);
    public event GetOnDamageList OnDamageList;

    public delegate void GetOnAttackTimeChange(float attackTime);
    public event GetOnAttackTimeChange OnAttackTimeChange;


    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        InitializeEnemy();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetEnemyRef(this);
        }
    }

    private void OnDestroy()
    {
        StopBattle();
    }

    private void InitializeEnemy()
    {
        if (enemyType == null)
        {
            Debug.LogError("EnemyBehaviour: No EnemyType assigned!");
            return;
        }

        spriteRenderer.sprite = enemyType.EnemySprite;
        currentHealth = enemyType.Health;
    }

    public void SetEnemyType(EnemyType newType)
    {
        enemyType = newType;
        InitializeEnemy();
    }

    public void StartBattle()
    {
        if (inBattle)
            return;

        inBattle = true;

        //Start by targeting
        CreateAndExecuteSpell();

        if (GameManager.Instance != null)
            GameManager.Instance.OnPlayerMove += HandlePlayerMove;
    }

    public void StopBattle()
    {
        if (!inBattle)
            return;

        inBattle = false;
        isCastingSpell = false;
        if (GameManager.Instance != null)
            GameManager.Instance.OnPlayerMove -= HandlePlayerMove;

        if (activeSpell != null && grid != null)
        {
            for (int x = 0; x < Grid.GridSize; x++)
                for (int y = 0; y < Grid.GridSize; y++)
                {
                    var pos = new Vector2Int(x, y);
                    var visual = grid.GetElementVisualAt(pos);
                    if (visual != null && visual.IsTargetedByEnemy)
                    {
                        visual.SetTargetedByEnemy(false);
                    }
                }
        }

        activeSpell = null;
        StopAllCoroutines();
    }

    private void CreateAndExecuteSpell()
    {
        if (enemyType == null || grid == null)
            return;

        activeSpell = enemyType.CreateSpell();
        if (activeSpell != null)
        {
            activeSpell.Execute(grid);
            isCastingSpell = true;
        }
    }

    private void HandlePlayerMove()
    {
        if (!inBattle || activeSpell == null)
            return;

        activeSpell.Tick(grid, () =>
        {
            isCastingSpell = false;
            CreateAndExecuteSpell();
        });
    }

    public void TakeDamage(List<DamageType> damageList)
    {
        if (damageList == null || damageList.Count == 0)
            return;

        float totalDamage = 0f;
        List<DamageType> processedDamage = new List<DamageType>();

        foreach (DamageType damageType in damageList)
        {
            float finalDamage = enemyType.CalculateDamageTaken(damageType.element, damageType.damage);
            totalDamage += finalDamage;
            processedDamage.Add(new DamageType(damageType.element, finalDamage));
        }

        currentHealth -= totalDamage;
        currentHealth = Mathf.Max(0, currentHealth);

        OnHealthChanged?.Invoke(currentHealth);
        OnDamageReceived?.Invoke(processedDamage);

        //Legacy events
        OnHitRecived?.Invoke(currentHealth);
        OnDamageList?.Invoke(processedDamage);

        Debug.Log($"Enemy {enemyType.name} took {totalDamage} damage. Health: {currentHealth}/{enemyType.Health}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        StopBattle();

        if (deathParticles != null)
        {
            GameObject particles = Instantiate(deathParticles, transform.position, Quaternion.identity);
            particles.transform.SetParent(null);
        }

        OnDeath?.Invoke();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnEnemyDead();
        }
        spriteRenderer.enabled = false;

        Debug.Log($"Enemy {enemyType.name} has been defeated!");
    }

    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => enemyType.Health;
    public bool IsInBattle() => inBattle;
    public bool IsCastingSpell() => isCastingSpell;
    public EnemyType GetEnemyType() => enemyType;

    // Legacy support methods
    public void InitBatle() => StartBattle();
    public void StopEnemy() => StopBattle();
    public float GetMaxHP() => GetMaxHealth();
    public void DealDamage(List<DamageType> damageList) => TakeDamage(damageList);
}