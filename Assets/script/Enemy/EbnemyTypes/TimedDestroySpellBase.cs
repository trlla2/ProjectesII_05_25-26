using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public abstract class TimedDestroySpellBase : EnemySpellBase
{
    private SpellTimer spellTimer;
    protected Grid cachedGrid;
    private bool effectApplied;

    protected float bonusTimePerClearedTarget = 0f;
    private int pendingClearedCount;

    public SpellTimer Timer => spellTimer;
    public bool IsEffectApplied => effectApplied;
    public virtual bool IsTimerExpired => spellTimer?.IsExpired ?? false;
    public List<Vector2Int> TargetedPositions => targetedPositions;

    protected TimedDestroySpellBase(float countdown, float damage)
    {
        spellTimer = new SpellTimer(countdown);
        damagePerElement = damage;
        remainingMoves = 0;
        initialMoves = 0;
    }

    // Returns how many targets were detected as cleared during the last RetargetAfterGravity
    public int ConsumeClearedCount()
    {
        int c = pendingClearedCount;
        pendingClearedCount = 0;
        return c;
    }

    public override void Execute(Grid grid)
    {
        base.Execute(grid);
        targetedPositions.Clear();
        cachedGrid = grid;
        effectApplied = false;
        pendingClearedCount = 0;

        if (grid == null) return;

        SelectTargets(grid);
        StartCountdownVisuals(grid);

        SubscribeTimerEvents();
        spellTimer.Start();
    }

    protected void SubscribeTimerEvents()
    {
        spellTimer.OnTimerExpired += OnTimerExpired;
        spellTimer.OnTimeAdded += OnTimeAdded;
    }

    protected abstract void SelectTargets(Grid grid);

    public void PauseTimer()
    {
        spellTimer?.Pause();
        PauseVisuals();
    }

    public void ResumeTimer()
    {
        spellTimer?.Resume();
        ResumeVisuals();
    }

    public void AddTime(float seconds)
    {
        if (spellTimer == null || effectApplied) return;
        spellTimer.AddTime(seconds);
    }

    public bool OnTargetCleared(Grid grid, Vector2Int clearedPosition)
    {        
        if (!targetedPositions.Contains(clearedPosition)) return false;

        targetedPositions.Remove(clearedPosition);

        if (bonusTimePerClearedTarget > 0f)
        {
            AddTime(bonusTimePerClearedTarget);
        }

        return !HasAnyValidTarget(grid);
    }

    //Update targetedPositions to follow visuals that moved with gravity.
    public override void RetargetAfterGravity(Grid grid)
    {
        if (grid == null || effectApplied) return;

        int previousCount = targetedPositions.Count;
        targetedPositions.Clear();
        int size = Grid.GridSize;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                ElementVisual visual = grid.GetElementVisualAt(pos);
                if (visual == null || !visual.IsTargetedByEnemy) continue;

                Element el = grid.GetElementAt(pos);
                if (el != null && el != grid.EmptyElement)
                {
                    // Element survived gravity keep targeting it.
                    targetedPositions.Add(pos);
                }
                else
                {
                    //visual drifted to an empty slot
                    visual.ClearOverrideSprite();
                    visual.SetTargetedByEnemy(false);
                }
            }
        }

        // Visuals destroyed by DeleteElementsByPlayer are gone before gravity runs,
        // so the only way to detect cleared targets is the count difference.
        int clearedDuringGravity = previousCount - targetedPositions.Count;
        if (clearedDuringGravity > 0)
        {
            pendingClearedCount += clearedDuringGravity;
            if (bonusTimePerClearedTarget > 0f)
                AddTime(bonusTimePerClearedTarget * clearedDuringGravity);
        }
    }

    public override void Tick(Grid grid, System.Action onComplete = null)
    {
        if (effectApplied)
        {
            onComplete?.Invoke();
            return;
        }

        if (!spellTimer.IsExpired)
        {
            onComplete?.Invoke();
            return;
        }

        if (!HasAnyValidTarget(grid))
        {
            targetedPositions.Clear();
            ClearAllVisuals(grid);
            onComplete?.Invoke();
            return;
        }

        effectApplied = true;
        ClearAllVisuals(grid);
        ApplyEffect(grid, onComplete);
    }

    private void StartCountdownVisuals(Grid grid)
    {
        if (grid == null) return;

        float duration = spellTimer.TotalSeconds;

        foreach (var pos in targetedPositions)
        {
            ElementVisual visual = grid.GetElementVisualAt(pos);
            if (visual == null || !visual.IsTargetedByEnemy) continue;

            visual.StartTimedCountdown(duration);
        }
    }

    private void OnTimeAdded(float newRemainingTime)
    {
        if (cachedGrid == null) return;

        // Update all visuals with new remaining time
        foreach (var pos in targetedPositions)
        {
            ElementVisual visual = cachedGrid.GetElementVisualAt(pos);
            if (visual == null || !visual.IsTargetedByEnemy) continue;

            visual.ExtendTimedCountdown(newRemainingTime, spellTimer.TotalSeconds);
        }
    }

    private void PauseVisuals()
    {
        if (cachedGrid == null) return;

        foreach (var pos in targetedPositions)
        {
            ElementVisual visual = cachedGrid.GetElementVisualAt(pos);
            visual?.PauseTimedCountdownDOTween();
        }
    }

    private void ResumeVisuals()
    {
        if (cachedGrid == null) return;

        foreach (var pos in targetedPositions)
        {
            ElementVisual visual = cachedGrid.GetElementVisualAt(pos);
            visual?.ResumeTimedCountdownDOTween();
        }
    }

    private void ClearAllVisuals(Grid grid)
    {
        if (grid == null) return;

        foreach (var pos in targetedPositions)
        {
            ElementVisual visual = grid.GetElementVisualAt(pos);
            if (visual != null)
            {
                visual.ClearTargetVisual();
                visual.StopTimedCountdown();
            }
        }
    }

    private void OnTimerExpired()
    {
        UnsubscribeEvents();
    }

    private void UnsubscribeEvents()
    {
        if (spellTimer != null)
        {
            spellTimer.OnTimerExpired -= OnTimerExpired;
            spellTimer.OnTimeAdded -= OnTimeAdded;
        }
    }

    public override void Cleanup(Grid grid)
    {
        spellTimer?.Stop();
        UnsubscribeEvents();
        ClearAllVisuals(grid);
    }

    public void ReapplyTargetingVisuals(Grid grid)
    {
        if (grid == null || effectApplied) return;

        foreach (var pos in targetedPositions)
        {
            ElementVisual visual = grid.GetElementVisualAt(pos);
            if (visual != null && !visual.IsTargetedByEnemy)
            {
                // A new element has fallen into this position, re-target it
                visual.SetTargetedByEnemy(true);
                visual.StartTimedCountdown(spellTimer.TotalSeconds * spellTimer.GetNormalizedRemaining());
            }
        }
    }
}