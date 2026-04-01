using System.Collections.Generic;
using UnityEngine;


//This enemy base is the same as timer enemy but with indivisual timer for each target position
public abstract class MultipleTimedSpellBase : EnemySpellBase
{
    protected class TargetInfo
    {
        public int Id;
        public Vector2Int Position;
        public SpellTimer Timer;
        public bool Fired;
    }

    private Dictionary<int, TargetInfo> activeTargets = new Dictionary<int, TargetInfo>();
    private Queue<int> expiredQueue = new Queue<int>();
    private int nextTargetId;
    private bool isProcessingEffect;

    protected Grid cachedGrid;
    protected float perTargetCountdown;
    protected float bonusTimePerClearedTarget;
    private float selectionInterval;
    private EnemySpellTimer intervalTimer;

    public bool IsActive { get; private set; }
    public float SelectionInterval => selectionInterval;
    public EnemySpellTimer IntervalTimer => intervalTimer;
    public bool HasExpiredTargets => expiredQueue.Count > 0;
    public bool IsProcessingEffect => isProcessingEffect;
    public List<Vector2Int> TargetedPositions => targetedPositions;

    protected MultipleTimedSpellBase(float interval, float countdown, float damage, float bonusTime = 0f)
    {
        selectionInterval = interval;
        perTargetCountdown = countdown;
        damagePerElement = damage;
        bonusTimePerClearedTarget = bonusTime;
        remainingMoves = 0;
        initialMoves = 0;
        shouldRerollAfterAttack = false;
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
        activeTargets.Clear();
        expiredQueue.Clear();
        nextTargetId = 0;

        if (grid == null) return;

        // Select first target immediately
        AddNewTarget(grid);
    }

    public void OnIntervalTick()
    {
        if (!IsActive || cachedGrid == null) return;
        AddNewTarget(cachedGrid);
    }

    protected void AddNewTarget(Grid grid)
    {
        Vector2Int pick = PickTargetPosition(grid);
        if (pick.x == -1) return;

        int id = nextTargetId++;
        SpellTimer timer = new SpellTimer(perTargetCountdown);

        var info = new TargetInfo
        {
            Id = id,
            Position = pick,
            Timer = timer,
            Fired = false
        };

        activeTargets[id] = info;
        targetedPositions.Add(pick);

        ElementVisual visual = grid.GetElementVisualAt(pick);
        if (visual != null)
        {
            visual.SetTargetedByEnemy(true);
            visual.StartTimedCountdown(perTargetCountdown);
        }

        int capturedId = id;
        SpellTimer capturedTimer = timer;
        timer.OnTimerExpired += () => OnTargetExpired(capturedId);
        timer.OnTimeAdded += newRemaining =>
        {
            if (cachedGrid == null || !activeTargets.TryGetValue(capturedId, out TargetInfo t) || t.Fired) return;
            ElementVisual v = cachedGrid.GetElementVisualAt(t.Position);
            v?.ExtendTimedCountdown(newRemaining, capturedTimer.TotalSeconds);
        };
        timer.Start();
    }

    private void OnTargetExpired(int id)
    {
        if (!activeTargets.ContainsKey(id) || activeTargets[id].Fired) return;
        expiredQueue.Enqueue(id);
    }

    public void ProcessNextExpiredTarget(Grid grid, System.Action onComplete)
    {
        // Skip targets that were already cleared by the player
        while (expiredQueue.Count > 0)
        {
            int id = expiredQueue.Dequeue();
            if (!activeTargets.TryGetValue(id, out TargetInfo info) || info.Fired)
                continue;

            info.Fired = true;
            isProcessingEffect = true;
            Vector2Int pos = info.Position;

            grid.GetElementVisualAt(pos)?.StopTimedCountdown();

            // Pause everything while processing
            PauseAllTargets();
            intervalTimer?.Pause();

            Element element = grid.GetElementAt(pos);
            ElementVisual visual = grid.GetElementVisualAt(pos);

            if (element != null && element != grid.EmptyElement &&
                visual != null && visual.IsTargetedByEnemy)
            {
                visual.SetTargetedByEnemy(false);
                visual.StopHoverAnimation();
                targetedPositions.Remove(pos);

                List<Vector2Int> singlePos = new List<Vector2Int> { pos };

                grid.ProcessEnemySpell(singlePos, () =>
                {
                    DamagePlayerWithAnimation(grid, new Vector2Int[] { pos },
                        new List<Element> { element }, damagePerElement, () =>
                        {
                            CleanupTarget(id);
                            isProcessingEffect = false;
                            onComplete?.Invoke();
                        });
                }, false);
            }
            else
            {
                targetedPositions.Remove(pos);
                if (visual != null)
                {
                    visual.SetTargetedByEnemy(false);
                    visual.StopHoverAnimation();
                }
                CleanupTarget(id);
                isProcessingEffect = false;
                onComplete?.Invoke();
            }
            return;
        }

        // No valid expired targets left in queue
        onComplete?.Invoke();
    }

    public void OnPostEffectSettle(Grid grid)
    {
        RemapAllAfterGravity(grid);
        ResumeAllTargets();
        intervalTimer?.Resume();
    }

    public void PauseAllTargets()
    {
        foreach (var kvp in activeTargets)
        {
            if (!kvp.Value.Fired)
            {
                kvp.Value.Timer?.Pause();
                cachedGrid?.GetElementVisualAt(kvp.Value.Position)?.PauseTimedCountdownDOTween();
            }
        }
    }

    public void ResumeAllTargets()
    {
        foreach (var kvp in activeTargets)
        {
            if (!kvp.Value.Fired)
            {
                kvp.Value.Timer?.Resume();
                cachedGrid?.GetElementVisualAt(kvp.Value.Position)?.ResumeTimedCountdownDOTween();
            }
        }
    }

    public void AddTimeToAll(float seconds)
    {
        foreach (var kvp in activeTargets)
        {
            if (!kvp.Value.Fired)
                kvp.Value.Timer?.AddTime(seconds);
        }
    }
    public bool OnTargetCleared(Grid grid, Vector2Int clearedPosition)
    {
        int? clearedId = null;
        foreach (var kvp in activeTargets)
        {
            if (!kvp.Value.Fired && kvp.Value.Position == clearedPosition)
            {
                clearedId = kvp.Key;
                break;
            }
        }

        if (!clearedId.HasValue) return false;

        if (cachedGrid != null)
        {
            ElementVisual visual = cachedGrid.GetElementVisualAt(clearedPosition);
            if (visual != null)
            {
                visual.StopTimedCountdown();
                visual.SetTargetedByEnemy(false);
            }
        }

        CleanupTarget(clearedId.Value);
        targetedPositions.Remove(clearedPosition);

        if (bonusTimePerClearedTarget > 0f)
            AddTimeToAll(bonusTimePerClearedTarget);

        return GetActiveTargetCount() == 0;
    }

    public int GetActiveTargetCount()
    {
        int count = 0;
        foreach (var kvp in activeTargets)
        {
            if (!kvp.Value.Fired) count++;
        }
        return count;
    }

    public override void RetargetAfterGravity(Grid grid)
    {
        if (grid == null) return;
        RemapAllAfterGravity(grid);
    }

    private void RemapAllAfterGravity(Grid grid)
    {
        if (grid == null) return;

        // Group active IDs by column
        var idsByColumn = new Dictionary<int, List<(int id, int y)>>();
        foreach (var kvp in activeTargets)
        {
            if (kvp.Value.Fired) continue;
            int col = kvp.Value.Position.x;
            if (!idsByColumn.ContainsKey(col))
                idsByColumn[col] = new List<(int id, int y)>();
            idsByColumn[col].Add((kvp.Key, kvp.Value.Position.y));
        }
        foreach (var list in idsByColumn.Values)
            list.Sort((a, b) => a.y.CompareTo(b.y));

        //Scan grid for live targeted visuals by column
        var liveByColumn = new Dictionary<int, List<Vector2Int>>();
        for (int x = 0; x < Grid.GridSize; x++)
        {
            for (int y = 0; y < Grid.GridSize; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                ElementVisual visual = grid.GetElementVisualAt(pos);
                if (visual != null && visual.IsTargetedByEnemy)
                {
                    if (!liveByColumn.ContainsKey(x))
                        liveByColumn[x] = new List<Vector2Int>();
                    liveByColumn[x].Add(pos);
                }
            }
        }
        foreach (var list in liveByColumn.Values)
            list.Sort((a, b) => a.y.CompareTo(b.y));

        int totalOrphans = 0;

        foreach (var kvp in idsByColumn)
        {
            int col = kvp.Key;
            var ids = kvp.Value;
            liveByColumn.TryGetValue(col, out List<Vector2Int> livePositions);
            livePositions = livePositions ?? new List<Vector2Int>();

            int paired = Mathf.Min(ids.Count, livePositions.Count);
            for (int i = 0; i < paired; i++)
                activeTargets[ids[i].id].Position = livePositions[i];

            //IDs with no matching live position, cleared by gravity
            for (int i = paired; i < ids.Count; i++)
            {
                CleanupTarget(ids[i].id);
                totalOrphans++;
            }
        }

        if (totalOrphans > 0 && bonusTimePerClearedTarget > 0f)
            AddTimeToAll(bonusTimePerClearedTarget * totalOrphans);

        // Rebuild targetedPositions from surviving active targets
        targetedPositions.Clear();
        foreach (var kvp in activeTargets)
        {
            if (!kvp.Value.Fired)
                targetedPositions.Add(kvp.Value.Position);
        }
    }

    public void ReapplyTargetingVisuals(Grid grid)
    {
        if (grid == null) return;

        foreach (var kvp in activeTargets)
        {
            if (kvp.Value.Fired) continue;

            Vector2Int pos = kvp.Value.Position;
            ElementVisual visual = grid.GetElementVisualAt(pos);
            if (visual != null && !visual.IsTargetedByEnemy)
            {
                visual.SetTargetedByEnemy(true);
                float remaining = kvp.Value.Timer.RemainingSeconds;
                visual.StartTimedCountdown(remaining);
            }
        }
    }

    protected abstract Vector2Int PickTargetPosition(Grid grid);

    protected Vector2Int PickRandomValidPosition(Grid grid)
    {
        List<Vector2Int> candidates = new List<Vector2Int>();
        for (int x = 0; x < Grid.GridSize; x++)
        {
            for (int y = 0; y < Grid.GridSize; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (!grid.IsPositionValid(pos)) continue;
                if (targetedPositions.Contains(pos)) continue;

                Element element = grid.GetElementAt(pos);
                if (element != null && element != grid.EmptyElement)
                    candidates.Add(pos);
            }
        }

        if (candidates.Count == 0)
            return new Vector2Int(-1, -1);

        return candidates[Random.Range(0, candidates.Count)];
    }

    public override void Tick(Grid grid, System.Action onComplete = null)
    {
        onComplete?.Invoke();
    }

    public override void Cleanup(Grid grid)
    {
        IsActive = false;
        intervalTimer?.StopTimer();

        foreach (var kvp in activeTargets)
        {
            kvp.Value.Timer?.Stop();
            if (grid != null)
            {
                ElementVisual visual = grid.GetElementVisualAt(kvp.Value.Position);
                if (visual != null)
                {
                    visual.ClearTargetVisual();
                    visual.StopTimedCountdown();
                    visual.SetTargetedByEnemy(false);
                }
            }
        }

        activeTargets.Clear();
        expiredQueue.Clear();
        targetedPositions.Clear();
    }

    private void CleanupTarget(int id)
    {
        if (activeTargets.TryGetValue(id, out TargetInfo info))
        {
            info.Timer?.Stop();
            activeTargets.Remove(id);
        }
    }
}