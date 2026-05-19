using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;


/* l'Enemy behaviour es el cript de escene que controla el comportament de l'enemic, com els seus atacs, la seva vida, i la interacció amb el grid.
 * Cada enemic implementa el seu attack a través d'un EnemyType, que és un ScriptableObject que defineix les característiques de l'enemic i el seu atac.
 * cada enemyType te asociat un enemySpell, que implementa la interfície IEnemySpell, i defineix la lògica de l'atac (quines caselles ataca, quant de temps tarda, etc).
 * Tenim Enemics de moviments (Basats en EnemySpellbase), enemics de temps(SingleTimedDestroySpell), enemics de temps múltiples (MultipleTimedSpellBase), enemics bloquejants (BlockerSpell) i enemic swapper (SwapperSpell).
 */
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

    [Header("VFX")]
    [SerializeField] private ParticleSystem hiddenEnemyAttackSelect;

    private SpriteRenderer spriteRenderer;
    [HideInInspector]
    public Animator animator;

    private float currentHealth;
    private bool inBattle = false;

    private IEnemySpell activeSpell;
    public IEnemySpell ActiveSpell => activeSpell;
    private Coroutine timedSpellMonitor;

    public Grid GetGrid() => grid;

    private bool timersPaused;
    private bool isFrozen;

    //Legacy System
    public delegate void GetOnHitRecived(float health);
    public event GetOnHitRecived OnHitRecived;

    public delegate void GetOnDamageList(DamageType damageType);
    public event GetOnDamageList OnGetDamage;

    public System.Action OnEnemyDeath;
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

        if (grid != null)
            grid.OnStateChanged += HandleGridStateChanged;
    }

    public void StopBattle()
    {
        if (!inBattle)
            return;

        inBattle = false;
        if (GameManager.Instance != null)
            GameManager.Instance.OnPlayerMove -= HandlePlayerMove;

        if (grid != null)
            grid.OnStateChanged -= HandleGridStateChanged;

        if (activeSpell != null && grid != null)
        {
            for (int x = 0; x < grid.GridSize; x++)
                for (int y = 0; y < grid.GridSize; y++)
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
                        // Clear any blocked state
                        if (visual.IsBlocked)
                        {
                            visual.SetBlocked(false);
                        }
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

        // Don't recreate if it's an active MultipleTimedSpellBase
        if (activeSpell is MultipleTimedSpellBase existingMulti && existingMulti.IsActive)
            return;

        // Same in blocker spell
        if (activeSpell is BlockerSpell existingBlocker && existingBlocker.IsActive)
            return;

        // Same in swapper spell
        if (activeSpell is SwapperSpell existingSwapper && existingSwapper.IsActive)
            return;

        HandleTargetVFX();  
        activeSpell = enemyType.CreateSpell();
        if (activeSpell != null)
        {
            if (activeSpell is MultipleTimedSpellBase newMultiSpell)
            {
                EnemySpellTimer enemyTimer = GetComponent<EnemySpellTimer>();
                if (enemyTimer == null)
                    Debug.LogError("NO ENEMY TIMER!");

                newMultiSpell.SetIntervalTimer(enemyTimer);
                newMultiSpell.Execute(grid);

                enemyTimer.Initialize(newMultiSpell.SelectionInterval, grid, newMultiSpell.OnIntervalTick);
                enemyTimer.StartTimer();

                timedSpellMonitor = StartCoroutine(MonitorMultipleTimedSpell());
            }
            else if (activeSpell is BlockerSpell blockerSpell)
            {
                blockerSpell.Execute(grid);
                timedSpellMonitor = StartCoroutine(MonitorBlockerSpell());
            }
            else if (activeSpell is SwapperSpell newSwapperSpell)
            {
                EnemySpellTimer enemyTimer = GetComponent<EnemySpellTimer>();
                if (enemyTimer == null)
                    Debug.LogError("NO ENEMY TIMER!");

                newSwapperSpell.SetIntervalTimer(enemyTimer);
                newSwapperSpell.Execute(grid);

                enemyTimer.Initialize(newSwapperSpell.SelectionInterval, grid, newSwapperSpell.OnIntervalTick);
                enemyTimer.StartTimer();

                timedSpellMonitor = StartCoroutine(MonitorSwapperSpell());
            }
            else
            {
                activeSpell.Execute(grid);

                if (activeSpell is TimedDestroySpellBase)
                {
                    timedSpellMonitor = StartCoroutine(MonitorTimedSpell());
                }
            }

            //If a freeze is still active when a new spell is spawned, pause 
            if (isFrozen)
                PauseActiveTimedSpell();
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

        // BombSpell and DoubleBomberSpell need to wait for secondary effect processing
        // (e.g. thunder, item effects) to fully complete before checking cable count / deciding to explode.
        if (activeSpell is BombSpell || activeSpell is DoubleBomberSpell)
        {
            yield return null; // wait one frame first to ensure no state fights
            while (grid != null && grid.CurrentState != GridStateType.Idle)
                yield return null;
        }

        if (!inBattle || grid == null || activeSpell == null)
            yield break;

        if (activeSpell is MultipleTimedSpellBase multiSpell)
        {
            CheckForMultipleClearedTargets(multiSpell);
            yield break;
        }

        if (activeSpell is BlockerSpell blockerSpell)
        {
            blockerSpell.CheckForClearedAttackTarget(grid);
            yield break;
        }

        // SwapperSpell no longer needs player move handling - it's fully autonomous
        if (activeSpell is SwapperSpell swapperSpell)
        {
            CheckForSwapperClearedTargets(swapperSpell);
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

        var positions = new List<Vector2Int>(timedSpell.TargetedPositions);

        if (positions.Count == 0)
        {
            if (timedSpellMonitor != null)
            {
                StopCoroutine(timedSpellMonitor);
                timedSpellMonitor = null;
            }

            timedSpell.Cleanup(grid);

            if (timedSpell is SingleElementTimedSpell)
            {
                CreateAndExecuteSpell();
                return;
            }

            StartCoroutine(RerollAndRetarget());
            return;
        }

        foreach (var pos in positions)
        {
            ElementVisual visual = grid.GetElementVisualAt(pos);
             
            if (visual == null || !visual.IsTargetedByEnemy)
            {
                timedSpell.OnTargetCleared(grid, pos);
                anyCleared = true;

                bool allCleared = timedSpell.TargetedPositions.Count == 0;

                if (allCleared)
                {
                    if (timedSpellMonitor != null)
                    {
                        StopCoroutine(timedSpellMonitor);
                        timedSpellMonitor = null;
                    }

                    timedSpell.Cleanup(grid);

                    if (timedSpell is SingleElementTimedSpell)
                    {
                        CreateAndExecuteSpell();
                        return;
                    }

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

    private void CheckForSwapperClearedTargets(SwapperSpell swapperSpell)
    {
        if (!swapperSpell.IsActive) return;

        var positions = new List<Vector2Int>(swapperSpell.TargetedPositions);

        foreach (var pos in positions)
        {
            ElementVisual visual = grid.GetElementVisualAt(pos);
            if (visual == null || !visual.IsTargetedByEnemy)
            {
                swapperSpell.OnTargetCleared(grid, pos);
            }
        }

        swapperSpell.ReapplyTargetingVisuals(grid);
    }

    private void HandleTargetVFX()
    {
        if (activeSpell == null || grid == null) return;
        if (activeSpell is HiddenDestroySpell)
        {
            if (hiddenEnemyAttackSelect != null)
            {
                hiddenEnemyAttackSelect.Play();
            }
        }
    }

    //com que vaig fer la decisio de disseny de basar els enemics amb un sistema de "Ticks",
    //on cada atac del jugador te progrés en el attack del enemic, necessito aquest sistema de monitorització per a controlar el flux de cada atac segons el seu tipus i les seves necessitats.
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

    private IEnumerator MonitorBlockerSpell()
    {
        BlockerSpell blockerSpell = activeSpell as BlockerSpell;
        if (blockerSpell == null) yield break;

        while (blockerSpell.IsActive && inBattle && grid != null)
        {
            // Monitor the inner timed-attack spell for expiry
            if (blockerSpell.HasActiveAttackSpell && blockerSpell.IsAttackTimerExpired)
            {
                // Wait for grid to be idle before processing the attack
                while (grid != null && grid.CurrentState != GridStateType.Idle)
                    yield return null;

                if (!inBattle || grid == null || !blockerSpell.IsActive)
                    yield break;

                // Process the expired attack (deal damage, destroy element)
                bool effectDone = false;
                blockerSpell.ProcessExpiredAttack(grid, () => effectDone = true);

                while (!effectDone)
                {
                    if (!inBattle || grid == null)
                        yield break;
                    yield return null;
                }

                // Wait for grid to settle after gravity / refill
                while (grid != null && grid.CurrentState != GridStateType.Idle)
                    yield return null;

                if (!inBattle || grid == null || !blockerSpell.IsActive)
                    yield break;

                // Retarget a fresh single element
                blockerSpell.RestartAttackSpell(grid);
            }

            yield return null;
        }

        timedSpellMonitor = null;
    }

    private IEnumerator MonitorSwapperSpell()
    {
        SwapperSpell swapperSpell = activeSpell as SwapperSpell;
        if (swapperSpell == null) yield break;

        while (swapperSpell.IsActive && inBattle && grid != null)
        {
            if (!swapperSpell.HasExpiredPairs)
            {
                yield return null;
                continue;
            }

            // Wait for grid idle before processing
            while (grid != null && grid.CurrentState != GridStateType.Idle)
                yield return null;

            if (!inBattle || grid == null || !swapperSpell.IsActive)
                yield break;

            // Process one expired pair
            bool effectDone = false;
            swapperSpell.ProcessNextExpiredPair(grid, () => effectDone = true);

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

            // Remap positions and resume all timers
            swapperSpell.OnPostEffectSettle(grid);
        }

        timedSpellMonitor = null;
    }

    public void NotifyAdjacentElementsUsed(List<Vector2Int> usedPositions)
    {
        if (!inBattle || activeSpell == null || grid == null)
            return;

        if (activeSpell is BlockerSpell blockerSpell)
        {
            bool allBroken = blockerSpell.OnAdjacentElementsUsed(grid, usedPositions);
            
            if (allBroken)
            {
                StartCoroutine(HandleBlockBroken(blockerSpell));
            }
        }
    }

    private IEnumerator HandleBlockBroken(BlockerSpell blockerSpell)
    {
        // Wait for grid to be idle before starting reroll
        while (grid != null && grid.CurrentState != GridStateType.Idle)
            yield return null;

        if (!inBattle || grid == null)
            yield break;

        // Wait for animation to complete before triggering reroll
        bool animationComplete = false;
        blockerSpell.OnAllBlocksBroken(grid, () => animationComplete = true);

        // Wait for the animation to finish
        while (!animationComplete)
        {
            if (!inBattle || grid == null)
                yield break;
            yield return null;
        }

        // Now trigger the reroll
        grid.Reroll(2);

        // Wait for reroll to complete
        while (grid != null && grid.CurrentState != GridStateType.Idle)
            yield return null;

        if (!inBattle || grid == null)
            yield break;

        // Place new blocks and start a new attack spell
        blockerSpell.OnRerollComplete(grid);
    }

    public HashSet<Vector2Int> GetBlockedPositions()
    {
        if (activeSpell is BlockerSpell blockerSpell)
        {
            return blockerSpell.GetBlockedPositionsSet();
        }
        if(activeSpell is HiddenDestroySpell hiddenSpell)
        {
            if (hiddenSpell.ElementsHidden)
            {
                return hiddenSpell.TargetedPositions.ToHashSet();
            }

        }
        return new HashSet<Vector2Int>();
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
        if (activeSpell is BlockerSpell)
        {
            return;
        }

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
        if (activeSpell is BlockerSpell blockerSpell)
        {
            blockerSpell.PauseAttackTimer();
            timersPaused = true;
        }
        else if (activeSpell is SwapperSpell swapperSpell)
        {
            swapperSpell.PauseAllPairs();
            swapperSpell.IntervalTimer?.Pause();
            timersPaused = true;
        }
        else if (activeSpell is MultipleTimedSpellBase multiSpell)
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
        if (isFrozen) return;

        if (activeSpell is BlockerSpell blockerSpell)
        {
            blockerSpell.ResumeAttackTimer();
            timersPaused = false;
        }
        else if (activeSpell is SwapperSpell swapperSpell)
        {
            swapperSpell.ResumeAllPairs();
            swapperSpell.IntervalTimer?.Resume();
            timersPaused = false;
        }
        else if (activeSpell is MultipleTimedSpellBase multiSpell)
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

    public void FreezeForDuration(float duration)
    {
        StartCoroutine(FreezeCoroutine(duration));
    }

    private IEnumerator FreezeCoroutine(float duration)
    {
        isFrozen = true;
        PauseActiveTimedSpell();
        yield return new WaitForSeconds(duration);
        isFrozen = false;
        ResumeActiveTimedSpell();
    }

    private void Die()
    {
        StopBattle();

        OnEnemyDeath?.Invoke();

        deathParticles.Play();

        if (tutorialManager != null)
            tutorialManager.NotifyEnemyDeath();

        GameManager.Instance.CombatHandler.OnEnemyDead();

        Debug.Log($"Enemy {enemyType.name} has been defeated!");
    }

    public float GetMaxHP() => enemyType.Health;
    public EnemyType GetEnemyType() => enemyType;

    private void HandleGridStateChanged(GridStateType newState)
    {
        if (!inBattle || activeSpell == null) return;

        if (newState == GridStateType.Idle)
            ResumeActiveTimedSpell();
        else
            PauseActiveTimedSpell();
    }

    private void Update()
    {
        if (!inBattle || activeSpell == null || grid == null) return;

        // Drive visual updates from spell timers every frame
        UpdateSpellVisuals();
    }

    private void UpdateSpellVisuals()
    {
        if (timersPaused || isFrozen) return;

        if (activeSpell is TimedDestroySpellBase timedSpell)
        {
            timedSpell.UpdateVisuals();
        }
        else if (activeSpell is MultipleTimedSpellBase multiSpell)
        {
            multiSpell.UpdateVisuals();
        }
        else if (activeSpell is BlockerSpell blockerSpell)
        {
            blockerSpell.UpdateVisuals();
        }
        else if (activeSpell is SwapperSpell swapperSpell)
        {
            swapperSpell.UpdateVisuals();
        }
    }
}