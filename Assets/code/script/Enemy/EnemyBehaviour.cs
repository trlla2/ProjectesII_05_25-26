using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class EnemyBehaviour : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private EnemyType enemyType;
    [SerializeField] private Grid grid;

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem deathParticles;

    [Header("Reroll Settings")]
    [SerializeField] private int rerollScrollTimes = 3;

    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private float currentHealth;
    private bool inBattle = false;

    private IEnemySpell activeSpell;
    private Coroutine timedSpellMonitor;

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
        if (GameManager.Instance != null && enemyType == null)
        {
            GameManager.Instance.SetEnemyRef(this);
        }
        else
            Debug.LogError("NO GAME MANAGER");
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
        if (timedSpellMonitor != null)
        {
            StopCoroutine(timedSpellMonitor);
            timedSpellMonitor = null;
        }
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

            // For timed spells, monitor when the timer expires and auto-trigger Tick
            if (activeSpell is TimedDestroySpellBase)
            {
                timedSpellMonitor = StartCoroutine(MonitorTimedSpell());
            }
        }
    }

    private void HandlePlayerMove()
    {
        if (!inBattle || activeSpell == null)
            return;

        if (activeSpell is TimedDestroySpellBase timedSpell)
        {
            if (timedSpell.UseMoveTicks)
            {
                if (timedSpell.IsEffectApplied())
                    return;

                timedSpell.TickPlayerMove(grid, () =>
                {
                    if (timedSpell.IsEffectApplied())
                    {
                        // Stop the timer monitor since we're done
                        if (timedSpellMonitor != null)
                        {
                            StopCoroutine(timedSpellMonitor);
                            timedSpellMonitor = null;
                        }
                        StartCoroutine(WaitAndRetarget());
                    }
                });
            }
            // Pure timed spells don't tick on player moves
            return;
        }

        // Regular move-based spells
        activeSpell.Tick(grid, () =>
        {
            StartCoroutine(RerollAndRetarget());
        });
    }

    private IEnumerator WaitAndRetarget()
    {
        // Wait for grid to settle
        while (grid != null && grid.CurrentState != GridStateType.Idle)
            yield return null;

        if (!inBattle || grid == null)
            yield break;

        TimedDestroySpellBase timedSpell = activeSpell as TimedDestroySpellBase;
        if (timedSpell != null && timedSpell.ShouldRerollAfterAttack)
        {
            StartCoroutine(RerollAndRetarget());
        }
        else
        {
            CreateAndExecuteSpell();
        }
    }

    private IEnumerator MonitorTimedSpell()
    {
        TimedDestroySpellBase timedSpell = activeSpell as TimedDestroySpellBase;
        if (timedSpell == null) yield break;

        // Wait until the timer has expired or effect was already applied (by moves)
        while (!timedSpell.IsTimerExpired() && !timedSpell.IsEffectApplied())
        {
            if (!inBattle || grid == null)
                yield break;
            yield return null;
        }

        // If effect was already applied by moves, exit
        if (timedSpell.IsEffectApplied())
        {
            timedSpellMonitor = null;
            yield break;
        }

        if (!inBattle || grid == null)
            yield break;

        // This prevents colliding with a player-triggered refill in progress.
        while (grid != null && grid.CurrentState != GridStateType.Idle)
            yield return null;

        if (!inBattle || grid == null)
            yield break;

        bool effectDone = false;
        timedSpell.Tick(grid, () => effectDone = true);

        //Wait for the effect to finish processing
        while (!effectDone)
        {
            if (!inBattle || grid == null)
                yield break;
            yield return null;
        }

        // Wait for grid to settle
        while (grid != null && grid.CurrentState != GridStateType.Idle)
            yield return null;

        if (!inBattle || grid == null)
            yield break;

        timedSpellMonitor = null;

        // Only reroll and retarget if the spell wants to
        if (timedSpell.ShouldRerollAfterAttack)
        {
            StartCoroutine(RerollAndRetarget());
        }
        else
        {
            CreateAndExecuteSpell();
        }
    }

    private IEnumerator RerollAndRetarget()
    {
        if (grid == null || grid.VisualGrid == null)
        {
            CreateAndExecuteSpell();
            yield break;
        }
        grid.Reroll(rerollScrollTimes);

        while (grid.CurrentState != GridStateType.Idle)
        {
            yield return null;
        }

        CreateAndExecuteSpell();
    }
    public void dealBossDamage(float percent)
    {
        float damage = enemyType.Health * percent;
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        OnHitRecived?.Invoke(currentHealth);
        OnGetDamage?.Invoke(new DamageType(ElementType.EMPTY, damage));

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void DealDamage(DamageType damageType)
    {
        deathParticles.Play(); // NO FUNCIONA
        float damage = enemyType.CalculateDamageTaken(damageType.element, damageType.damage);

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        OnHitRecived?.Invoke(currentHealth);
        OnGetDamage?.Invoke(new DamageType(damageType.element, damage, damageType.IsCrit));

        

        if (currentHealth <= 0)
        {
            Die();
        }
        
    }

    private void Die()
    {
        StopBattle();

        deathParticles.Play();

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