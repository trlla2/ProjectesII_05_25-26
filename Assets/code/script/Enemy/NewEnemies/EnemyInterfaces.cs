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

            // Pass excludeBelowTargets through to grid so the refill logic can choose behavior
            grid.ProcessEnemySpell(finalPositions, () =>
            {
                DamagePlayer(totalDamage);
                onComplete?.Invoke();
            }, excludeBelowTargets);
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
        
        GameManager.Instance.AttackPlayer(damage);
    }
}

public class ColumnDestroySpell : EnemySpellBase
{
    private int columnIndex;

    public ColumnDestroySpell(float damage = 4f, int moves = 2, bool excludeBelowTargets = true, int column = -1)
    {
        damagePerElement = damage;
        remainingMoves = moves;
        initialMoves = moves;
        columnIndex = column;
        spellName = "Column Destroy";
        this.excludeBelowTargets = excludeBelowTargets;
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

    public RandomDestroySpell(int count = 3, float damage = 4f, int moves = 2, bool excludeBelowTargets = true)
    {
        elementCount = count;
        damagePerElement = damage;
        remainingMoves = moves;
        initialMoves = moves;
        spellName = "Random Destroy";
        this.excludeBelowTargets = excludeBelowTargets;
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

public class SingleElementTimedSpell : EnemySpellBase
{
    private float timerSeconds;
    private Grid ownerGrid;

    public SingleElementTimedSpell(float timer = 4f, float damage = 4f, bool excludeBelowTargets = true)
    {
        timerSeconds = timer;
        damagePerElement = damage;
        remainingMoves = 0;
        initialMoves = 0;
        spellName = "Single Timed Target";
        this.excludeBelowTargets = excludeBelowTargets;
    }

    public override void Execute(Grid grid)
    {
        targetedPositions.Clear();
        ownerGrid = grid;

        if (grid == null)
            return;

        Vector2Int pick = PickRandomValidPosition(grid);
        if (pick.x == -1)
            return;

        targetedPositions.Add(pick);

        ElementVisual visual = grid.GetElementVisualAt(pick);
        if (visual != null)
        {
            visual.SetTargetedByEnemy(true);
            visual.StartCountdown(timerSeconds);
        }

        grid.StartCoroutine(CountdownAndApply());
    }

    public override void Tick(Grid grid, System.Action onComplete = null)
    {

    }

    private IEnumerator CountdownAndApply()
    {
        while (true)
        {
            if (ownerGrid == null)
                yield break;

            // If current target is invalid, clear and retarget immediately
            if (targetedPositions.Count == 0 || !IsTargetStillValid(ownerGrid, targetedPositions[0]))
            {
                if (targetedPositions.Count > 0)
                {
                    var old = targetedPositions[0];
                    var oldVis = ownerGrid.GetElementVisualAt(old);
                    if (oldVis != null)
                    {
                        oldVis.StopCountdown();
                        oldVis.SetTargetedByEnemy(false);
                    }
                    targetedPositions.Clear();
                }

                Vector2Int newPick = PickRandomValidPosition(ownerGrid);
                if (newPick.x == -1)
                {
                    // no valid targets -> stop
                    yield break;
                }

                targetedPositions.Add(newPick);
                var newVis = ownerGrid.GetElementVisualAt(newPick);
                if (newVis != null)
                {
                    newVis.SetTargetedByEnemy(true);
                    newVis.StartCountdown(timerSeconds);
                }
            }

            // Countdown
            float elapsed = 0f;
            while (elapsed < timerSeconds)
            {
                if (ownerGrid == null)
                    yield break;

                if (targetedPositions.Count == 0 || !IsTargetStillValid(ownerGrid, targetedPositions[0]))
                    break;

                elapsed += Time.deltaTime;
                yield return null;
            }

            if (targetedPositions.Count > 0 && IsTargetStillValid(ownerGrid, targetedPositions[0]) && elapsed >= timerSeconds)
            {
                var pos = targetedPositions[0];
                List<Vector2Int> finalPositions = new List<Vector2Int> { pos };

                // Flag to wait until grid processing completes
                bool processingComplete = false;

                ownerGrid.ProcessEnemySpell(finalPositions, () =>
                {
                    DamagePlayer(damagePerElement);
                    processingComplete = true;
                }, excludeBelowTargets);

                var vis = ownerGrid.GetElementVisualAt(pos);
                if (vis != null)
                {
                    vis.StopCountdown();
                    vis.SetTargetedByEnemy(false);
                }

                targetedPositions.Clear();

                while (!processingComplete)
                {
                    if (ownerGrid == null)
                        yield break;
                    yield return null;
                }

                // After processing completes, loop continues and the spell will retarget automatically.
            }

            yield return null;
        }
    }

    private bool IsTargetStillValid(Grid grid, Vector2Int pos)
    {
        if (grid == null)
            return false;
        if (!grid.IsPositionValid(pos))
            return false;
        Element element = grid.GetElementAt(pos);
        ElementVisual visual = grid.GetElementVisualAt(pos);
        return element != null && element != grid.EmptyElement && visual != null && visual.IsTargetedByEnemy;
    }

    private Vector2Int PickRandomValidPosition(Grid grid)
    {
        List<Vector2Int> candidates = new List<Vector2Int>();
        for (int x = 0; x < Grid.GridSize; x++)
        {
            for (int y = 0; y < Grid.GridSize; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (!grid.IsPositionValid(pos))
                    continue;

                Element element = grid.GetElementAt(pos);
                if (element != null && element != grid.EmptyElement)
                {
                    candidates.Add(pos);
                }
            }
        }

        if (candidates.Count == 0)
            return new Vector2Int(-1,-1);

        int idx = Random.Range(0, candidates.Count);
        return candidates[idx];
    }
}