using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemySpell
{
    void Execute(Grid grid);
    //Tick once per player move. if max moves reached, apply effect.
    void Tick(Grid grid, System.Action onComplete = null);
    int RemainingMoves { get; }
    string GetSpellName();
}

public abstract class EnemySpellBase : IEnemySpell
{
    protected int remainingMoves;
    protected int initialMoves;
    protected string spellName;
    protected float damagePerElement;
    protected List<Vector2Int> targetedPositions = new List<Vector2Int>();

    protected bool excludeBelowTargets = true;

    public int RemainingMoves => remainingMoves;
    public string GetSpellName() => spellName;

    public abstract void Execute(Grid grid);

    public virtual void Tick(Grid grid, System.Action onComplete = null)
    {
        if (grid != null)
        {
            float animDuration = grid.EnemyTargetAnimDuration;
            AnimationCurve curve = grid.EnemyTargetFillCurve;
            foreach (var pos in targetedPositions)
            {
                ElementVisual visual = grid.GetElementVisualAt(pos);
                if (visual != null && visual.IsTargetedByEnemy)
                    visual.UpdateTargetVisual(remainingMoves, initialMoves, animDuration, curve);
            }
        }

        remainingMoves--;
        remainingMoves = Mathf.Max(0, remainingMoves);

        if (grid != null)
        {
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

        if (remainingMoves <= 0)
        {
            // forward excludeBelowTargets flag into ProcessEnemySpell
            ApplyEffect(grid, onComplete);
        }
    }

    protected virtual void ApplyEffect(Grid grid, System.Action onComplete = null)
    {
        if (grid == null)
        {
            onComplete?.Invoke();
            return;
        }

        //collect only positions that still contain non-empty elements and are targeted
        List<Vector2Int> finalPositions = new List<Vector2Int>();
        List<Element> elements = new List<Element>();

        foreach (var pos in targetedPositions)
        {
            if (!grid.IsPositionValid(pos))
                continue;

            Element element = grid.GetElementAt(pos);
            ElementVisual visual = grid.GetElementVisualAt(pos);

            if (element != null && element != grid.EmptyElement && visual != null && visual.IsTargetedByEnemy)
            {
                finalPositions.Add(pos);
                elements.Add(element);
            }
            else
            {
                // clear target if it's no longer valid
                if (visual != null)
                {
                    visual.SetTargetedByEnemy(false);
                }
            }
        }

        if (finalPositions.Count > 0)
        {
            // Pass excludeBelowTargets through to grid so the refill logic can choose behavior
            grid.ProcessEnemySpell(finalPositions, () =>
            {
                DamagePlayerWithAnimation(grid, finalPositions.ToArray(), elements, damagePerElement, onComplete);
            }, excludeBelowTargets);
        }
        else
        {
            // nothing to process, continue immediately
            onComplete?.Invoke();
        }

        targetedPositions.Clear();
    }

    protected void DamagePlayerWithAnimation(Grid grid, Vector2Int[] positions, List<Element> elements, float damagePerElement, System.Action onComplete = null)
    {
        SpellPathAnimator enemyAnimator = grid.GetEnemySpellAnimator();

        if (enemyAnimator != null)
        {
            enemyAnimator.SetIsPlayerAnimator(false);
            enemyAnimator.SetDamagePerElement(damagePerElement);
            enemyAnimator.OnSpellCast(positions, elements, onComplete);
        }
        else
        {
            Debug.LogWarning("Enemy spell animator not found on grid, CANT DEAL DMG");
        }
    }
}