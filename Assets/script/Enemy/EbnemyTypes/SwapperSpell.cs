using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SwapperSpell : EnemySpellBase
{
    private Grid cachedGrid;
    private bool isActive;

    private float swapInterval;
    private Sequence swapSequence;

    private bool isSwapPending;

    private const float SwapAnimDuration = 0.2f;
    private float SwapPreviewDuration;

    public bool IsActive => isActive;

    // Captures both position and element reference at the moment of targeting
    private struct SwapTarget
    {
        public Vector2Int PosA;
        public Element ElA;
        public Vector2Int PosB;
        public Element ElB;

        public SwapTarget(Vector2Int posA, Element elA, Vector2Int posB, Element elB)
        {
            PosA = posA; ElA = elA; PosB = posB; ElB = elB;
        }
    }

    public SwapperSpell(float swapInterval = 2.5f, float damage = 4f, float PreviewDuration = 6.0f)
    {
        this.swapInterval = swapInterval;
        this.damagePerElement = damage;
        this.SwapPreviewDuration = PreviewDuration;
        this.remainingMoves = 0;
        this.initialMoves = 0;
        this.shouldRerollAfterAttack = false;
        this.spellName = "Swapper";
    }

    public override void Execute(Grid grid)
    {
        base.Execute(grid);
        cachedGrid = grid;
        isActive = true;
        isSwapPending = false;
        targetedPositions.Clear();
        StartSwapLoop();
    }

    private void StartSwapLoop()
    {
        swapSequence?.Kill();
        swapSequence = DOTween.Sequence();
        swapSequence.AppendInterval(swapInterval);
        swapSequence.AppendCallback(TryPerformSwap);
        swapSequence.SetLoops(-1);
    }

    private void TryPerformSwap()
    {
        if (!isActive || cachedGrid == null) return;
        if (cachedGrid.CurrentState != GridStateType.Idle) return;
        if (isSwapPending) return;

        isSwapPending = true;
        cachedGrid.StartCoroutine(PreviewThenSwap(PickSwapPairs(cachedGrid)));
    }

    private IEnumerator PreviewThenSwap(List<SwapTarget> targets)
    {
        while (isActive && cachedGrid != null)
        {
            // No candidates yet: wait then re-pick before attempting a preview
            if (targets.Count == 0)
            {
                yield return new WaitForSeconds(swapInterval);
                if (!isActive || cachedGrid == null) break;

                while (isActive && cachedGrid != null && cachedGrid.CurrentState != GridStateType.Idle)
                    yield return null;
                if (!isActive || cachedGrid == null) break;

                targets = PickSwapPairs(cachedGrid);
                continue;
            }

            // Phase 1 – warning blink while the grid remains in IdleState
            yield return cachedGrid.StartCoroutine(
                cachedGrid.VisualGrid.PreviewSwapTargets(ToPositionPairs(targets), SwapPreviewDuration)
            );
            if (!isActive || cachedGrid == null) break;

            // Wait for Idle in case the player cast a spell mid-preview
            while (isActive && cachedGrid != null && cachedGrid.CurrentState != GridStateType.Idle)
                yield return null;
            if (!isActive || cachedGrid == null) break;

            // Phase 2 – validate by element identity RIGHT BEFORE swapping
            targets = FilterValidPairs(targets);

            bool swapPerformed = false;
            if (targets.Count > 0)
            {
                // Clear previous targeted positions and mark swapped elements as targeted
                targetedPositions.Clear();
                
                foreach (var target in targets)
                {
                    targetedPositions.Add(target.PosA);
                    targetedPositions.Add(target.PosB);

                    ElementVisual visA = cachedGrid.GetElementVisualAt(target.PosA);
                    ElementVisual visB = cachedGrid.GetElementVisualAt(target.PosB);

                    if (visA != null)
                        visA.SetTargetedByEnemy(true);
                    if (visB != null)
                        visB.SetTargetedByEnemy(true);
                }

                // Perform the swap
                cachedGrid.ProcessEnemySwap(ToPositionPairs(targets), SwapAnimDuration);
                swapPerformed = true;

                // Wait for SwapState -> ValidateSpellState -> IdleState to settle
                while (isActive && cachedGrid != null && cachedGrid.CurrentState != GridStateType.Idle)
                    yield return null;
                if (!isActive || cachedGrid == null) break;

                // Pause the swap loop while we process the attack
                PauseSwapLoop();

                // Set remainingMoves to 0 to trigger ApplyEffect in Tick
                remainingMoves = 0;

                // Call Tick which will trigger ApplyEffect since remainingMoves <= 0
                bool attackComplete = false;
                Tick(cachedGrid, () =>
                {
                    attackComplete = true;
                });

                // Wait for attack to complete
                while (!attackComplete)
                {
                    if (!isActive || cachedGrid == null)
                        yield break;
                    yield return null;
                }

                // Wait for grid to settle after gravity/refill
                while (isActive && cachedGrid != null && cachedGrid.CurrentState != GridStateType.Idle)
                    yield return null;
                if (!isActive || cachedGrid == null) break;

                // Resume the swap loop
                ResumeSwapLoop();
            }

            // If nothing was swapped, insert a cooldown so the routine doesn't re-preview immediately
            if (!swapPerformed)
            {
                yield return new WaitForSeconds(swapInterval);
                if (!isActive || cachedGrid == null) break;

                while (isActive && cachedGrid != null && cachedGrid.CurrentState != GridStateType.Idle)
                    yield return null;
                if (!isActive || cachedGrid == null) break;
            }

            // Always pick fresh targets at the end of each cycle
            targets = PickSwapPairs(cachedGrid);
        }

        isSwapPending = false;
    }

    private static List<(Vector2Int, Vector2Int)> ToPositionPairs(List<SwapTarget> targets)
    {
        var result = new List<(Vector2Int, Vector2Int)>(targets.Count);
        foreach (var t in targets)
            result.Add((t.PosA, t.PosB));
        return result;
    }

    private List<SwapTarget> FilterValidPairs(List<SwapTarget> targets)
    {
        var valid = new List<SwapTarget>();
        foreach (var t in targets)
        {
            if (IsPairValid(t.PosA, t.ElA, t.PosB, t.ElB))
                valid.Add(t);
        }
        return valid;
    }

    private bool IsPairValid(Vector2Int posA, Element expectedA, Vector2Int posB, Element expectedB)
    {
        Element elA = cachedGrid.GetElementAt(posA);
        Element elB = cachedGrid.GetElementAt(posB);

        if (elA == null || elA == cachedGrid.EmptyElement || elA != expectedA) return false;
        if (elB == null || elB == cachedGrid.EmptyElement || elB != expectedB) return false;

        ElementVisual visA = cachedGrid.GetElementVisualAt(posA);
        ElementVisual visB = cachedGrid.GetElementVisualAt(posB);

        if (visA != null && visA.IsBlocked) return false;
        if (visB != null && visB.IsBlocked) return false;

        return true;
    }

    private List<SwapTarget> PickSwapPairs(Grid grid)
    {
        var targets = new List<SwapTarget>();
        var used = new HashSet<Vector2Int>();
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        for (int i = 0; i < 2; i++)
        {
            List<Vector2Int> candidates = new List<Vector2Int>();
            for (int x = 0; x < Grid.GridSize; x++)
            {
                for (int y = 0; y < Grid.GridSize; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    if (used.Contains(pos)) continue;
                    if (!grid.IsPositionValid(pos)) continue;

                    Element element = grid.GetElementAt(pos);
                    if (element == null || element == grid.EmptyElement) continue;

                    ElementVisual visual = grid.GetElementVisualAt(pos);
                    if (visual != null && visual.IsBlocked) continue;

                    candidates.Add(pos);
                }
            }

            if (candidates.Count == 0) break;

            for (int j = candidates.Count - 1; j > 0; j--)
            {
                int k = Random.Range(0, j + 1);
                var temp = candidates[j];
                candidates[j] = candidates[k];
                candidates[k] = temp;
            }

            bool found = false;
            foreach (var pos in candidates)
            {
                if (used.Contains(pos)) continue;

                List<Vector2Int> adjacentCandidates = new List<Vector2Int>();
                foreach (var dir in directions)
                {
                    Vector2Int adj = pos + dir;
                    if (used.Contains(adj)) continue;
                    if (!grid.IsPositionValid(adj)) continue;

                    Element adjElement = grid.GetElementAt(adj);
                    if (adjElement == null || adjElement == grid.EmptyElement) continue;

                    ElementVisual adjVisual = grid.GetElementVisualAt(adj);
                    if (adjVisual != null && adjVisual.IsBlocked) continue;

                    adjacentCandidates.Add(adj);
                }

                if (adjacentCandidates.Count > 0)
                {
                    Vector2Int adj = adjacentCandidates[Random.Range(0, adjacentCandidates.Count)];
                    targets.Add(new SwapTarget(pos, grid.GetElementAt(pos), adj, grid.GetElementAt(adj)));
                    used.Add(pos);
                    used.Add(adj);
                    found = true;
                    break;
                }
            }

            if (!found) break;
        }

        return targets;
    }

    public void PauseSwapLoop()
    {
        swapSequence?.Pause();
    }

    public void ResumeSwapLoop()
    {
        swapSequence?.Play();
    }

    public void PauseAllTimers()
    {
        PauseSwapLoop();
    }

    public void ResumeAllTimers()
    {
        ResumeSwapLoop();
    }

    public override void Cleanup(Grid grid)
    {
        isActive = false;
        isSwapPending = false;

        swapSequence?.Kill();
        swapSequence = null;

        // Clear any targeted visuals
        if (grid != null)
        {
            foreach (var pos in targetedPositions)
            {
                ElementVisual visual = grid.GetElementVisualAt(pos);
                if (visual != null && visual.IsTargetedByEnemy)
                {
                    visual.SetTargetedByEnemy(false);
                    visual.StopHoverAnimation();
                }
            }
        }

        targetedPositions.Clear();
        cachedGrid = null;
    }
}