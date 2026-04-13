using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiddenDestroySpell : TimedDestroySpellBase
{
    private int elementCount;
    private float visibilityDuration;
    private bool elementsHidden;
    private bool timerStarted;
    private Coroutine visibilityCoroutine;
    private Coroutine revealCoroutine;
    private List<Coroutine> targetingAnimations = new List<Coroutine>();
    private const float revealOnMatchDuration = 0.5f;
    private Sprite hiddenSprite;

    private bool hasTriggeredLowTimeReveal;
    private const float lowTimeThreshold = 0.33f;

    // Animation settings
    private const float animationStartInterval = 1f;
    private const float animationEndInterval = 0.05f;

    public bool ElementsHidden => elementsHidden;
    public bool TimerStarted => timerStarted;

    public HiddenDestroySpell(int count = 3, float damage = 4f, float attackTimer = 6f,
                               float visibilityDuration = 2.5f,
                               Sprite hiddenSprite = null, float bonusTimeOnClear = 1.5f)
        : base(attackTimer, damage)
    {
        elementCount = count;
        this.visibilityDuration = visibilityDuration;
        this.hiddenSprite = hiddenSprite;
        spellName = "Hidden Destroy";
        shouldRerollAfterAttack = true;
        bonusTimePerClearedTarget = bonusTimeOnClear;
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
        // Don't call base.Execute - we handle timer start ourselves after visibility phase
        ResetEffectState();
        targetedPositions.Clear();
        cachedGrid = grid;

        CleanupCoroutines(grid);

        elementsHidden = false;
        timerStarted = false;
        hasTriggeredLowTimeReveal = false;
        targetingAnimations.Clear();

        if (grid == null) return;

        SelectTargets(grid);

        visibilityCoroutine = grid.StartCoroutine(VisibilityPhaseRoutine(grid));
    }

    public override void UpdateVisuals()
    {
        if (!timerStarted) return;

        // Call base implementation for countdown progress
        base.UpdateVisuals();

        // Check for low time reveal
        if (cachedGrid != null && !hasTriggeredLowTimeReveal && elementsHidden)
        {
            if (Timer.GetNormalizedRemaining() < lowTimeThreshold)
            {
                hasTriggeredLowTimeReveal = true;
                TriggerBriefReveal(cachedGrid);
            }
        }
    }

    public override void Tick(Grid grid, System.Action onComplete = null)
    {
        RevealAllTargets(grid);
        base.Tick(grid, onComplete);
    }

    public override void Cleanup(Grid grid)
    {
        if (grid == null) return;

        CleanupCoroutines(grid);
        RevealAllTargets(grid);
        base.Cleanup(grid);
    }

    public void TriggerBriefReveal(Grid grid)
    {
        Debug.Log("TriggerBriefReveal called");
        if (grid == null || !elementsHidden) return;

        if (revealCoroutine != null)
            grid.StopCoroutine(revealCoroutine);

        revealCoroutine = grid.StartCoroutine(BriefRevealRoutine(grid));
    }

    public void ReExecuteAfterReroll(Grid grid)
    {
        Cleanup(grid);
        Execute(grid);
    }

    private IEnumerator VisibilityPhaseRoutine(Grid grid)
    {
        if (grid == null) yield break;

        elementsHidden = false;

        // Start targeting animations for all targets
        foreach (var pos in targetedPositions)
        {
            Coroutine anim = grid.StartCoroutine(TargetingAnimationRoutine(grid, pos, visibilityDuration));
            targetingAnimations.Add(anim);
        }

        yield return new WaitForSeconds(visibilityDuration);

        if (grid == null) yield break;

        StopTargetingAnimations(grid);
        HideAllTargets(grid);

        // Start the countdown timer - now controller-driven
        SubscribeTimerEvents();
        Timer.Start();
        timerStarted = true;

        // Start the fill animation for each target now that the timer is running
        foreach (var pos in targetedPositions)
        {
            ElementVisual visual = grid.GetElementVisualAt(pos);
            if (visual == null || !visual.IsTargetedByEnemy) continue;

            visual.StartTimedFillAnimation(Timer.RemainingSeconds);
        }

        visibilityCoroutine = null;
    }

    private IEnumerator TargetingAnimationRoutine(Grid grid, Vector2Int pos, float duration)
    {
        if (grid == null) yield break;

        ElementVisual visual = grid.GetElementVisualAt(pos);
        if (visual == null) yield break;

        const int toggleCount = 12; // total number of flips between real / hidden

        float totalTime = Mathf.Max(0.01f, duration);
        float elapsed = 0f;
        bool showHidden = false;

        float avgInterval = (animationStartInterval + animationEndInterval) * 0.5f;
        float normalizedTotal = avgInterval * toggleCount;
        float scale = totalTime / normalizedTotal;

        for (int i = 0; i < toggleCount; i++)
        {
            // Normalized progress of this toggle within the whole sequence
            float t = (float)i / (toggleCount - 1);

            float interval = Mathf.Lerp(animationStartInterval, animationEndInterval, t) * scale;

            // Safety clamp so we don't overshoot too much
            if (elapsed + interval > totalTime)
                interval = Mathf.Max(0.01f, totalTime - elapsed);

            showHidden = !showHidden;
            if (showHidden)
                visual.SetOverrideSprite(hiddenSprite);
            else
                visual.ClearOverrideSprite();

            yield return new WaitForSeconds(interval);
            elapsed += interval;

            if (elapsed >= totalTime || grid == null)
                break;
        }

        if (visual != null)
            visual.ClearOverrideSprite();
    }

    private IEnumerator BriefRevealRoutine(Grid grid)
    {
        Debug.Log("Triggering brief reveal");
        if (grid == null) yield break;

        // Restore original sprites
        foreach (var pos in targetedPositions)
        {
            ElementVisual visual = grid.GetElementVisualAt(pos);
            if (visual != null && visual.IsTargetedByEnemy)
                visual.ClearOverrideSprite();
        }

        yield return new WaitForSeconds(revealOnMatchDuration);

        if (grid == null) yield break;

        // Swap back to hidden sprite
        foreach (var pos in targetedPositions)
        {
            ElementVisual visual = grid.GetElementVisualAt(pos);
            if (visual != null && visual.IsTargetedByEnemy)
                visual.SetOverrideSprite(hiddenSprite);
        }

        revealCoroutine = null;
    }

    private void HideAllTargets(Grid grid)
    {
        if (grid == null) return;

        foreach (var pos in targetedPositions)
        {
            ElementVisual visual = grid.GetElementVisualAt(pos);
            if (visual != null && visual.IsTargetedByEnemy)
                visual.SetOverrideSprite(hiddenSprite);
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
                visual.ClearOverrideSprite();
        }

        elementsHidden = false;
    }

    private void StopTargetingAnimations(Grid grid)
    {
        if (grid == null) return;

        foreach (var anim in targetingAnimations)
        {
            if (anim != null)
                grid.StopCoroutine(anim);
        }

        targetingAnimations.Clear();
    }

    private void CleanupCoroutines(Grid grid)
    {
        if (grid == null) return;

        if (visibilityCoroutine != null)
        {
            grid.StopCoroutine(visibilityCoroutine);
            visibilityCoroutine = null;
        }

        if (revealCoroutine != null)
        {
            grid.StopCoroutine(revealCoroutine);
            revealCoroutine = null;
        }

        StopTargetingAnimations(grid);
    }
}
