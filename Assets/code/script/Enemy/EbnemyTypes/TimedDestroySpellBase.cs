using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TimedDestroySpellBase : EnemySpellBase
{
    private SpellTimer spellTimer;
    private Grid cachedGrid;
    private bool timerExpired;
    private bool effectApplied;
    private bool delayCompleted;
    protected bool useMoveTicks = false;

    public bool UseMoveTicks => useMoveTicks;

    protected TimedDestroySpellBase(float delay, float countdown, float damage, bool excludeBelow)
    {
        spellTimer = new SpellTimer(delay, countdown);
        damagePerElement = damage;
        remainingMoves = 1;
        initialMoves = 1;
        this.excludeBelowTargets = excludeBelow;
    }

    protected void SetMoveCount(int moves)
    {
        remainingMoves = moves;
        initialMoves = moves;
    }

    public override void Execute(Grid grid)
    {
        targetedPositions.Clear();
        cachedGrid = grid;
        timerExpired = false;
        effectApplied = false;
        delayCompleted = false;

        if (grid == null) return;
            
        SelectTargets(grid);

        spellTimer.Start(grid,
            onDelayComplete: () => OnDelayComplete(grid),
            onCountdownExpired: () => OnCountdownExpired(grid)
        );
    }

    // we need this abstract class to implement their specific spells
    protected abstract void SelectTargets(Grid grid);

    public void TickPlayerMove(Grid grid, System.Action onComplete = null)
    {
        if (effectApplied)
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

        // Decrement move counter
        remainingMoves--;
        remainingMoves = Mathf.Max(0, remainingMoves);

        // Update move visual to show remaining moves
        UpdateMoveVisuals(grid);

        // Check if moves depleted - trigger spell
        if (remainingMoves <= 0)
        {
            effectApplied = true;
            ClearAllVisuals(grid); // Clear both visuals when spell triggers
            ApplyEffect(grid, onComplete);
            return;
        }

        onComplete?.Invoke();
    }

    //Called when timer expires
    public override void Tick(Grid grid, System.Action onComplete = null)
    {
        if (effectApplied)
        {
            onComplete?.Invoke();
            return;
        }

        if (!timerExpired)
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
        ClearAllVisuals(grid); // Clear both visuals when spell triggers
        ApplyEffect(grid, onComplete);
    }

    private void UpdateMoveVisuals(Grid grid)
    {
        if (grid == null) return;

        float animDuration = grid.EnemyTargetAnimDuration;
        AnimationCurve curve = grid.EnemyTargetFillCurve;

        foreach (var pos in targetedPositions)
        {
            ElementVisual visual = grid.GetElementVisualAt(pos);
            if (visual != null && visual.IsTargetedByEnemy)
            {
                visual.UpdateTargetVisual(remainingMoves, initialMoves, animDuration, curve);
            }
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
                visual.ClearTimedTargetVisual();
            }
        }
    }

    private void OnDelayComplete(Grid grid)
    {
        if (grid == null || effectApplied) return;

        delayCompleted = true;

        foreach (var pos in targetedPositions)
        {
            ElementVisual visual = grid.GetElementVisualAt(pos);
            if (visual != null && visual.IsTargetedByEnemy)
                visual.StartTimedCountdown(spellTimer.CountdownSeconds);
        }
    }

    private void OnCountdownExpired(Grid grid)
    {
        if (grid == null) return;
        timerExpired = true;
    }

    public bool IsTimerExpired() => timerExpired;
    public bool IsEffectApplied() => effectApplied;
}