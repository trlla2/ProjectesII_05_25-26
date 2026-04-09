using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SwapperSpell : IEnemySpell
{
    private ContinuouslyTimedSpell activeAttackSpell;
    private Grid cachedGrid;
    private bool isActive;

    private float swapInterval;
    private Sequence swapSequence;

    private const float SwapAnimDuration = 0.3f;
    private const float SwapPreviewDuration = 4.00f;

    public int RemainingMoves => 0;
    public bool ShouldRerollAfterAttack => false;
    public bool IsActive => isActive;
    public ContinuouslyTimedSpell AttackSpell => activeAttackSpell;

    public SwapperSpell(
        float selectionInterval = 4f,
        float timer = 8f,
        float damage = 4f,
        float bonusTime = 3f,
        float swapInterval = 2.5f)
    {
        this.swapInterval = swapInterval;
        activeAttackSpell = new ContinuouslyTimedSpell(selectionInterval, timer, damage, bonusTime);
    }

    public override string GetSpellName() => "Swapper";

    public void Execute(Grid grid)
    {
        cachedGrid = grid;
        isActive = true;
        StartSwapLoop();
    }

    public void SetupAttackSpell(Grid grid, EnemySpellTimer enemyTimer)
    {
        activeAttackSpell.SetIntervalTimer(enemyTimer);
        activeAttackSpell.Execute(grid);
        enemyTimer.Initialize(activeAttackSpell.SelectionInterval, grid, activeAttackSpell.OnIntervalTick);
        enemyTimer.StartTimer();
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

        var pairs = PickSwapPairs(cachedGrid);
        if (pairs.Count == 0) return;

        cachedGrid.ProcessEnemySwap(pairs, SwapAnimDuration, SwapPreviewDuration);
    }

    private List<(Vector2Int, Vector2Int)> PickSwapPairs(Grid grid)
    {
        var pairs = new List<(Vector2Int, Vector2Int)>();
        var used = new HashSet<Vector2Int>();
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        HashSet<Vector2Int> excluded = new HashSet<Vector2Int>();
        if (activeAttackSpell != null && activeAttackSpell.TargetedPositions != null)
        {
            foreach (var pos in activeAttackSpell.TargetedPositions)
                excluded.Add(pos);
        }

        for (int i = 0; i < 2; i++)
        {
            List<Vector2Int> candidates = new List<Vector2Int>();
            for (int x = 0; x < Grid.GridSize; x++)
            {
                for (int y = 0; y < Grid.GridSize; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    if (used.Contains(pos) || excluded.Contains(pos)) continue;
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
                    if (used.Contains(adj) || excluded.Contains(adj)) continue;
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
                    pairs.Add((pos, adj));
                    used.Add(pos);
                    used.Add(adj);
                    found = true;
                    break;
                }
            }

            if (!found) break;
        }

        return pairs;
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
        activeAttackSpell?.PauseAllTargets();
        activeAttackSpell?.IntervalTimer?.Pause();
        PauseSwapLoop();
    }

    public void ResumeAllTimers()
    {
        activeAttackSpell?.ResumeAllTargets();
        activeAttackSpell?.IntervalTimer?.Resume();
        ResumeSwapLoop();
    }

    public override void Tick(Grid grid, System.Action onComplete = null)
    {
        activeAttackSpell?.Tick(grid, onComplete);
    }

    public override void Cleanup(Grid grid)
    {
        isActive = false;

        swapSequence?.Kill();
        swapSequence = null;

        activeAttackSpell?.Cleanup(grid);
        activeAttackSpell = null;

        cachedGrid = null;
    }

    public override void RetargetAfterGravity(Grid grid)
    {
        activeAttackSpell?.RetargetAfterGravity(grid);
    }
}