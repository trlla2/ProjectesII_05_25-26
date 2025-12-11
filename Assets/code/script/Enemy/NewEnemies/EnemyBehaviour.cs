using UnityEditor.Animations;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(AnimatorController))]
public class EnemyBehaviour : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private EnemyType enemyType;
    [SerializeField] private Grid grid;

    [Header("Visual Effects")]
    [SerializeField] private GameObject deathParticles;


    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private float currentHealth;
    private bool inBattle = false;

    private IEnemySpell activeSpell;

    //Legacy System
    public delegate void GetOnHitRecived(float health);
    public event GetOnHitRecived OnHitRecived;

    public delegate void GetOnDamageList(DamageType damageType);
    public event GetOnDamageList OnGetDamage;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        
    }

    private void Start()
    {
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
            Debug.LogWarning("EnemyBehaviour: No EnemyType assigned!");
            return;
        }

        spriteRenderer.sprite = enemyType.EnemySprite;
        currentHealth = enemyType.Health;
        animator.runtimeAnimatorController = enemyType.AnimatorController;
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
        }
    }

    private void HandlePlayerMove()
    {
        if (!inBattle || activeSpell == null)
            return;

        activeSpell.Tick(grid, () =>
        {
            CreateAndExecuteSpell();
        });
    }

    public void DealDamage(DamageType damageType)
    {
        float damage = enemyType.CalculateDamageTaken(damageType.element, damageType.damage);

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        OnHitRecived?.Invoke(currentHealth);
        OnGetDamage?.Invoke(damageType);

        Debug.Log($"Enemy {enemyType.name} took {damage} damage. Health: {currentHealth}/{enemyType.Health}");

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

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnEnemyDead(); 
        }
        spriteRenderer.enabled = false;

        Debug.Log($"Enemy {enemyType.name} has been defeated!");
    }

    public float GetMaxHP() => enemyType.Health;
    public EnemyType GetEnemyType() => enemyType;
}