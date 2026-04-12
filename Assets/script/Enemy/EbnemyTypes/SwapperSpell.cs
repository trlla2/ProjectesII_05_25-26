using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class SwapperSpell : EnemySpellBase
{
    public static (Vector2Int, Vector2Int) NULLPAIR = (new Vector2Int(-1, -1), new Vector2Int(-1, -1));

    protected class SwapPairInfo
    {
        public int Id;
        public Vector2Int PositionA;
        public Vector2Int PositionB;
        public SpellTimer Timer;
        public bool Fired;
        public bool IsPaused;
    }

    private Dictionary<int, SwapPairInfo> activePairs = new Dictionary<int, SwapPairInfo>();
    private Queue<int> expiredQueue = new Queue<int>();
    private int nextPairId;
    private bool isProcessingEffect;

    protected Grid cachedGrid;
    protected float perPairCountdown;
    protected float bonusTimePerClearedPair;
    private float selectionInterval;
    private EnemySpellTimer intervalTimer;

    private const float SwapAnimDuration = 0.2f;

    public bool IsActive { get; private set; }
    public float SelectionInterval => selectionInterval;
    public EnemySpellTimer IntervalTimer => intervalTimer;
    public bool HasExpiredPairs => expiredQueue.Count > 0;
    public bool IsProcessingEffect => isProcessingEffect;
    public List<Vector2Int> TargetedPositions => targetedPositions;

    public SwapperSpell(float interval = 2.5f, float countdown = 6f, float damage = 4f, float bonusTime = 0f)
    {
        selectionInterval = interval;
        perPairCountdown = countdown;
        damagePerElement = damage;
        bonusTimePerClearedPair = bonusTime;
        remainingMoves = 0;
        initialMoves = 0;
        shouldRerollAfterAttack = false;
        spellName = "Swapper";
    }

    public void SetIntervalTimer(EnemySpellTimer timer)
    {
        intervalTimer = timer;
    }

    public override void Execute(Grid grid)
    {
        base.Execute(grid);
        cachedGrid = grid;
        IsActive = true;
        isProcessingEffect = false;
        targetedPositions.Clear();
        activePairs.Clear();
        expiredQueue.Clear();
        nextPairId = 0;

        if (grid == null) return;

        // Select first pair immediately
        AddNewSwapPair(grid);
    }

    public void OnIntervalTick()
    {
        if (!IsActive || cachedGrid == null) return;
        AddNewSwapPair(cachedGrid);
    }

    protected void AddNewSwapPair(Grid grid)
    {
        var pair = PickSwapPair(grid);
        if (pair == NULLPAIR) return;

        int id = nextPairId++;
        SpellTimer timer = new SpellTimer(perPairCountdown);

        var info = new SwapPairInfo
        {
            Id = id,
            PositionA = pair.Item1,
            PositionB = pair.Item2,
            Timer = timer,
            Fired = false,
            IsPaused = false
        };

        activePairs[id] = info;
        targetedPositions.Add(info.PositionA);
        targetedPositions.Add(info.PositionB);

        // Set visuals for both elements AND mark them as swap-targeted to prevent gravity
        ElementVisual visualA = grid.GetElementVisualAt(info.PositionA);
        ElementVisual visualB = grid.GetElementVisualAt(info.PositionB);

        if (visualA != null)
        {
            visualA.SetTargetedByEnemy(true);
            visualA.StartTimedCountdown(perPairCountdown);
            visualA.PlaySwapWarning(perPairCountdown);
        }
        if (visualB != null)
        {
            visualB.SetTargetedByEnemy(true);
            visualB.StartTimedCountdown(perPairCountdown);
            visualB.PlaySwapWarning(perPairCountdown);
        }

        int capturedId = id;
        SpellTimer capturedTimer = timer;

        timer.OnTimerExpired += () => OnPairExpired(capturedId);
        timer.OnTimeAdded += newRemaining =>
        {
            if (cachedGrid == null || !activePairs.TryGetValue(capturedId, out SwapPairInfo p) || p.Fired) return;
            ElementVisual vA = cachedGrid.GetElementVisualAt(p.PositionA);
            ElementVisual vB = cachedGrid.GetElementVisualAt(p.PositionB);
            vA?.ExtendTimedCountdown(newRemaining, capturedTimer.TotalSeconds);
            vB?.ExtendTimedCountdown(newRemaining, capturedTimer.TotalSeconds);
        };

        timer.Start();
    }

    private void OnPairExpired(int id)
    {
        if (!activePairs.ContainsKey(id) || activePairs[id].Fired) return;
        expiredQueue.Enqueue(id);
    }

    public void ProcessNextExpiredPair(Grid grid, System.Action onComplete)
    {
        while (expiredQueue.Count > 0)
        {
            int id = expiredQueue.Dequeue();
            if (!activePairs.TryGetValue(id, out SwapPairInfo info) || info.Fired)
                continue;

            info.Fired = true;
            isProcessingEffect = true;

            // Stop countdowns for both positions
            grid.GetElementVisualAt(info.PositionA)?.StopTimedCountdown();
            grid.GetElementVisualAt(info.PositionB)?.StopTimedCountdown();

            // DON'T pause all pairs here - let grid state changes handle it
            grid.StartCoroutine(ExecuteSwapAndAttack(info, onComplete));
            return;
        }

        onComplete?.Invoke();
    }

    private IEnumerator ExecuteSwapAndAttack(SwapPairInfo info, System.Action onComplete)
    {
        Vector2Int posA = info.PositionA;
        Vector2Int posB = info.PositionB;

        Element elementA = cachedGrid.GetElementAt(posA);
        Element elementB = cachedGrid.GetElementAt(posB);
        ElementVisual visualA = cachedGrid.GetElementVisualAt(posA);
        ElementVisual visualB = cachedGrid.GetElementVisualAt(posB);

        bool validA = elementA != null && elementA != cachedGrid.EmptyElement && visualA != null && visualA.IsTargetedByEnemy;
        bool validB = elementB != null && elementB != cachedGrid.EmptyElement && visualB != null && visualB.IsTargetedByEnemy;

        // Clear targeting visuals and swap-targeted flag
        if (visualA != null)
        {
            visualA.SetTargetedByEnemy(false);
            visualA.StopHoverAnimation();
        }
        if (visualB != null)
        {
            visualB.SetTargetedByEnemy(false);
            visualB.StopHoverAnimation();
        }

        targetedPositions.Remove(posA);
        targetedPositions.Remove(posB);

        if (validA && validB)
        {
            // Perform swap animation
            var swapPairs = new List<(Vector2Int, Vector2Int)> { (posA, posB) };
            cachedGrid.ProcessEnemySwap(swapPairs, SwapAnimDuration);

            // Wait for swap to complete
            yield return WaitForIdle();

            if (!IsStillValid())
            {
                CleanupPair(info.Id);
                isProcessingEffect = false;
                onComplete?.Invoke();
                yield break;
            }

            // Attack player with both elements (use swapped positions)
            List<Vector2Int> positions = new List<Vector2Int> { posA, posB };
            List<Element> elements = new List<Element> { elementA, elementB };

            bool attackDone = false;
            cachedGrid.ProcessEnemySpell(positions, () =>
            {
                DamagePlayerWithAnimation(cachedGrid, positions.ToArray(), elements, damagePerElement, () =>
                {
                    attackDone = true;
                });
            }, false);

            while (!attackDone)
            {
                if (!IsStillValid())
                {
                    CleanupPair(info.Id);
                    isProcessingEffect = false;
                    onComplete?.Invoke();
                    yield break;
                }
                yield return null;
            }
        }

        CleanupPair(info.Id);
        isProcessingEffect = false;
        onComplete?.Invoke();
    }

    private IEnumerator WaitForIdle()
    {
        while (IsActive && cachedGrid != null && cachedGrid.CurrentState != GridStateType.Idle)
            yield return null;
    }

    private bool IsStillValid()
    {
        return IsActive && cachedGrid != null;
    }

    public void OnPostEffectSettle(Grid grid)
    {
        // Swap pairs are position-based and don't follow gravity
        // Just reapply visuals and resume
        ReapplyTargetingVisuals(grid);
        ResumeAllPairs();
        intervalTimer?.Resume();
    }

    public void PauseAllPairs()
    {
        foreach (var kvp in activePairs)
        {
            if (!kvp.Value.Fired && !kvp.Value.IsPaused)
            {
                kvp.Value.Timer?.Pause();
                kvp.Value.IsPaused = true;
                
                if (cachedGrid != null)
                {
                    cachedGrid.GetElementVisualAt(kvp.Value.PositionA)?.PauseTimedCountdown();
                    cachedGrid.GetElementVisualAt(kvp.Value.PositionB)?.PauseTimedCountdown();
                }
            }
        }
    }

    public void ResumeAllPairs()
    {
        foreach (var kvp in activePairs)
        {
            if (!kvp.Value.Fired && kvp.Value.IsPaused)
            {
                kvp.Value.Timer?.Resume();
                kvp.Value.IsPaused = false;
                
                if (cachedGrid != null)
                {
                    cachedGrid.GetElementVisualAt(kvp.Value.PositionA)?.ResumeTimedCountdown();
                    cachedGrid.GetElementVisualAt(kvp.Value.PositionB)?.ResumeTimedCountdown();
                }
            }
        }
    }

    public void AddTimeToAll(float seconds)
    {
        foreach (var kvp in activePairs)
        {
            if (!kvp.Value.Fired)
                kvp.Value.Timer?.AddTime(seconds);
        }
    }

    public bool OnTargetCleared(Grid grid, Vector2Int clearedPosition)
    {
        int clearedId = -1;
        foreach (var kvp in activePairs)
        {
            if (!kvp.Value.Fired &&
                (kvp.Value.PositionA == clearedPosition || kvp.Value.PositionB == clearedPosition))
            {
                clearedId = kvp.Key;
                break;
            }
        }

        if (clearedId == -1) return false;

        SwapPairInfo info = activePairs[clearedId];

        // Stop both timers and visuals
        if (cachedGrid != null)
        {
            ElementVisual visualA = cachedGrid.GetElementVisualAt(info.PositionA);
            ElementVisual visualB = cachedGrid.GetElementVisualAt(info.PositionB);

            if (visualA != null)
            {
                visualA.StopTimedCountdown();
                visualA.StopSwapWarning();
                visualA.SetTargetedByEnemy(false);
            }
            if (visualB != null)
            {
                visualB.StopTimedCountdown();
                visualB.StopSwapWarning();
                visualB.SetTargetedByEnemy(false);
            }
        }

        targetedPositions.Remove(info.PositionA);
        targetedPositions.Remove(info.PositionB);
        CleanupPair(clearedId);

        if (bonusTimePerClearedPair > 0f)
            AddTimeToAll(bonusTimePerClearedPair);

        return GetActivePairCount() == 0;
    }

    public int GetActivePairCount()
    {
        int count = 0;
        foreach (var kvp in activePairs)
        {
            if (!kvp.Value.Fired) count++;
        }
        return count;
    }

    public override void RetargetAfterGravity(Grid grid)
    {
        // Swap pairs stay at fixed positions
        if (grid == null) return;
        ReapplyTargetingVisuals(grid);
    }

    /// Reapply targeting visuals to elements at pair positions.
    /// If a position now has a valid element, mark it as targeted.
    /// If a position is empty or invalid, cancel the pair.
    public void ReapplyTargetingVisuals(Grid grid)
    {
        if (grid == null) return;

        var pairsToCancel = new List<int>();

        foreach (var kvp in activePairs)
        {
            if (kvp.Value.Fired) continue;

            SwapPairInfo info = kvp.Value;
            float remaining = info.Timer.RemainingSeconds;

            Element elA = grid.GetElementAt(info.PositionA);
            Element elB = grid.GetElementAt(info.PositionB);
            ElementVisual visualA = grid.GetElementVisualAt(info.PositionA);
            ElementVisual visualB = grid.GetElementVisualAt(info.PositionB);

            bool validA = elA != null && elA != grid.EmptyElement && visualA != null;
            bool validB = elB != null && elB != grid.EmptyElement && visualB != null;

            // If either position became invalid cancel the pair
            if (!validA || !validB)
            {
                if (visualA != null)
                {
                    visualA.StopTimedCountdown();
                    visualA.StopSwapWarning();
                    visualA.SetTargetedByEnemy(false);
                }
                if (visualB != null)
                {
                    visualB.StopTimedCountdown();
                    visualB.StopSwapWarning();
                    visualB.SetTargetedByEnemy(false);
                }
                pairsToCancel.Add(kvp.Key);
                continue;
            }

            // Reapply visuals and swap-targeted flag if needed
            if (!visualA.IsTargetedByEnemy)
            {
                visualA.SetTargetedByEnemy(true);
                visualA.StartTimedCountdown(remaining);
                visualA.PlaySwapWarning(remaining);
            }
            if (!visualB.IsTargetedByEnemy)
            {
                visualB.SetTargetedByEnemy(true);
                visualB.StartTimedCountdown(remaining);
                visualB.PlaySwapWarning(remaining);
            }
        }

        // Cancel invalid pairs
        foreach (int id in pairsToCancel)
        {
            if (activePairs.TryGetValue(id, out SwapPairInfo info))
            {
                targetedPositions.Remove(info.PositionA);
                targetedPositions.Remove(info.PositionB);
            }
            CleanupPair(id);
        }

        if (pairsToCancel.Count > 0 && bonusTimePerClearedPair > 0f)
            AddTimeToAll(bonusTimePerClearedPair * pairsToCancel.Count);
    }

    private (Vector2Int, Vector2Int) PickSwapPair(Grid grid)
    {
        var used = new HashSet<Vector2Int>(targetedPositions);
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        List<Vector2Int> candidates = new List<Vector2Int>();
        for (int x = 0; x < Grid.GridSize; x++)
        {
            for (int y = 0; y < Grid.GridSize; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (used.Contains(pos)) continue;
                if (!IsPositionSwappable(pos)) continue;
                candidates.Add(pos);
            }
        }

        if (candidates.Count == 0) return NULLPAIR;

        // Shuffle candidates
        for (int j = candidates.Count - 1; j > 0; j--)
        {
            int k = Random.Range(0, j + 1);
            (candidates[j], candidates[k]) = (candidates[k], candidates[j]);
        }

        foreach (var pos in candidates)
        {
            List<Vector2Int> adjacentCandidates = new List<Vector2Int>();
            foreach (var dir in directions)
            {
                Vector2Int adj = pos + dir;
                if (used.Contains(adj)) continue;
                if (!IsPositionSwappable(adj)) continue;
                adjacentCandidates.Add(adj);
            }

            if (adjacentCandidates.Count > 0)
            {
                Vector2Int adj = adjacentCandidates[Random.Range(0, adjacentCandidates.Count)];
                return (pos, adj);
            }
        }

        return NULLPAIR;
    }

    private bool IsPositionSwappable(Vector2Int pos)
    {
        if (!cachedGrid.IsPositionValid(pos)) return false;

        Element el = cachedGrid.GetElementAt(pos);
        if (el == null || el == cachedGrid.EmptyElement) return false;

        ElementVisual vis = cachedGrid.GetElementVisualAt(pos);
        if (vis != null && vis.IsBlocked) return false;

        return true;
    }

    public override void Tick(Grid grid, System.Action onComplete = null)
    {
        onComplete?.Invoke();
    }

    public override void Cleanup(Grid grid)
    {
        IsActive = false;
        intervalTimer?.StopTimer();

        foreach (var kvp in activePairs)
        {
            kvp.Value.Timer?.Stop();
            if (grid != null)
            {
                ElementVisual visualA = grid.GetElementVisualAt(kvp.Value.PositionA);
                ElementVisual visualB = grid.GetElementVisualAt(kvp.Value.PositionB);

                if (visualA != null)
                {
                    visualA.ClearTargetVisual();
                    visualA.StopTimedCountdown();
                    visualA.SetTargetedByEnemy(false);
                }
                if (visualB != null)
                {
                    visualB.ClearTargetVisual();
                    visualB.StopTimedCountdown();
                    visualB.SetTargetedByEnemy(false);
                }
            }
        }

        activePairs.Clear();
        expiredQueue.Clear();
        targetedPositions.Clear();
        cachedGrid = null;
    }

    private void CleanupPair(int id)
    {
        if (activePairs.TryGetValue(id, out SwapPairInfo info))
        {
            info.Timer?.Stop();
            activePairs.Remove(id);
        }
    }
}