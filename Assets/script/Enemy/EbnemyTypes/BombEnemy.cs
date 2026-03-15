using System.Collections.Generic;
using UnityEngine;

public class BombSpell : EnemySpellBase
{
    private Vector2Int bombPosition = new Vector2Int(-1, -1);
    
    private List<Vector2Int> activeCardinalOffsets = new List<Vector2Int>();
    private List<Vector2Int> clearedCardinalOffsets = new List<Vector2Int>();
    private List<Vector2Int> cardinalPositions = new List<Vector2Int>();
    private BombCableVisual cableVisual;

    //This became more complicated than it should, but with the gravity now moving verything around we have to track which visuals correspond to which offsets in order to determine if the player cleared them or if they just moved due to gravity. This dictionary allows us to check if the visual we originally targeted for a given offset still exists after gravity.
    private Dictionary<Vector2Int, ElementVisual> targetedVisualsByOffset = new Dictionary<Vector2Int, ElementVisual>();

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
        activeCardinalOffsets = new List<Vector2Int>(CardinalOffsets);
        clearedCardinalOffsets.Clear();
        targetedVisualsByOffset.Clear();

        bombPosition = PickRandomInnerPosition(grid);
        if (bombPosition.x == -1) return;

        PlaceBombElement(grid);

        ElementVisual bombVisual = grid.GetElementVisualAt(bombPosition);
        if (bombVisual != null)
            bombVisual.SetTargetedByEnemy(true);

        targetedPositions.Add(bombPosition);

        foreach (var offset in activeCardinalOffsets)
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
                    
                    targetedVisualsByOffset[offset] = visual;
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
        if (newBombPos.x == -1) return;

        // Check which offsets had their elements ACTUALLY cleared by the player
        List<Vector2Int> newlyCleared = new List<Vector2Int>();
        List<Vector2Int> activeOffsetsCopy = new List<Vector2Int>(activeCardinalOffsets);

        foreach (var offset in activeOffsetsCopy)
        {
            if (targetedVisualsByOffset.TryGetValue(offset, out ElementVisual originalVisual))
            {
                // Check if the original visual still exists and is valid
                if (originalVisual == null || originalVisual.gameObject == null)
                {
                    // Visual was destroyed; player cleared it
                    newlyCleared.Add(offset);
                    continue;
                }

                // Check if the visual still exists and is still targeted
                // If it's still targeted by enemy, it means it just moved due to gravity, not cleared
                if (originalVisual.IsTargetedByEnemy)
                {
                    // Visual still exists and is targeted
                    continue;
                }
                else
                {
                    //Playher clear
                    newlyCleared.Add(offset);
                }
            }
            else
            {
                // No tracked visual for this offset (shouldn't happen, but handle it)
                Vector2Int pos = bombPosition + offset;
                if (!grid.IsPositionValid(pos))
                {
                    newlyCleared.Add(offset);
                    continue;
                }

                ElementVisual visual = grid.GetElementVisualAt(pos);
                if (visual == null || !visual.IsTargetedByEnemy)
                {
                    newlyCleared.Add(offset);
                }
            }
        }

        // Move newly cleared offsets to cleared list BEFORE retargeting
        foreach (var offset in newlyCleared)
        {
            activeCardinalOffsets.Remove(offset);
            if (!clearedCardinalOffsets.Contains(offset))
            {
                clearedCardinalOffsets.Add(offset);
            }
            
            targetedVisualsByOffset.Remove(offset);

            if (cableVisual != null)
            {
                cableVisual.RemoveCable(offset);
            }
        }

        ClearAllCurrentTargeting(grid);
        ClearCableVisual();
        
        bombPosition = newBombPos;
        targetedPositions.Clear();
        cardinalPositions.Clear();

        ElementVisual bombVisual = grid.GetElementVisualAt(bombPosition);
        if (bombVisual != null)
        {
            bombVisual.SetTargetedByEnemy(true);
            bombVisual.SetSelectable(false);
            targetedPositions.Add(bombPosition);
        }

        List<Vector2Int> additionalCleared = new List<Vector2Int>();
        
        foreach (var offset in activeCardinalOffsets)
        {
            Vector2Int pos = bombPosition + offset;
            bool validTarget = false;
            
            if (grid.IsPositionValid(pos))
            {
                Element element = grid.GetElementAt(pos);
                if (element != null && element != grid.EmptyElement)
                    validTarget = true;
            }

            if (!validTarget)
            {
                // Position became invalid after bomb moved
                additionalCleared.Add(offset);
            }
            else
            {
                // Valid element found - retarget it
                cardinalPositions.Add(pos);
                targetedPositions.Add(pos);
                ElementVisual visual = grid.GetElementVisualAt(pos);
                if (visual != null)
                {
                    visual.SetTargetedByEnemy(true);
                    visual.UpdateTargetVisual(remainingMoves, initialMoves,
                        grid.EnemyTargetAnimDuration, grid.EnemyTargetFillCurve);
                    
                    // Update tracked visual for this offset
                    targetedVisualsByOffset[offset] = visual;
                }
            }
        }

        // Clear any additional offsets that became invalid
        foreach (var offset in additionalCleared)
        {
            activeCardinalOffsets.Remove(offset);
            if (!clearedCardinalOffsets.Contains(offset))
            {
                clearedCardinalOffsets.Add(offset);
            }
            targetedVisualsByOffset.Remove(offset);
        }

        UpdateBombVisual(grid);
        InitializeCableVisual(grid);
    }

    public override void Tick(Grid grid, System.Action onComplete = null)
    {
        if (grid == null) return;

        if (activeCardinalOffsets.Count <= 1)
        {
            Debug.Log("Bomb defused! 3+ cables cleared.");
            ClearBombVisual(grid);
            RemoveBombElement(grid);
            ClearCableVisual();
            targetedPositions.Clear();
            cardinalPositions.Clear();
            activeCardinalOffsets.Clear();
            clearedCardinalOffsets.Clear();
            targetedVisualsByOffset.Clear();
            onComplete?.Invoke();
            return;
        }

        remainingMoves--;
        remainingMoves = Mathf.Max(0, remainingMoves);

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

            ClearBombVisual(grid);
            RemoveBombElement(grid);
            ClearCableVisual();

            SelectExplosionTargets(grid);
            ApplyEffect(grid, onComplete, true);
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
        // Only initialize if we have tracking data
        if (activeCardinalOffsets.Count + clearedCardinalOffsets.Count < 4) return;

        EnemyBehaviour enemy = GameManager.Instance.CombatHandler.GetEnemyRef();

        if (enemy != null)
        {
            cableVisual = enemy.GetComponentInChildren<BombCableVisual>();
            if (cableVisual != null)
            {
                List<Vector2Int> allOffsets = new List<Vector2Int>(activeCardinalOffsets);
                allOffsets.AddRange(clearedCardinalOffsets);

                // Initialize with all 4 original cardinal directions
                cableVisual.Initialize(grid, bombPosition, allOffsets);

                // Remove cables that were already cleared
                foreach (Vector2Int clearedOffset in clearedCardinalOffsets)
                {
                    cableVisual.RemoveCable(clearedOffset);
                }
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
        if (bombEl == null) return;

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
        clearedCardinalOffsets.Clear();
        targetedVisualsByOffset.Clear();
    }
}
