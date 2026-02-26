using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiddenDestroySpell : EnemySpellBase
{
    private int elementCount;
    private float visibilityDuration;
    private Grid cachedGrid;
    private bool elementsHidden;
    private Coroutine hideCoroutine;

    public bool ElementsHidden => elementsHidden;

    public HiddenDestroySpell(int count = 3, float damage = 4f, int moves = 2,
                               bool excludeBelow = true, float visibilityDuration = 2.5f)
    {
        elementCount = count;
        damagePerElement = damage;
        remainingMoves = moves;
        initialMoves = moves;
        spellName = "Hidden Destroy";
        this.excludeBelowTargets = excludeBelow;
        this.visibilityDuration = visibilityDuration;
        shouldRerollAfterAttack = true;
    }

    public override void Execute(Grid grid)
    {
        targetedPositions.Clear();
        cachedGrid = grid;
        elementsHidden = false;

        if (grid == null)
            return;

        SelectRandomTargets(grid, elementCount);

        hideCoroutine = grid.StartCoroutine(VisibilityPhase(grid));
    }

    //HAndle grid auto reload for this specigfic spell
    public void ReExecuteAfterReroll(Grid grid)
    {
        int currentActiveTargets = CountValidTargets(grid);

        ShowAllTargets(grid);

        targetedPositions.Clear();
        cachedGrid = grid;
        elementsHidden = false;

        if (grid == null)
            return;

        // Retarget only as many positions as were still targeted before the reroll
        SelectRandomTargets(grid, currentActiveTargets);

        if (hideCoroutine != null)
            grid.StopCoroutine(hideCoroutine);

        hideCoroutine = grid.StartCoroutine(VisibilityPhase(grid));
    }

    public override void Tick(Grid grid, System.Action onComplete = null)
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
            ShowAllTargets(grid);
            ApplyEffect(grid, onComplete);
        }
    }

    public override void Cleanup(Grid grid)
    {
        if (grid == null) return;

        if (hideCoroutine != null)
        {
            grid.StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        ShowAllTargets(grid);
    }

    private void SelectRandomTargets(Grid grid, int count)
    {
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

        int targetCount = Mathf.Min(count, allPositions.Count);
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
                visual.UpdateTargetVisual(remainingMoves, initialMoves,
                    grid.EnemyTargetAnimDuration, grid.EnemyTargetFillCurve);
            }

            allPositions.RemoveAt(randomIndex);
        }
    }

    private int CountValidTargets(Grid grid)
    {
        if (grid == null) return 0;

        int count = 0;
        foreach (var pos in targetedPositions)
        {
            if (!grid.IsPositionValid(pos)) continue;

            Element element = grid.GetElementAt(pos);
            ElementVisual visual = grid.GetElementVisualAt(pos);

            if (element != null && element != grid.EmptyElement && visual != null && visual.IsTargetedByEnemy)
            {
                count++;
            }
        }
        return count;
    }

    private IEnumerator VisibilityPhase(Grid grid)
    {
        if (grid == null)
            yield break;

        float blinkStrength = 0.08f;
        float blinkFrequency = 1.5f;

        foreach (var pos in targetedPositions)
        {
            ElementVisual visual = grid.GetElementVisualAt(pos);
            if (visual != null && visual.IsTargetedByEnemy)
            {
                ScaleAnimator scaleAnimator = visual.GetComponent<ScaleAnimator>();
                if (scaleAnimator != null)
                {
                    scaleAnimator.PlayDelayBlink(blinkStrength, blinkFrequency, visibilityDuration);
                }
            }
        }

        yield return new WaitForSeconds(visibilityDuration);

        HideAllTargets(grid);

        hideCoroutine = null;
    }

    private void HideAllTargets(Grid grid)
    {
        if (grid == null) return;

        foreach (var pos in targetedPositions)
        {
            ElementVisual visual = grid.GetElementVisualAt(pos);
            if (visual != null && visual.IsTargetedByEnemy)
            {
                visual.SetSpriteVisible(false);
            }
        }

        elementsHidden = true;
    }

    private void ShowAllTargets(Grid grid)
    {
        if (grid == null) return;

        foreach (var pos in targetedPositions)
        {
            ElementVisual visual = grid.GetElementVisualAt(pos);
            if (visual != null)
            {
                visual.SetSpriteVisible(true);
                visual.SetTargetedByEnemy(false);
                visual.ClearTargetVisual();
            }
        }

        elementsHidden = false;
    }
}
