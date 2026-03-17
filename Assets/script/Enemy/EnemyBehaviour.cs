using UnityEngine;
using System.Collections.Generic;
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

    [Header("Tutorial")]
    [SerializeField] private TutorialManager tutorialManager;

    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private float currentHealth;
    private bool inBattle = false;

    private IEnemySpell activeSpell;
    public IEnemySpell ActiveSpell => activeSpell;
    private Coroutine timedSpellMonitor;

    private bool timersPaused;

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
            GameManager.Instance.CombatHandler.SetEnemyRef(this);
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
                    if (visual != null)
                    {
                        visual.SetSpriteVisible(true);
                    }
                }

            activeSpell.Cleanup(grid);
        }

        // Stop interval timer if present
        EnemySpellTimer enemyTimer = GetComponent<EnemySpellTimer>();
        if (enemyTimer != null)
            enemyTimer.StopTimer();

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

        if (activeSpell is TutorialStage2TimedSpell)
        {
            activeSpell.Execute(grid);
            timedSpellMonitor = StartCoroutine(MonitorTimedSpell());
            return;
        }

        // Don't recreate if it's an active MultipleTimedSpellBase (persists across grid states)
        if (activeSpell is MultipleTimedSpellBase existingMulti && existingMulti.IsActive)
            return;

        activeSpell = enemyType.CreateSpell();
        if (activeSpell != null)
        {
            if (activeSpell is MultipleTimedSpellBase newMultiSpell)
            {
                EnemySpellTimer enemyTimer = GetComponent<EnemySpellTimer>();
                if (enemyTimer == null)
                    enemyTimer = gameObject.AddComponent<EnemySpellTimer>();

                newMultiSpell.SetIntervalTimer(enemyTimer);
                newMultiSpell.Execute(grid);

                enemyTimer.Initialize(newMultiSpell.SelectionInterval, grid, newMultiSpell.OnIntervalTick);
                enemyTimer.StartTimer();

                timedSpellMonitor = StartCoroutine(MonitorMultipleTimedSpell());
            }
            else
            {
                activeSpell.Execute(grid);

                if (activeSpell is TimedDestroySpellBase)
                {
                    timedSpellMonitor = StartCoroutine(MonitorTimedSpell());
                }
            }
        }
    }

    private void HandlePlayerMove()
    {
        if (!inBattle || activeSpell == null)
            return;

        StartCoroutine(HandlePlayerMoveWhenGridReady());
    }

    private IEnumerator HandlePlayerMoveWhenGridReady()
    {
        // Wait for gravity, refill, and validation to fully complete
        while (grid != null && grid.CurrentState != GridStateType.Idle)
            yield return null;

        if (!inBattle || grid == null || activeSpell == null)
            yield break;

        if (activeSpell is MultipleTimedSpellBase multiSpell)
        {
            CheckForMultipleClearedTargets(multiSpell);
            yield break;
        }

        if (activeSpell is TimedDestroySpellBase timedSpell)
        {
            CheckForClearedTargets(timedSpell);
            yield break;
        }

        activeSpell.Tick(grid, () =>
        {
            StartCoroutine(RerollAndRetarget());
        });
    }

    private void CheckForClearedTargets(TimedDestroySpellBase timedSpell)
    {
        if (timedSpell.IsEffectApplied) return;

        bool anyCleared = timedSpell.ConsumeClearedCount() > 0;
        Debug.Log($" Cleared count: {anyCleared}");

        var positions = new List<Vector2Int>(timedSpell.TargetedPositions);
        Debug.Log($"Checking {positions.Count} targeted positions for clearing...");

        if (positions.Count == 0)
        {
            if (timedSpellMonitor != null)
            {
                StopCoroutine(timedSpellMonitor);
                timedSpellMonitor = null;
            }
            timedSpell.Cleanup(grid);
            StartCoroutine(RerollAndRetarget());
            return;
        }

        foreach (var pos in positions)
        {
            ElementVisual visual = grid.GetElementVisualAt(pos);
             
            if (visual == null || !visual.IsTargetedByEnemy)
            {
                bool allCleared = timedSpell.OnTargetCleared(grid, pos);
                anyCleared = true;

                if (allCleared)
                {
                    if (timedSpellMonitor != null)
                    {
                        StopCoroutine(timedSpellMonitor);
                        timedSpellMonitor = null;
                    }
                    timedSpell.Cleanup(grid);
                    StartCoroutine(RerollAndRetarget());
                    return;
                }
            }
        }
       // Re-apply targeting visuals to any new elements that fell into targeted positions
        timedSpell.ReapplyTargetingVisuals(grid);

        if (anyCleared && timedSpell is HiddenDestroySpell hiddenSpell && hiddenSpell.ElementsHidden)
        {
            hiddenSpell.TriggerBriefReveal(grid);
        }
    }

    private void CheckForMultipleClearedTargets(MultipleTimedSpellBase multiSpell)
    {
        if (!multiSpell.IsActive) return;

        var positions = new List<Vector2Int>(multiSpell.TargetedPositions);

        foreach (var pos in positions)
        {
            ElementVisual visual = grid.GetElementVisualAt(pos);
            if (visual == null || !visual.IsTargetedByEnemy)
            {
                multiSpell.OnTargetCleared(grid, pos);
            }
        }

        multiSpell.ReapplyTargetingVisuals(grid);
    }

    private IEnumerator MonitorTimedSpell()
    {
        TimedDestroySpellBase timedSpell = activeSpell as TimedDestroySpellBase;
        if (timedSpell == null) yield break;

        // Wait for timer to expire
        while (!timedSpell.IsTimerExpired)
        {
            if (!inBattle || grid == null)
                yield break;
            yield return null;
        }

        // Wait for grid to be idle before applying effect
        while (grid != null && grid.CurrentState != GridStateType.Idle)
            yield return null;

        if (!inBattle || grid == null)
            yield break;

        // Apply the effect
        bool effectDone = false;
        timedSpell.Tick(grid, () => effectDone = true);

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

        if (timedSpell.ShouldRerollAfterAttack)
            StartCoroutine(RerollAndRetarget());
        else
            CreateAndExecuteSpell();
    }

    private IEnumerator MonitorMultipleTimedSpell()
    {
        MultipleTimedSpellBase multiSpell = activeSpell as MultipleTimedSpellBase;
        if (multiSpell == null) yield break;

        while (multiSpell.IsActive && inBattle && grid != null)
        {
            // Wait until a per-target timer has expired
            if (!multiSpell.HasExpiredTargets)
            {
                yield return null;
                continue;
            }

            // Wait for grid idle before processing
            while (grid != null && grid.CurrentState != GridStateType.Idle)
                yield return null;

            if (!inBattle || grid == null || !multiSpell.IsActive)
                yield break;

            // Process one expired targe
            bool effectDone = false;
            multiSpell.ProcessNextExpiredTarget(grid, () => effectDone = true);

            while (!effectDone)
            {
                if (!inBattle || grid == null)
                    yield break;
                yield return null;
            }

            // Wait for grid to settle after gravity / refill
            while (grid != null && grid.CurrentState != GridStateType.Idle)
                yield return null;

            if (!inBattle || grid == null)
                yield break;

            // Remap positions and resume all timers
            multiSpell.OnPostEffectSettle(grid);
        }

        timedSpellMonitor = null;
    }

    private IEnumerator RerollAndRetarget()
    {
        if (grid == null || grid.VisualGrid == null)
        {
            CreateAndExecuteSpell();
            yield break;
        }

        if (activeSpell is HiddenDestroySpell hiddenSpell)
        {
            hiddenSpell.Cleanup(grid);
        }

        while (grid != null && grid.CurrentState != GridStateType.Idle)
            yield return null;

        if (!inBattle || grid == null)
            yield break;

        grid.Reroll(rerollScrollTimes);

        // Wait until reroll and subsequent validation finish
        while (grid != null && grid.CurrentState != GridStateType.Idle)
            yield return null;

        if (!inBattle || grid == null)
            yield break;

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
        deathParticles.Play();
        HapticsManager.Instance.GenerateHapticFeedback(HapticsManager.HapticTypes.MediumImpact);

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

    public void PauseActiveTimedSpell()
    {
        if (activeSpell is MultipleTimedSpellBase multiSpell)
        {
            multiSpell.PauseAllTargets();
            multiSpell.IntervalTimer?.Pause();
            timersPaused = true;
        }
        else if (activeSpell is TimedDestroySpellBase timedSpell)
        {
            timedSpell.PauseTimer();
            timersPaused = true;
        }
    }

    public void ResumeActiveTimedSpell()
    {
        if (activeSpell is MultipleTimedSpellBase multiSpell)
        {
            multiSpell.ResumeAllTargets();
            multiSpell.IntervalTimer?.Resume();
            timersPaused = false;
        }
        else if (activeSpell is TimedDestroySpellBase timedSpell)
        {
            timedSpell.ResumeTimer();
            timersPaused = false;
        }
    }

    private void Die()
    {
        StopBattle();

        deathParticles.Play();

        if (tutorialManager != null)
            tutorialManager.NotifyEnemyDeath();

        GameManager.Instance.CombatHandler.OnEnemyDead();

        spriteRenderer.enabled = false;

        Debug.Log($"Enemy {enemyType.name} has been defeated!");
    }

    public float GetMaxHP() => enemyType.Health;
    public EnemyType GetEnemyType() => enemyType;
}