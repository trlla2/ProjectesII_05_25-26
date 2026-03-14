using System.Collections.Generic;
using UnityEngine;

public class BombSpell : EnemySpellBase
{
    private Vector2Int bombPosition = new Vector2Int(-1, -1);
    private List<Vector2Int> cardinalPositions = new List<Vector2Int>();
    private List<Vector2Int> clearedPositions = new List<Vector2Int>();
    private BombCableVisual cableVisual;

    private Vector2Int[] CardinalOffsets = new Vector2Int[]
    {
        new Vector2Int(0, 1), 
        new Vector2Int(0, -1),
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0)  
    };

    private Vector2Int[] AllAdjacentOffsets = new Vector2Int[]
    {
        new Vector2Int(0, 1),
        new Vector2Int(0, -1),
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(1, 1),
        new Vector2Int(-1, 1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, -1)
    };

    public BombSpell(float damagePerElement, int moveCount = 4)
    {
        this.damagePerElement = damagePerElement;
        this.remainingMoves = moveCount;
        this.initialMoves = moveCount;
        this.spellName = "Bomb";
        this.shouldRerollAfterAttack = true;
    }

    public override void Execute(Grid grid)
    {
        if (grid == null) return;

        targetedPositions.Clear();
        cardinalPositions.Clear();
        clearedPositions.Clear();

        bombPosition = PickRandomInnerPosition(grid);
        if (bombPosition.x == -1) return;

        PlaceBombElement(grid);

        ElementVisual bombVisual = grid.GetElementVisualAt(bombPosition);
        if (bombVisual != null)
            bombVisual.SetTargetedByEnemy(true);

        foreach (var offset in CardinalOffsets)
        {
            Vector2Int pos = bombPosition + offset;
            if (!grid.IsPositionValid(pos)) continue;

            Element element = grid.GetElementAt(pos);
            if (element != null && element != grid.EmptyElement)
            {
                cardinalPositions.Add(pos);
                ElementVisual visual = grid.GetElementVisualAt(pos);
                if (visual != null)
                {
                    visual.SetTargetedByEnemy(true);
                    visual.UpdateTargetVisual(remainingMoves, initialMoves,
                        grid.EnemyTargetAnimDuration, grid.EnemyTargetFillCurve);
                }
            }
        }
        UpdateBombVisual(grid);
        InitializeCableVisual(grid);
    }

    public override void RetargetAfterGravity(Grid grid)
    {
        if (grid == null) return;

        Vector2Int newBombPos = FindBombPosition(grid);
        if (newBombPos.x == -1)
        {
            return;
        }

        // If bomb didn't move, just do the default retarget for cardinals
        if (newBombPos == bombPosition)
        {
            RebuildCardinalTracking(grid);
            return;
        }

        ClearAllCurrentTargeting(grid);

        bombPosition = newBombPos;
        cardinalPositions.Clear();
        clearedPositions.Clear();
        targetedPositions.Clear();

        // Re-target bomb visual
        ElementVisual bombVisual = grid.GetElementVisualAt(bombPosition);
        if (bombVisual != null)
        {
            bombVisual.SetTargetedByEnemy(true);
            bombVisual.SetSelectable(false);
        }

        foreach (var offset in CardinalOffsets)
        {
            Vector2Int pos = bombPosition + offset;
            if (!grid.IsPositionValid(pos)) continue;

            Element element = grid.GetElementAt(pos);
            if (element != null && element != grid.EmptyElement)
            {
                cardinalPositions.Add(pos);
                ElementVisual visual = grid.GetElementVisualAt(pos);
                if (visual != null)
                {
                    visual.SetTargetedByEnemy(true);
                    visual.UpdateTargetVisual(remainingMoves, initialMoves,
                        grid.EnemyTargetAnimDuration, grid.EnemyTargetFillCurve);
                }
            }
        }

        targetedPositions.Add(bombPosition);
        targetedPositions.AddRange(cardinalPositions);

        // Refresh visuals
        UpdateBombVisual(grid);
        ReinitializeCableVisual(grid);
    }

    public override void Tick(Grid grid, System.Action onComplete = null)
    {
        if (grid == null) return;

        int clearedCount = GetClearedCardinalCount(grid);

        if (clearedCount >= 3)
        {
            Debug.Log("Bomb defused! 3 targets cleared.");
            ClearBombVisual(grid);
            RemoveBombElement(grid);
            ClearCableVisual();
            targetedPositions.Clear();
            cardinalPositions.Clear();
            clearedPositions.Clear();
            onComplete?.Invoke(); // triggers RerollAndRetarget in EnemyBehaviour
            return;
        }

        remainingMoves--;
        remainingMoves = Mathf.Max(0, remainingMoves);

        //Update countdown indicators
        float animDuration = grid.EnemyTargetAnimDuration;
        AnimationCurve curve = grid.EnemyTargetFillCurve;
        foreach (var pos in cardinalPositions)
        {
            ElementVisual visual = grid.GetElementVisualAt(pos);
            if (visual != null && visual.IsTargetedByEnemy)
                visual.UpdateTargetVisual(remainingMoves, initialMoves, animDuration, curve);
        }

        UpdateBombVisual(grid);

        if (remainingMoves <= 0)
        {
            Debug.Log("Bomb exploded!");
            grid.explosionTrigger.Explode(grid.VisualGrid.GridToWorldPosition(bombPosition - new Vector2Int(1,0)));
            Camera.main.GetComponent<CameraShake>().Shake(0.4f, 0.5f);

            // Remove bomb element before explosion so EnemySpellState refills the slot
            ClearBombVisual(grid);
            RemoveBombElement(grid);
            ClearCableVisual();

            SelectExplosionTargets(grid);
            ApplyEffect(grid, onComplete,true);
        }
    }

    private Vector2Int FindBombPosition(Grid grid)
    {
        Element bombEl = grid.BombElement;
        if (bombEl == null) return new Vector2Int(-1, -1);

        for (int x = 0; x < Grid.GridSize; x++)
        {
            for (int y = 0; y < Grid.GridSize; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (grid.GetElementAt(pos) == bombEl)
                    return pos;
            }
        }
        return new Vector2Int(-1, -1);
    }

    private void ClearAllCurrentTargeting(Grid grid)
    {
        for (int x = 0; x < Grid.GridSize; x++)
        {
            for (int y = 0; y < Grid.GridSize; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                ElementVisual visual = grid.GetElementVisualAt(pos);
                if (visual != null && visual.IsTargetedByEnemy)
                {
                    visual.ClearTargetVisual();
                    visual.SetTargetedByEnemy(false);
                }
            }
        }
    }

    private void RebuildCardinalTracking(Grid grid)
    {
        //The Bomb didn't move, but cardinal elements may have shifted due to gravity.
        foreach (var pos in cardinalPositions)
        {
            ElementVisual visual = grid.GetElementVisualAt(pos);
            if (visual != null && visual.IsTargetedByEnemy)
            {
                visual.ClearTargetVisual();
                visual.SetTargetedByEnemy(false);
            }
        }

        cardinalPositions.Clear();
        clearedPositions.Clear();
        targetedPositions.Clear();
        targetedPositions.Add(bombPosition);

        // Ensure bomb is still targeted
        ElementVisual bombVisual = grid.GetElementVisualAt(bombPosition);
        if (bombVisual != null && !bombVisual.IsTargetedByEnemy)
        {
            bombVisual.SetTargetedByEnemy(true);
            bombVisual.SetSelectable(false);
        }

        foreach (var offset in CardinalOffsets)
        {
            Vector2Int pos = bombPosition + offset;
            if (!grid.IsPositionValid(pos)) continue;

            Element element = grid.GetElementAt(pos);
            if (element != null && element != grid.EmptyElement)
            {
                cardinalPositions.Add(pos);
                targetedPositions.Add(pos);
                ElementVisual visual = grid.GetElementVisualAt(pos);
                if (visual != null)
                {
                    visual.SetTargetedByEnemy(true);
                    visual.UpdateTargetVisual(remainingMoves, initialMoves,
                        grid.EnemyTargetAnimDuration, grid.EnemyTargetFillCurve);
                }
            }
        }

        ReinitializeCableVisual(grid);
    }

    private void ReinitializeCableVisual(Grid grid)
    {
        if (cableVisual != null)
        {
            cableVisual.ClearAllLines();
            if (cardinalPositions.Count >= 4)
                cableVisual.Initialize(grid, bombPosition, cardinalPositions);
        }
    }

    private int GetClearedCardinalCount(Grid grid)
    {
        int cleared = 0;
        List<Vector2Int> newlyCleared = new List<Vector2Int>();

        foreach (var pos in cardinalPositions)
        {
            ElementVisual visual = grid.GetElementVisualAt(pos);
            if (visual == null || !visual.IsTargetedByEnemy)
            {
                // Check if this position was already processed
                if (!clearedPositions.Contains(pos))
                {
                    newlyCleared.Add(pos);
                    clearedPositions.Add(pos);
                }
                cleared++;
            }
        }
        if (cableVisual != null && newlyCleared.Count > 0)
        {
            foreach (var pos in newlyCleared)
            {
                cableVisual.RemoveCable(pos);
            }
        }

        return cleared;
    }

    private void SelectExplosionTargets(Grid grid)
    {
        targetedPositions.Clear();

        foreach (var offset in AllAdjacentOffsets)
        {
            Vector2Int pos = bombPosition + offset;
            if (!grid.IsPositionValid(pos)) continue;

            Element element = grid.GetElementAt(pos);
            if (element == null || element == grid.EmptyElement) continue;

            if (!targetedPositions.Contains(pos))
            {
                targetedPositions.Add(pos);

                ElementVisual visual = grid.GetElementVisualAt(pos);
                if (visual != null && !visual.IsTargetedByEnemy)
                    visual.SetTargetedByEnemy(true);
            }
        }
    }

    private void UpdateBombVisual(Grid grid)
    {
        ElementVisual bombVisual = grid.GetElementVisualAt(bombPosition);
        if (bombVisual == null) return;

        float progress = 1f - ((float)remainingMoves / initialMoves);
        bombVisual.SetBombVisual(true, progress);
    }

    private void InitializeCableVisual(Grid grid)
    {
        if (cardinalPositions.Count < 4) return;

        EnemyBehaviour enemy = GameManager.Instance.CombatHandler.GetEnemyRef();

        if (enemy != null)
        {
            cableVisual = enemy.GetComponentInChildren<BombCableVisual>();
            if (cableVisual != null)
            {
                cableVisual.Initialize(grid, bombPosition, cardinalPositions);
            }
        }
    }

    private void ClearCableVisual()
    {
        if (cableVisual != null)
        {
            cableVisual.ClearAllLines();
            cableVisual = null;
        }
    }

    private void ClearBombVisual(Grid grid)
    {
        ElementVisual bombVisual = grid.GetElementVisualAt(bombPosition);
        if (bombVisual != null)
        {
            bombVisual.SetBombVisual(false, 0f);
            if (bombVisual.IsTargetedByEnemy)
                bombVisual.SetTargetedByEnemy(false);
        }

        foreach (Vector2Int pos in cardinalPositions)
        {
            ElementVisual visual = grid.GetElementVisualAt(pos);
            if (visual != null && visual.IsTargetedByEnemy)
            {
                visual.ClearTargetVisual();
                visual.SetTargetedByEnemy(false);
            }
        }
    }

    private Vector2Int PickRandomInnerPosition(Grid grid)
    {
        List<Vector2Int> inner = new List<Vector2Int>();
        for (int x = 1; x < Grid.GridSize - 1; x++)
            for (int y = 1; y < Grid.GridSize - 1; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (grid.IsPositionValid(pos))
                    inner.Add(pos);
            }

        if (inner.Count == 0)
            return new Vector2Int(-1, -1);

        return inner[Random.Range(0, inner.Count)];
    }

    private void PlaceBombElement(Grid grid)
    {
        Element bombEl = grid.BombElement;
        if (bombEl == null)
        {
            Debug.LogWarning("BombSpell: No bomb Element assigned on Grid.");
            return;
        }

        grid.SetElementAt(bombPosition, bombEl);

        ElementVisual visual = grid.GetElementVisualAt(bombPosition);
        if (visual != null)
        {
            visual.UpdateVisual(bombEl);
            visual.SetSelectable(false);
        }
    }

    private void RemoveBombElement(Grid grid)
    {
        Element bombEl = grid.BombElement;
        if (bombEl == null) return;

        if (grid.GetElementAt(bombPosition) != bombEl) return;

        grid.InstantVisualEmpty(new List<Vector2Int> { bombPosition });

        ElementVisual visual = grid.GetElementVisualAt(bombPosition);
        if (visual != null)
            visual.SetSelectable(true);
    }

    public override void Cleanup(Grid grid)
    {
        if (grid == null || bombPosition.x == -1) return;
        RemoveBombElement(grid);
        ClearCableVisual();
        clearedPositions.Clear();
    }
}
