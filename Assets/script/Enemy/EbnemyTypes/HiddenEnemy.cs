using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiddenDestroySpell : TimedDestroySpellBase
{
    private int elementCount;
    private float visibilityDuration;
    private Grid cachedGrid;
    private bool elementsHidden;
    private Coroutine hideCoroutine;
    private Coroutine revealCoroutine;
    private int lastValidTargetCount;
    private const float revealOnMatchDuration = 0.5f;
    private Sprite hiddenSprite;

    public bool ElementsHidden => elementsHidden;

    public HiddenDestroySpell(int count = 3, float damage = 4f, float attackTimer = 5f,
                               bool excludeBelow = true, float visibilityDuration = 2.5f,
                               Sprite hiddenSprite = null)
        : base(visibilityDuration, attackTimer, damage, excludeBelow)
    {
        elementCount = count;
        this.visibilityDuration = visibilityDuration;
        this.hiddenSprite = hiddenSprite;
        spellName = "Hidden Destroy";
        shouldRerollAfterAttack = true;
    }

    protected override void SelectTargets(Grid grid)
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
            }

            allPositions.RemoveAt(randomIndex);
        }
    }

    public override void Execute(Grid grid)
    {
        // Clean up coroutines from a previous cycle
        if (cachedGrid != null)
        {
            if (hideCoroutine != null)
            {
                cachedGrid.StopCoroutine(hideCoroutine);
                hideCoroutine = null;
            }
            if (revealCoroutine != null)
            {
                cachedGrid.StopCoroutine(revealCoroutine);
                revealCoroutine = null;
            }
        }

        cachedGrid = grid;
        elementsHidden = false;

        base.Execute(grid); // SelectTargets, delay blink animation, start timer

        lastValidTargetCount = targetedPositions.Count;

        if (grid != null)
        {
            hideCoroutine = grid.StartCoroutine(HideAfterDelay(grid));
        }
    }

    public override void Tick(Grid grid, System.Action onComplete = null)
    {
        // Restore original sprites before the base applies the effect
        RevealAllTargets(grid);
        base.Tick(grid, onComplete);
    }

    public override void Cleanup(Grid grid)
    {
        if (grid == null) return;

        if (hideCoroutine != null)
        {
            grid.StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        if (revealCoroutine != null)
        {
            grid.StopCoroutine(revealCoroutine);
            revealCoroutine = null;
        }

        ShowAllTargets(grid);
    }

    public bool CheckForRemovedTargets(Grid grid)
    {
        if (grid == null) return false;

        int currentCount = CountValidTargets(grid);

        if (currentCount <= 0)
            return true;

        // Brief reveal only while elements are hidden
        if (elementsHidden && currentCount < lastValidTargetCount)
        {
            lastValidTargetCount = currentCount;

            if (revealCoroutine != null)
                grid.StopCoroutine(revealCoroutine);

            revealCoroutine = grid.StartCoroutine(BriefReveal(grid));
        }

        return false;
    }

    //Handle grid auto reload for this specific spell
    public void ReExecuteAfterReroll(Grid grid)
    {
        Cleanup(grid);
        Execute(grid);
    }

    private IEnumerator HideAfterDelay(Grid grid)
    {
        yield return new WaitForSeconds(visibilityDuration);

        if (grid == null) yield break;

        HideAllTargets(grid);
        lastValidTargetCount = CountValidTargets(grid);
        hideCoroutine = null;
    }

    private IEnumerator BriefReveal(Grid grid)
    {
        if (grid == null) yield break;

        // Restore original sprites
        foreach (var pos in targetedPositions)
        {
            ElementVisual visual = grid.GetElementVisualAt(pos);
            if (visual != null && visual.IsTargetedByEnemy)
            {
                visual.ClearOverrideSprite();
            }
        }

        yield return new WaitForSeconds(revealOnMatchDuration);

        if (grid == null) yield break;

        // Swap back to hidden sprite
        foreach (var pos in targetedPositions)
        {
            ElementVisual visual = grid.GetElementVisualAt(pos);
            if (visual != null && visual.IsTargetedByEnemy)
            {
                visual.SetOverrideSprite(hiddenSprite);
            }
        }

        revealCoroutine = null;
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

    private void HideAllTargets(Grid grid)
    {
        if (grid == null) return;

        foreach (var pos in targetedPositions)
        {
            ElementVisual visual = grid.GetElementVisualAt(pos);
            if (visual != null && visual.IsTargetedByEnemy)
            {
                visual.SetOverrideSprite(hiddenSprite);
            }
        }

        elementsHidden = true;
    }

    private void RevealAllTargets(Grid grid)
    {
        if (grid == null) return;

        foreach (var pos in targetedPositions)
        {
            ElementVisual visual = grid.GetElementVisualAt(pos);
            if (visual != null)
            {
                visual.ClearOverrideSprite();
            }
        }

        elementsHidden = false;
    }

    private void ShowAllTargets(Grid grid)
    {
        if (grid == null) return;

        foreach (var pos in targetedPositions)
        {
            ElementVisual visual = grid.GetElementVisualAt(pos);
            if (visual != null)
            {
                visual.ClearOverrideSprite();
                visual.SetTargetedByEnemy(false);
                visual.ClearTargetVisual();
            }
        }

        elementsHidden = false;
    }
}
