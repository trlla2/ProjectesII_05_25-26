using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public abstract class TimedDestroySpellBase : EnemySpellBase
{
    private SpellTimer spellTimer;
    protected Grid cachedGrid;
    private bool effectApplied;

    protected float bonusTimePerClearedTarget = 0f;

    public SpellTimer Timer => spellTimer;
    public bool IsEffectApplied => effectApplied;
    public bool IsTimerExpired => spellTimer?.IsExpired ?? false;
    public float BonusTimePerClearedTarget => bonusTimePerClearedTarget;
    public List<Vector2Int> TargetedPositions => targetedPositions;

    protected TimedDestroySpellBase(float countdown, float damage, bool excludeBelow)
    {
        spellTimer = new SpellTimer(countdown);
        damagePerElement = damage;
        excludeBelowTargets = excludeBelow;
        remainingMoves = 0;
        initialMoves = 0;
    }

    public override void Execute(Grid grid)
    {
        targetedPositions.Clear();
        cachedGrid = grid;
        effectApplied = false;

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
            Debug.Log($"Adding {bonusTimePerClearedTarget} seconds to timer for cleared target at {clearedPosition}");
            AddTime(bonusTimePerClearedTarget);
        }

        return !HasAnyValidTarget(grid);
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
}