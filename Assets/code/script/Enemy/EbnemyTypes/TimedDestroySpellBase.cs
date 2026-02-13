using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//we have an abstract function
public abstract class TimedDestroySpellBase : EnemySpellBase
{
    private float delaySeconds;
    private float countdownSeconds;
    private Grid Grid;

    protected TimedDestroySpellBase(float delay, float countdown, float damage, bool excludeBelow)
    {
        delaySeconds = delay;
        countdownSeconds = countdown;
        damagePerElement = damage;
        remainingMoves = 0;
        initialMoves = 0;
        this.excludeBelowTargets = excludeBelow;
    }

    public override void Execute(Grid grid)
    {
        targetedPositions.Clear();
        Grid = grid;
        if (grid == null) return;

        SelectTargets(grid);
        grid.StartCoroutine(TimedAttackRoutine());
    }

    //Abstract method our diferent enemyspell patterns can implement
    protected abstract void SelectTargets(Grid grid);

    public override void Tick(Grid grid, System.Action onComplete = null) { }

    private IEnumerator TimedAttackRoutine()
    {
        while (true)
        {
            //DElay 5 seconds firts
            float elapsed = 0f;
            while (elapsed < delaySeconds)
            {
                if (Grid == null) yield break;
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (Grid == null) yield break;

            foreach (var pos in targetedPositions)
            {
                ElementVisual visual = Grid.GetElementVisualAt(pos);
                if (visual != null && visual.IsTargetedByEnemy)
                    visual.StartCountdown(countdownSeconds);
            }

            elapsed = 0f;
            bool timerExpired = false;
            while (elapsed < countdownSeconds)
            {
                if (Grid == null) yield break;
                if (!HasAnyValidTarget())
                    break;

                elapsed += Time.deltaTime;
                yield return null;
            }

            if (elapsed >= countdownSeconds)
                timerExpired = true;

            if (Grid == null) yield break;

            if (timerExpired && HasAnyValidTarget())
            {
                bool attackDone = false;
                ApplyEffect(Grid, () => attackDone = true, true);

                while (!attackDone)
                {
                    if (Grid == null) yield break;
                    yield return null;
                }
            }

            while (Grid != null && Grid.CurrentState != GridStateType.Idle)
                yield return null;

            if (Grid == null) yield break;

            targetedPositions.Clear();
            SelectTargets(Grid);
            yield return null;
        }
    }

    private bool HasAnyValidTarget()
    {
        if (Grid == null) return false;

        foreach (var pos in targetedPositions)
        {
            ElementVisual visual = Grid.GetElementVisualAt(pos);
            if (visual != null && visual.IsTargetedByEnemy)
                return true;
        }
        return false;
    }
}