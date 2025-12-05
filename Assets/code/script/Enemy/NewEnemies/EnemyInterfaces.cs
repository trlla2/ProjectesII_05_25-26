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

        foreach (var pos in targetedPositions)
        {
            if (!grid.IsPositionValid(pos))
                continue;

            Element element = grid.GetElementAt(pos);
            ElementVisual visual = grid.GetElementVisualAt(pos);

            if (element != null && element != grid.EmptyElement && visual != null && visual.IsTargetedByEnemy)
            {
                finalPositions.Add(pos);
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
            float totalDamage = damagePerElement * finalPositions.Count;

            grid.ProcessEnemySpell(finalPositions, () =>
            {
                DamagePlayer(totalDamage);
                onComplete?.Invoke();
            });
        }
        else
        {
            // nothing to process, continue immediately
            onComplete?.Invoke();
        }

        targetedPositions.Clear();
    }

    protected void DamagePlayer(float damage)
    {
        Debug.Log($"Enemy spell deals {damage} damage to player.");
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.DealDamage(damage);
        }
    }
}

public class ColumnDestroySpell : EnemySpellBase
{
    private int columnIndex;

    public ColumnDestroySpell(float damage = 4f, int moves = 2, int column = -1)
    {
        damagePerElement = damage;
        remainingMoves = moves;
        initialMoves = moves;
        columnIndex = column;
        spellName = "Column Destroy";
    }

    public override void Execute(Grid grid)
    {
        targetedPositions.Clear();

        if (grid == null)
            return;

        int targetColumn = columnIndex >= 0 ? columnIndex : Random.Range(0, Grid.GridSize);

        for (int row = 0; row < Grid.GridSize; row++)
        {
            Vector2Int pos = new Vector2Int(targetColumn, row);
            if (grid.IsPositionValid(pos))
            {
                Element element = grid.GetElementAt(pos);
                if (element != null && element != grid.EmptyElement)
                {
                    targetedPositions.Add(pos);
                    ElementVisual visual = grid.GetElementVisualAt(pos);
                    if (visual != null)
                    {
                        visual.SetTargetedByEnemy(true);
                        visual.UpdateTargetVisual(remainingMoves, initialMoves, grid.EnemyTargetAnimDuration, grid.EnemyTargetFillCurve);
                    }
                }
            }
        }
    }
}

public class RandomDestroySpell : EnemySpellBase
{
    private int elementCount;

    public RandomDestroySpell(int count = 3, float damage = 4f, int moves = 2)
    {
        elementCount = count;
        damagePerElement = damage;
        remainingMoves = moves;
        initialMoves = moves;
        spellName = "Random Destroy";
    }

    public override void Execute(Grid grid)
    {
        targetedPositions.Clear();

        if (grid == null)
            return;

        List<Vector2Int> allPositions = new List<Vector2Int>();

        for (int x = 0; x < Grid.GridSize; x++)
        {
            for (int y = 0; y < Grid.GridSize; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                Element element = grid.GetElementAt(pos);
                if (element != null && element != grid.EmptyElement)
                {
                    allPositions.Add(pos);
                }
            }
        }

        int targetCount = Mathf.Min(elementCount, allPositions.Count);
        for (int i = 0; i < targetCount; i++)
        {
            if (allPositions.Count == 0)
                break;

            int randomIndex = Random.Range(0, allPositions.Count);
            Vector2Int chosen = allPositions[randomIndex];
            targetedPositions.Add(chosen);

            ElementVisual visual = grid.GetElementVisualAt(chosen);
            if (visual != null)
            {
                visual.SetTargetedByEnemy(true);
                visual.UpdateTargetVisual(remainingMoves, initialMoves, grid.EnemyTargetAnimDuration, grid.EnemyTargetFillCurve);
            }

            allPositions.RemoveAt(randomIndex);
        }
    }
}