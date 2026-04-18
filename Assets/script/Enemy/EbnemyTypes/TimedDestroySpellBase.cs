using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public abstract class TimedDestroySpellBase : EnemySpellBase
{
    private SpellTimer spellTimer;
    protected Grid cachedGrid;
    private bool effectApplied;
    private bool isPaused;

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
        ResetEffectState();

        if (grid == null) return;

        SelectTargets(grid);
        ApplyTargetingVisuals(grid);

        SubscribeTimerEvents();
        spellTimer.Start();
    }

    protected void SubscribeTimerEvents()
    {
        spellTimer.OnTimerExpired -= OnTimerExpired; //unsubscribe first to prevent multiple subscriptions
        spellTimer.OnTimerExpired += OnTimerExpired;
    }

    protected abstract void SelectTargets(Grid grid);

    private void ApplyTargetingVisuals(Grid grid)
    {
        if (grid == null) return;

        foreach (var pos in targetedPositions)
        {
            ElementVisual visual = grid.GetElementVisualAt(pos);
            if (visual != null)
            {
                visual.SetTargetedByEnemy(true);
                visual.StartTimedFillAnimation(spellTimer.RemainingSeconds);
            }
        }
    }

    public void PauseTimer()
    {
        spellTimer?.Pause();
        isPaused = true;
        if (cachedGrid == null) return;
        foreach (var pos in targetedPositions)
            cachedGrid.GetElementVisualAt(pos)?.PauseTimedFillAnimation();
    }

    public void ResumeTimer()
    {
        spellTimer?.Resume();
        isPaused = false;
        if (cachedGrid == null) return;
        foreach (var pos in targetedPositions)
            cachedGrid.GetElementVisualAt(pos)?.ResumeTimedFillAnimation();
    }

    public void AddTime(float seconds)
    {
        if (spellTimer == null || effectApplied) return;
        spellTimer.AddTime(seconds);

        if (cachedGrid == null) return;

        // Smoothly extend the fill animation to reflect the new remaining duration
        foreach (var pos in targetedPositions)
        {
            ElementVisual visual = cachedGrid.GetElementVisualAt(pos);
            if (visual == null) continue;
            visual.ExtendTimedCountdown(spellTimer.RemainingSeconds, spellTimer.TotalSeconds);
        }
    }

    // Call this every frame to drive visual state from timer state.
    public virtual void UpdateVisuals()
    {
        if (cachedGrid == null || effectApplied || isPaused) return;

        List<Vector2Int> invalidPositions = null;

        foreach (var pos in targetedPositions)
        {
            ElementVisual visual = cachedGrid.GetElementVisualAt(pos);
            Element element = cachedGrid.GetElementAt(pos);

            bool valid = element != null && element != cachedGrid.EmptyElement && visual != null;

            if (!valid)
            {
                if (visual != null)
                {
                    visual.SetTimedCountdownProgress(0f);
                    visual.SetTargetedByEnemy(false);
                }
                invalidPositions ??= new List<Vector2Int>();
                invalidPositions.Add(pos);
            }
            // DOTween drives the fill — no per-frame SetTimedCountdownProgress needed
        }

        if (invalidPositions != null)
        {
            // Remove ALL invalid positions first, before adding bonus time,
            foreach (var pos in invalidPositions)
            {
                targetedPositions.Remove(pos);
                pendingClearedCount++;
            }

            if (bonusTimePerClearedTarget > 0f)
                AddTime(bonusTimePerClearedTarget * invalidPositions.Count);
        }
    }

    public bool OnTargetCleared(Grid grid, Vector2Int clearedPosition)
    {
        if (!targetedPositions.Contains(clearedPosition)) return false;

        ElementVisual visual = grid?.GetElementVisualAt(clearedPosition);
        if (visual != null)
        {
            visual.SetTimedCountdownProgress(0f);
            visual.SetTargetedByEnemy(false);
        }

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
        int size = grid.GridSize;

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
                    // Element survived gravity — keep targeting it.
                    targetedPositions.Add(pos);
                }
                else
                {
                    //visual drifted to an empty slot
                    visual.ClearOverrideSprite();
                    visual.SetTargetedByEnemy(false);
                    visual.SetTimedCountdownProgress(0f);
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
        ApplyEffect(grid, onComplete);
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
                visual.SetTimedCountdownProgress(0f);
                visual.SetTargetedByEnemy(false);
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
                visual.SetTargetedByEnemy(true);
                // Restart the animation from the current remaining time
                visual.StartTimedFillAnimation(spellTimer.RemainingSeconds);
            }
        }
    }

    protected void ResetEffectState()
    {
        effectApplied = false;
        isPaused = false;
        pendingClearedCount = 0;
    }
}