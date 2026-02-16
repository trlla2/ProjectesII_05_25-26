using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TimedDestroySpellBase : EnemySpellBase
{
    private SpellTimer spellTimer;
    private Grid cachedGrid;
    private bool timerExpired;

    protected TimedDestroySpellBase(float delay, float countdown, float damage, bool excludeBelow)
    {
        spellTimer = new SpellTimer(delay, countdown);
        damagePerElement = damage;
        remainingMoves = 1;
        initialMoves = 1;
        this.excludeBelowTargets = excludeBelow;
    }

    public override void Execute(Grid grid)
    {
        targetedPositions.Clear();
        cachedGrid = grid;
        timerExpired = false;
        remainingMoves = 1;

        if (grid == null) return;

        SelectTargets(grid);

        //timer; delay -> countdown visuals -> auto tick
        spellTimer.Start(grid,
            onDelayComplete: () => OnDelayComplete(grid),
            onCountdownExpired: () => OnCountdownExpired(grid)
        );
    }
    
    // we need this abstract class to implement their specific spells
    protected abstract void SelectTargets(Grid grid);

    // For timed spells, Tick() is ignored during player moves.
    // It only fires the effect when the timer has expired.
    public override void Tick(Grid grid, System.Action onComplete = null)
    {
        if (!timerExpired)
            return;

        if (!HasAnyValidTarget(grid))
        {
            targetedPositions.Clear();
            onComplete?.Invoke();
            return;
        }

        base.Tick(grid, onComplete);
    }

    private void OnDelayComplete(Grid grid)
    {
        if (grid == null) return;

        foreach (var pos in targetedPositions)
        {
            ElementVisual visual = grid.GetElementVisualAt(pos);
            if (visual != null && visual.IsTargetedByEnemy)
                visual.StartCountdown(spellTimer.CountdownSeconds);
        }
    }

    private void OnCountdownExpired(Grid grid)
    {
        if (grid == null) return;
        timerExpired = true;

        // EnemyBehaviour will handle retargeting via the onComplete callback
        // EnemyBehaviour will be notified to retarget through its coroutine monitoring.
    }

    public bool IsTimerExpired() => timerExpired;
}