
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
                    //no valid targets -> stop
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
                Vector2Int pos = targetedPositions[0];
                Element element = ownerGrid.GetElementAt(pos);
                List<Vector2Int> finalPositions = new List<Vector2Int> { pos };
                List<Element> elements = new List<Element> { element };

                // Flag to wait until grid processing completes
                bool processingComplete = false;

                ownerGrid.ProcessEnemySpell(finalPositions, () =>
                {
                    DamagePlayerWithAnimation(ownerGrid, finalPositions.ToArray(), elements, damagePerElement, () =>
                    {
                        processingComplete = true;
                    });
                }, excludeBelowTargets);

                ElementVisual vis = ownerGrid.GetElementVisualAt(pos);
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

                //After processing completes, loop continues and the spell will retarget automatically.
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
            return new Vector2Int(-1, -1);

        int i = Random.Range(0, candidates.Count);
        return candidates[i];
    }
}