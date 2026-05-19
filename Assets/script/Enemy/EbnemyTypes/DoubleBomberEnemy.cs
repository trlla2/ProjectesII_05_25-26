using System.Collections.Generic;
using UnityEngine;

public class DoubleBomberSpell : EnemySpellBase
{
    //Only two valid diagonal configurations on a 4x4 grid.
    //neither bomb ever appears in the other's cardinal offsets.
    private static Vector2Int[] DiagConfigA = { new Vector2Int(1, 1), new Vector2Int(2, 2) };
    private static Vector2Int[] DiagConfigB = { new Vector2Int(1, 2), new Vector2Int(2, 1) };

    private static readonly Vector2Int[] CardinalOffsets = new Vector2Int[]
    {
        new Vector2Int(0,  1),
        new Vector2Int(0, -1),
        new Vector2Int(1,  0),
        new Vector2Int(-1, 0)
    };

    private BombInstance bombA;
    private BombInstance bombB;

    public DoubleBomberSpell(float damagePerElement, int moveCount = 5)
    {
        this.damagePerElement = damagePerElement;
        this.remainingMoves = moveCount;
        this.initialMoves = moveCount;
        this.spellName = "Double Bomb";
        this.shouldRerollAfterAttack = true;
    }

    public override void Execute(Grid grid)
    {
        base.Execute(grid);
        if (grid == null) return;

        targetedPositions.Clear();

        Vector2Int[] config = (Random.Range(0, 2) == 0) ? DiagConfigA : DiagConfigB;
        bombA = new BombInstance(config[0]);
        bombB = new BombInstance(config[1]);

        InitializeBomb(grid, bombA);
        InitializeBomb(grid, bombB);

        BombCableVisual[] cables = GetCableVisuals();
        if (cables.Length >= 1) bombA.SetupCableVisual(grid, cables[0]);
        if (cables.Length >= 2) bombB.SetupCableVisual(grid, cables[1]);
    }

    private void InitializeBomb(Grid grid, BombInstance bomb)
    {
        bomb.Reset(CardinalOffsets);

        PlaceBombElement(grid, bomb);

        ElementVisual bombVisual = grid.GetElementVisualAt(bomb.position);
        if (bombVisual != null)
            bombVisual.SetTargetedByEnemy(true);

        targetedPositions.Add(bomb.position);

        foreach (var offset in bomb.activeCardinalOffsets)
        {
            Vector2Int pos = bomb.position + offset;
            if (!grid.IsPositionValid(pos)) continue;

            Element element = grid.GetElementAt(pos);
            // Skip empty and other bomb cells
            if (element == null || element == grid.EmptyElement || element == grid.BombElement) continue;

            bomb.cardinalPositions.Add(pos);
            if (!targetedPositions.Contains(pos))
                targetedPositions.Add(pos);

            ElementVisual visual = grid.GetElementVisualAt(pos);
            if (visual != null)
            {
                visual.SetTargetedByEnemy(true);
                visual.UpdateTargetVisual(initialMoves, initialMoves, grid.EnemyTargetAnimDuration, grid.EnemyTargetFillCurve);
                bomb.targetedVisualsByOffset[offset] = visual;
            }
        }

        RefreshBombVisual(grid, bomb);
    }

    public override void RetargetAfterGravity(Grid grid)
    {
        if (grid == null || bombA == null || bombB == null) return;

        // Detect which cardinal offsets the player actually cleared
        DetectAndMarkClearedOffsets(grid, bombA);
        DetectAndMarkClearedOffsets(grid, bombB);

        ClearAllCurrentTargeting(grid);
        ClearAllCableVisuals();

        // Re-pin or replace bomb cells
        if (bombA.IsDefused)
            ReplaceDefusedBomb(grid, bombA);
        else
            PlaceBombElement(grid, bombA);

        if (bombB.IsDefused)
            ReplaceDefusedBomb(grid, bombB);
        else
            PlaceBombElement(grid, bombB);

        targetedPositions.Clear();

        if (!bombA.IsDefused) RetargetBomb(grid, bombA);
        if (!bombB.IsDefused) RetargetBomb(grid, bombB);

        // Only assign cable visuals to bombs that are still active
        BombCableVisual[] cables = GetCableVisuals();
        int cableIndex = 0;
        if (!bombA.IsDefused && cableIndex < cables.Length)
            bombA.SetupCableVisual(grid, cables[cableIndex++]);
        if (!bombB.IsDefused && cableIndex < cables.Length)
            bombB.SetupCableVisual(grid, cables[cableIndex++]);
    }

    private void DetectAndMarkClearedOffsets(Grid grid, BombInstance bomb)
    {
        List<Vector2Int> copy = new List<Vector2Int>(bomb.activeCardinalOffsets);

        foreach (var offset in copy)
        {
            bool wasCleared = false;

            if (bomb.targetedVisualsByOffset.TryGetValue(offset, out ElementVisual originalVisual))
            {
                if (originalVisual == null || originalVisual.gameObject == null)
                    wasCleared = true;
                else if (!originalVisual.IsTargetedByEnemy)
                    wasCleared = true;
            }
            else
            {
                // No tracked visual — treat as cleared
                wasCleared = true;
            }

            if (wasCleared)
            {
                bomb.activeCardinalOffsets.Remove(offset);
                if (!bomb.clearedCardinalOffsets.Contains(offset))
                    bomb.clearedCardinalOffsets.Add(offset);
                bomb.targetedVisualsByOffset.Remove(offset);
                bomb.cableVisual?.RemoveCable(offset);
            }
        }
    }

    private void RetargetBomb(Grid grid, BombInstance bomb)
    {
        bomb.targetedVisualsByOffset.Clear();
        bomb.cardinalPositions.Clear();

        ElementVisual bombVisual = grid.GetElementVisualAt(bomb.position);
        if (bombVisual != null)
        {
            bombVisual.SetTargetedByEnemy(true);
            bombVisual.SetSelectable(false);
            targetedPositions.Add(bomb.position);
        }

        foreach (var offset in bomb.activeCardinalOffsets)
        {
            Vector2Int pos = bomb.position + offset;
            if (!grid.IsPositionValid(pos)) continue;

            Element element = grid.GetElementAt(pos);
            if (element == null || element == grid.EmptyElement || element == grid.BombElement) continue;

            bomb.cardinalPositions.Add(pos);
            if (!targetedPositions.Contains(pos))
                targetedPositions.Add(pos);

            ElementVisual visual = grid.GetElementVisualAt(pos);
            if (visual != null)
            {
                visual.SetTargetedByEnemy(true);
                visual.UpdateTargetVisual(initialMoves, initialMoves, grid.EnemyTargetAnimDuration, grid.EnemyTargetFillCurve);
                bomb.targetedVisualsByOffset[offset] = visual;
            }
        }

        RefreshBombVisual(grid, bomb);
    }

    public override void Tick(Grid grid, System.Action onComplete = null)
    {
        if (grid == null) return;

        // Both bombs defused by player — no damage, end early
        if (bombA.IsDefused && bombB.IsDefused)
        {
            FullCleanup(grid);
            onComplete?.Invoke();
            return;
        }

        remainingMoves--;
        remainingMoves = Mathf.Max(0, remainingMoves);

        if (!bombA.IsDefused) TickBombVisuals(grid, bombA);
        if (!bombB.IsDefused) TickBombVisuals(grid, bombB);

        if (remainingMoves <= 0)
        {
            Debug.Log("[DoubleBomber] Bombs exploded!");

            if (!bombA.IsDefused) TriggerExplosion(grid, bombA);
            if (!bombB.IsDefused) TriggerExplosion(grid, bombB);

            // Clear visuals for both bombs — defused bombs may still have
            // residual cardinalPositions visuals that need sweeping
            ClearBombVisual(grid, bombA);
            ClearBombVisual(grid, bombB);

            // RemoveBombElement is self-guarded: skips if cell is not BombElement
            RemoveBombElement(grid, bombA);
            RemoveBombElement(grid, bombB);

            ClearAllCableVisuals();

            // Final sweep — catches any lingering targeted visuals on the grid
            ClearAllCurrentTargeting(grid);

            SelectExplosionTargets(grid);
            ApplyEffect(grid, onComplete, true);
        }
    }

    private void TickBombVisuals(Grid grid, BombInstance bomb)
    {
        if (bomb.IsDefused) return;

        foreach (var offset in bomb.activeCardinalOffsets)
        {
            Vector2Int pos = bomb.position + offset;
            if (!grid.IsPositionValid(pos)) continue;
            ElementVisual visual = grid.GetElementVisualAt(pos);
            if (visual != null && visual.IsTargetedByEnemy)
                visual.UpdateTargetVisual(initialMoves, initialMoves, grid.EnemyTargetAnimDuration, grid.EnemyTargetFillCurve);
        }

        RefreshBombVisual(grid, bomb);
    }

    private void TriggerExplosion(Grid grid, BombInstance bomb)
    {
        grid.explosionTrigger.Explode(grid.VisualGrid.GridToWorldPosition(bomb.position));
        Camera.main.GetComponent<CameraShake>().Shake(0.4f, 0.5f);
    }

    private void SelectExplosionTargets(Grid grid)
    {
        targetedPositions.Clear();

        foreach (var bomb in new[] { bombA, bombB })
        {
            if (bomb.IsDefused) continue;

            foreach (var offset in CardinalOffsets)
            {
                Vector2Int pos = bomb.position + offset;
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
    }

    public override void Cleanup(Grid grid)
    {
        if (grid == null || bombA == null) return;
        RemoveBombElement(grid, bombA);
        RemoveBombElement(grid, bombB);
        ClearAllCableVisuals();
    }


    //RePlaces the bomb element at its fixed position
    private void PlaceBombElement(Grid grid, BombInstance bomb)
    {
        grid.SetElementAt(bomb.position, grid.BombElement);
        ElementVisual visual = grid.GetElementVisualAt(bomb.position);
        if (visual != null)
        {
            visual.UpdateVisual(grid.BombElement);
            visual.SetSelectable(false);
        }
    }

    // Replaces a defused bomb cell with a random playable element
    private void ReplaceDefusedBomb(Grid grid, BombInstance bomb)
    {
        Element replacement = grid.PickWeightedRandom(grid.SpawnableElements);
        grid.SetElementAt(bomb.position, replacement);
        ElementVisual visual = grid.GetElementVisualAt(bomb.position);
        if (visual != null)
        {
            visual.SetBombVisual(false, 0f);
            visual.UpdateVisual(replacement);
            visual.SetSelectable(true);
            if (visual.IsTargetedByEnemy)
                visual.SetTargetedByEnemy(false);
        }
    }

    private void RefreshBombVisual(Grid grid, BombInstance bomb)
    {
        ElementVisual visual = grid.GetElementVisualAt(bomb.position);
        if (visual == null) return;
        float progress = 1f - ((float)remainingMoves / initialMoves);
        visual.SetBombVisual(true, progress);
    }

    private void ClearBombVisual(Grid grid, BombInstance bomb)
    {
        ElementVisual bombVisual = grid.GetElementVisualAt(bomb.position);
        if (bombVisual != null)
        {
            bombVisual.SetBombVisual(false, 0f);
            if (bombVisual.IsTargetedByEnemy)
                bombVisual.SetTargetedByEnemy(false);
        }

        foreach (var pos in bomb.cardinalPositions)
        {
            ElementVisual visual = grid.GetElementVisualAt(pos);
            if (visual != null && visual.IsTargetedByEnemy)
            {
                visual.ClearTargetVisual();
                visual.SetTargetedByEnemy(false);
            }
        }
    }

    private void RemoveBombElement(Grid grid, BombInstance bomb)
    {
        if (grid.GetElementAt(bomb.position) != grid.BombElement) return;
        grid.InstantVisualEmpty(new List<Vector2Int> { bomb.position });
        ElementVisual visual = grid.GetElementVisualAt(bomb.position);
        if (visual != null) visual.SetSelectable(true);
    }

    private void ClearAllCurrentTargeting(Grid grid)
    {
        for (int x = 0; x < grid.GridSize; x++)
            for (int y = 0; y < grid.GridSize; y++)
            {
                var pos = new Vector2Int(x, y);
                ElementVisual visual = grid.GetElementVisualAt(pos);
                if (visual != null && visual.IsTargetedByEnemy)
                {
                    visual.ClearTargetVisual();
                    visual.SetTargetedByEnemy(false);
                }
            }
    }

    private BombCableVisual[] GetCableVisuals()
    {
        EnemyBehaviour enemy = GameManager.Instance?.CombatHandler?.GetEnemyRef();
        return enemy != null ? enemy.GetComponentsInChildren<BombCableVisual>() : new BombCableVisual[0];
    }

    private void ClearAllCableVisuals()
    {
        if (bombA?.cableVisual != null) { bombA.cableVisual.ClearAllLines(); bombA.cableVisual = null; }
        if (bombB?.cableVisual != null) { bombB.cableVisual.ClearAllLines(); bombB.cableVisual = null; }
    }

    private void FullCleanup(Grid grid)
    {
        ClearBombVisual(grid, bombA);
        ClearBombVisual(grid, bombB);
        RemoveBombElement(grid, bombA);
        RemoveBombElement(grid, bombB);
        ClearAllCableVisuals();
        targetedPositions.Clear();
    }

    private class BombInstance
    {
        public Vector2Int position;
        public List<Vector2Int> activeCardinalOffsets  = new List<Vector2Int>();
        public List<Vector2Int> clearedCardinalOffsets = new List<Vector2Int>();
        public List<Vector2Int> cardinalPositions      = new List<Vector2Int>();
        public Dictionary<Vector2Int, ElementVisual> targetedVisualsByOffset = new Dictionary<Vector2Int, ElementVisual>();
        public BombCableVisual cableVisual;

        public bool IsDefused => activeCardinalOffsets.Count == 0;

        public BombInstance(Vector2Int pos) { position = pos; }

        public void Reset(Vector2Int[] offsets)
        {
            activeCardinalOffsets  = new List<Vector2Int>(offsets);
            clearedCardinalOffsets.Clear();
            cardinalPositions.Clear();
            targetedVisualsByOffset.Clear();
        }

        public void SetupCableVisual(Grid grid, BombCableVisual visual)
        {
            if (visual == null) return;
            cableVisual = visual;

            if (activeCardinalOffsets.Count + clearedCardinalOffsets.Count < 4) return;

            List<Vector2Int> allOffsets = new List<Vector2Int>(activeCardinalOffsets);
            allOffsets.AddRange(clearedCardinalOffsets);

            cableVisual.Initialize(grid, position, allOffsets);

            foreach (var cleared in clearedCardinalOffsets)
                cableVisual.RemoveCable(cleared);
        }
    }
}