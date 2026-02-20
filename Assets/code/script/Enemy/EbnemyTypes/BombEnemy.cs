using System.Collections.Generic;
using UnityEngine;

public class BombSpell : EnemySpellBase
{
    private Vector2Int bombPosition = new Vector2Int(-1, -1);
    private List<Vector2Int> cardinalPositions = new List<Vector2Int>();

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

    public BombSpell(float damagePerElement, int moveCount = 4, bool excludeBelow = false)
    {
        this.damagePerElement = damagePerElement;
        this.remainingMoves = moveCount;
        this.initialMoves = moveCount;
        this.spellName = "Bomb";
        this.excludeBelowTargets = excludeBelow;
        this.shouldRerollAfterAttack = true;
    }

    public override void Execute(Grid grid)
    {
        if (grid == null) return;

        targetedPositions.Clear();
        cardinalPositions.Clear();

        bombPosition = PickRandomInnerPosition(grid);
        if (bombPosition.x == -1) return;

        PlaceBombElement(grid);

        // Mark bomb centre as targeted so StopBattle and ClearBombVisual find it
        ElementVisual bombVisual = grid.GetElementVisualAt(bombPosition);
        if (bombVisual != null)
            bombVisual.SetTargetedByEnemy(true);

        // Mark the 4 cardinal neighbours as defuse targets
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
    }

    public override void Tick(Grid grid, System.Action onComplete = null)
    {
        if (grid == null) return;

        if (AllCardinalsCleared(grid))
        {
            //ElementVisual bombVisual = grid.GetElementVisualAt(bombPosition);
            //if (bombVisual == null) Debug.Log("no bomb visual");
            //bombVisual.explosionTrigger.Explode();

            ClearBombVisual(grid);
            RemoveBombElement(grid);
            targetedPositions.Clear();
            cardinalPositions.Clear();
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
            // Remove bomb element before explosion so EnemySpellState refills the slot
            ClearBombVisual(grid);
            RemoveBombElement(grid);

            SelectExplosionTargets(grid);
            ApplyEffect(grid, onComplete);
        }
    }

    private bool AllCardinalsCleared(Grid grid)
    {
        if (cardinalPositions.Count == 0) return false;

        foreach (var pos in cardinalPositions)
        {
            ElementVisual visual = grid.GetElementVisualAt(pos);
            if (visual != null && visual.IsTargetedByEnemy)
                return false;
        }
        return true;
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

    private void ClearBombVisual(Grid grid)
    {
        ElementVisual bombVisual = grid.GetElementVisualAt(bombPosition);
        if (bombVisual != null)
        {
            bombVisual.SetBombVisual(false, 0f);
            if (bombVisual.IsTargetedByEnemy)
                bombVisual.SetTargetedByEnemy(false);
        }

        foreach (var pos in cardinalPositions)
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
            visual.SetSelectable(false); // Bomb tile cannot be part of a player spell path
        }
    }

    private void RemoveBombElement(Grid grid)
    {
        Element bombEl = grid.BombElement;
        if (bombEl == null) return;

        //only clear if the bomb element is still present at this position
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
    }
}
