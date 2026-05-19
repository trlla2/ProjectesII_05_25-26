using System.Collections.Generic;
using UnityEngine;

public class DoubleBomberSpell : EnemySpellBase
{
    // Only two valid diagonal configurations on a 4x4 grid so the bombs
    // never appear in each other's cardinal offsets.
    private static readonly Vector2Int[] DiagConfigA = { new Vector2Int(1, 1), new Vector2Int(2, 2) };
    private static readonly Vector2Int[] DiagConfigB = { new Vector2Int(1, 2), new Vector2Int(2, 1) };

    private static readonly Vector2Int[] CardinalOffsets =
    {
        new Vector2Int( 0,  1),
        new Vector2Int( 0, -1),
        new Vector2Int( 1,  0),
        new Vector2Int(-1,  0),
    };

    private BombInstance bombA;
    private BombInstance bombB;

    // Prevents RetargetAfterGravity from touching the grid once the spell resolved.
    private bool isResolved = false;

    public DoubleBomberSpell(float damagePerElement, int moveCount = 5)
    {
        this.damagePerElement = damagePerElement;
        this.remainingMoves   = moveCount;
        this.initialMoves     = moveCount;
        this.spellName        = "Double Bomb";
        this.shouldRerollAfterAttack = true;
    }

    public override void Execute(Grid grid)
    {
        base.Execute(grid);
        if (grid == null) return;

        isResolved = false;
        targetedPositions.Clear();

        Vector2Int[] config = (Random.Range(0, 2) == 0) ? DiagConfigA : DiagConfigB;

        bombA = new BombInstance(config[0], initialMoves);
        bombB = new BombInstance(config[1], initialMoves);

        PlaceBomb(grid, bombA);
        PlaceBomb(grid, bombB);

        AssignCableVisuals(grid);
    }


    public override void RetargetAfterGravity(Grid grid)
    {
        if (isResolved || grid == null || bombA == null || bombB == null) return;

        RetargetOneBomb(grid, bombA);
        RetargetOneBomb(grid, bombB);

        // Rebuild shared targetedPositions list.
        RebuildTargetedPositions();

        AssignCableVisuals(grid);
    }

    private void RetargetOneBomb(Grid grid, BombInstance bomb)
    {
        if (bomb.IsExploded) return;

        if (bomb.IsDefused)
        {
            // Already defused – make sure the grid cell holds a real element.
            EnsureDefusedCellIsReplaced(grid, bomb);
            return;
        }

        // Detect which offsets were cleared by the player since last tick.
        DetectClearedOffsets(grid, bomb);

        // If the bomb just became defused, replace its cell and stop.
        if (bomb.IsDefused)
        {
            ReplaceBombWithElement(grid, bomb);
            return;
        }

        // Re-place the bomb element (gravity may have swapped the cell).
        PlaceBombOnGrid(grid, bomb);

        // Re-target remaining active offsets.
        bomb.ClearTargetingData();

        foreach (var offset in bomb.ActiveOffsets)
        {
            Vector2Int pos = bomb.Position + offset;
            if (!grid.IsPositionValid(pos)) continue;

            Element element = grid.GetElementAt(pos);
            if (element == null || element == grid.EmptyElement || element == grid.BombElement) continue;

            ElementVisual visual = grid.GetElementVisualAt(pos);
            if (visual == null) continue;

            visual.SetTargetedByEnemy(true);
            visual.UpdateTargetVisual(initialMoves, initialMoves,
                grid.EnemyTargetAnimDuration, grid.EnemyTargetFillCurve);

            bomb.TrackTarget(offset, pos, element, visual);
        }

        RefreshBombVisual(grid, bomb);

        ElementVisual bombVis = grid.GetElementVisualAt(bomb.Position);
        if (bombVis != null)
        {
            bombVis.SetTargetedByEnemy(true);
            bombVis.SetSelectable(false);
        }
    }


    public override void Tick(Grid grid, System.Action onComplete = null)
    {
        if (grid == null) return;

        // Both bombs defused – clean up and reroll.
        if (bombA.IsDefused && bombB.IsDefused)
        {
            isResolved = true;
            FullCleanup(grid);
            onComplete?.Invoke();
            return;
        }

        bombA.RemainingMoves = Mathf.Max(0, bombA.RemainingMoves - 1);
        bombB.RemainingMoves = Mathf.Max(0, bombB.RemainingMoves - 1);

        // Keep the base remainingMoves in sync with the least-progressed bomb
        // so the base class timer UI stays meaningful.
        remainingMoves = Mathf.Max(
            bombA.IsDefused ? 0 : bombA.RemainingMoves,
            bombB.IsDefused ? 0 : bombB.RemainingMoves);

        TickBombVisuals(grid, bombA);
        TickBombVisuals(grid, bombB);

        bool aExplodes = !bombA.IsDefused && bombA.RemainingMoves <= 0;
        bool bExplodes = !bombB.IsDefused && bombB.RemainingMoves <= 0;

        if (!aExplodes && !bExplodes) return; // Nothing resolves this tick.

        isResolved = true;

        if (aExplodes) ExplodeBomb(grid, bombA);
        if (bExplodes) ExplodeBomb(grid, bombB);

        // Remove bomb elements and cable visuals.
        if (!bombA.IsDefused) RemoveBombElement(grid, bombA);
        if (!bombB.IsDefused) RemoveBombElement(grid, bombB);

        bombA.IsExploded = true;
        bombB.IsExploded = true;

        ClearAllCableVisuals();
        ClearAllTargetingVisuals(grid);

        // Collect explosion targets.
        CollectExplosionTargets(grid);

        ApplyEffect(grid, onComplete, rerollAfter: true);
    }


    public override void Cleanup(Grid grid)
    {
        if (grid == null || bombA == null) return;
        isResolved = true;
        FullCleanup(grid);
    }


    private void PlaceBomb(Grid grid, BombInstance bomb)
    {
        PlaceBombOnGrid(grid, bomb);

        ElementVisual bombVis = grid.GetElementVisualAt(bomb.Position);
        if (bombVis != null)
        {
            bombVis.SetTargetedByEnemy(true);
            bombVis.SetSelectable(false);
        }

        foreach (var offset in bomb.ActiveOffsets)
        {
            Vector2Int pos = bomb.Position + offset;
            if (!grid.IsPositionValid(pos)) continue;

            Element element = grid.GetElementAt(pos);
            if (element == null || element == grid.EmptyElement || element == grid.BombElement) continue;

            ElementVisual visual = grid.GetElementVisualAt(pos);
            if (visual == null) continue;

            visual.SetTargetedByEnemy(true);
            visual.UpdateTargetVisual(initialMoves, initialMoves,
                grid.EnemyTargetAnimDuration, grid.EnemyTargetFillCurve);

            bomb.TrackTarget(offset, pos, element, visual);
        }

        RefreshBombVisual(grid, bomb);
        RebuildTargetedPositions();
    }

    private void PlaceBombOnGrid(Grid grid, BombInstance bomb)
    {
        grid.SetElementAt(bomb.Position, grid.BombElement);
        ElementVisual visual = grid.GetElementVisualAt(bomb.Position);
        if (visual != null)
        {
            visual.UpdateVisual(grid.BombElement);
            visual.SetSelectable(false);
        }
    }

    // Checks each active offset: if the element in that cell changed (cleared
    //by the player), move the offset to the cleared list and cut its cable.
    // A bomb becomes defused when all 4 offsets are cleared.
    private void DetectClearedOffsets(Grid grid, BombInstance bomb)
    {
        List<Vector2Int> toCheck = new List<Vector2Int>(bomb.ActiveOffsets);

        foreach (var offset in toCheck)
        {
            Vector2Int pos = bomb.Position + offset;
            bool cleared = false;

            if (!grid.IsPositionValid(pos))
            {
                cleared = true;
            }
            else
            {
                Element current = grid.GetElementAt(pos);
                Element original = bomb.GetTrackedElement(offset);
                cleared = (original == null || current != original);
            }

            if (cleared)
            {
                bomb.MarkOffsetCleared(offset);
                bomb.CableVisual?.RemoveCable(offset);
            }
        }
    }

    private void ReplaceBombWithElement(Grid grid, BombInstance bomb)
    {
        Element replacement = grid.PickWeightedRandom(grid.SpawnableElements);
        grid.SetElementAt(bomb.Position, replacement);

        ElementVisual visual = grid.GetElementVisualAt(bomb.Position);
        if (visual != null)
        {
            visual.SetBombVisual(false, 0f);
            visual.UpdateVisual(replacement);
            visual.SetSelectable(true);
            if (visual.IsTargetedByEnemy) visual.SetTargetedByEnemy(false);
        }
    }

    private void EnsureDefusedCellIsReplaced(Grid grid, BombInstance bomb)
    {
        // If gravity somehow left a bomb element in a defused slot, fix it.
        if (grid.GetElementAt(bomb.Position) == grid.BombElement)
            ReplaceBombWithElement(grid, bomb);
    }

    private void TickBombVisuals(Grid grid, BombInstance bomb)
    {
        if (bomb.IsDefused || bomb.IsExploded) return;

        foreach (var offset in bomb.ActiveOffsets)
        {
            Vector2Int pos = bomb.Position + offset;
            if (!grid.IsPositionValid(pos)) continue;

            ElementVisual visual = grid.GetElementVisualAt(pos);
            if (visual != null && visual.IsTargetedByEnemy)
                visual.UpdateTargetVisual(initialMoves, initialMoves,
                    grid.EnemyTargetAnimDuration, grid.EnemyTargetFillCurve);
        }

        RefreshBombVisual(grid, bomb);
    }

    private void RefreshBombVisual(Grid grid, BombInstance bomb)
    {
        ElementVisual visual = grid.GetElementVisualAt(bomb.Position);
        if (visual == null) return;
        float progress = 1f - ((float)bomb.RemainingMoves / initialMoves);
        visual.SetBombVisual(true, progress);
    }

    private void ExplodeBomb(Grid grid, BombInstance bomb)
    {
        grid.explosionTrigger.Explode(grid.VisualGrid.GridToWorldPosition(bomb.Position));
        Camera.main.GetComponent<CameraShake>().Shake(0.4f, 0.5f);

        ClearBombCellVisual(grid, bomb);
    }

    private void RemoveBombElement(Grid grid, BombInstance bomb)
    {
        if (grid.GetElementAt(bomb.Position) != grid.BombElement) return;
        grid.InstantVisualEmpty(new List<Vector2Int> { bomb.Position });

        ElementVisual visual = grid.GetElementVisualAt(bomb.Position);
        if (visual != null) visual.SetSelectable(true);
    }

    private void ClearBombCellVisual(Grid grid, BombInstance bomb)
    {
        ElementVisual bombVis = grid.GetElementVisualAt(bomb.Position);
        if (bombVis != null)
        {
            bombVis.SetBombVisual(false, 0f);
            if (bombVis.IsTargetedByEnemy) bombVis.SetTargetedByEnemy(false);
        }

        foreach (var pos in bomb.CardinalPositions)
        {
            ElementVisual visual = grid.GetElementVisualAt(pos);
            if (visual != null && visual.IsTargetedByEnemy)
            {
                visual.ClearTargetVisual();
                visual.SetTargetedByEnemy(false);
            }
        }
    }

    private void CollectExplosionTargets(Grid grid)
    {
        targetedPositions.Clear();

        foreach (var bomb in new[] { bombA, bombB })
        {
            if (bomb.IsDefused) continue;

            foreach (var offset in CardinalOffsets)
            {
                Vector2Int pos = bomb.Position + offset;
                if (!grid.IsPositionValid(pos)) continue;

                Element element = grid.GetElementAt(pos);
                if (element == null || element == grid.EmptyElement) continue;

                if (targetedPositions.Contains(pos)) continue;
                targetedPositions.Add(pos);

                ElementVisual visual = grid.GetElementVisualAt(pos);
                if (visual != null && !visual.IsTargetedByEnemy)
                    visual.SetTargetedByEnemy(true);
            }
        }
    }

    private void RebuildTargetedPositions()
    {
        targetedPositions.Clear();

        foreach (var bomb in new[] { bombA, bombB })
        {
            if (bomb.IsDefused || bomb.IsExploded) continue;

            if (!targetedPositions.Contains(bomb.Position))
                targetedPositions.Add(bomb.Position);

            foreach (var pos in bomb.CardinalPositions)
                if (!targetedPositions.Contains(pos))
                    targetedPositions.Add(pos);
        }
    }

    private void ClearAllTargetingVisuals(Grid grid)
    {
        for (int x = 0; x < grid.GridSize; x++)
            for (int y = 0; y < grid.GridSize; y++)
            {
                ElementVisual visual = grid.GetElementVisualAt(new Vector2Int(x, y));
                if (visual != null && visual.IsTargetedByEnemy)
                {
                    visual.ClearTargetVisual();
                    visual.SetTargetedByEnemy(false);
                }
            }
    }

    private void AssignCableVisuals(Grid grid)
    {
        ClearAllCableVisuals();
        BombCableVisual[] cables = GetCableVisuals();
        int idx = 0;

        if (!bombA.IsDefused && !bombA.IsExploded && idx < cables.Length)
            bombA.SetupCable(grid, cables[idx++]);

        if (!bombB.IsDefused && !bombB.IsExploded && idx < cables.Length)
            bombB.SetupCable(grid, cables[idx]);
    }

    private void ClearAllCableVisuals()
    {
        bombA?.ClearCable();
        bombB?.ClearCable();
    }

    private void FullCleanup(Grid grid)
    {
        ClearBombCellVisual(grid, bombA);
        ClearBombCellVisual(grid, bombB);
        RemoveBombElement(grid, bombA);
        RemoveBombElement(grid, bombB);
        ClearAllCableVisuals();
        ClearAllTargetingVisuals(grid);
        targetedPositions.Clear();
    }

    private BombCableVisual[] GetCableVisuals()
    {
        EnemyBehaviour enemy = GameManager.Instance?.CombatHandler?.GetEnemyRef();
        return enemy != null ? enemy.GetComponentsInChildren<BombCableVisual>() : new BombCableVisual[0];
    }

    private class BombInstance
    {
        public readonly Vector2Int Position;

        // each bomb has its own independent countdown
        public int RemainingMoves;

        //Defused when all 4 cardinal offsets have been cleared by the player.
        public bool IsDefused => activeOffsets.Count <= 1;

        //>Set to true after the bomb has triggered its explosion
        public bool IsExploded { get; set; }

        public IReadOnlyList<Vector2Int> ActiveOffsets  => activeOffsets;
        public IReadOnlyList<Vector2Int> CardinalPositions => cardinalPositions;

        public BombCableVisual CableVisual { get; private set; }

        private readonly List<Vector2Int> activeOffsets  = new List<Vector2Int>();
        private readonly List<Vector2Int> clearedOffsets = new List<Vector2Int>();
        private readonly List<Vector2Int> cardinalPositions = new List<Vector2Int>();

        private readonly Dictionary<Vector2Int, Element>      trackedElements = new Dictionary<Vector2Int, Element>();
        private readonly Dictionary<Vector2Int, ElementVisual> trackedVisuals  = new Dictionary<Vector2Int, ElementVisual>();

        public BombInstance(Vector2Int position, int initialMoves)
        {
            Position = position;
            RemainingMoves = initialMoves;

            foreach (var o in new[]
            {
                new Vector2Int( 0,  1),
                new Vector2Int( 0, -1),
                new Vector2Int( 1,  0),
                new Vector2Int(-1,  0),
            })
                activeOffsets.Add(o);
        }

        public void TrackTarget(Vector2Int offset, Vector2Int worldPos, Element element, ElementVisual visual)
        {
            cardinalPositions.Add(worldPos);
            trackedElements[offset] = element;
            trackedVisuals[offset]  = visual;
        }

        public Element GetTrackedElement(Vector2Int offset)
        {
            trackedElements.TryGetValue(offset, out Element e);
            return e;
        }

        /// <summary>Removes an offset from active and adds it to cleared.</summary>
        public void MarkOffsetCleared(Vector2Int offset)
        {
            activeOffsets.Remove(offset);
            if (!clearedOffsets.Contains(offset))
                clearedOffsets.Add(offset);
            trackedElements.Remove(offset);
            trackedVisuals.Remove(offset);
        }

        /// <summary>Clears per-tick targeting data so RetargetAfterGravity can repopulate it.</summary>
        public void ClearTargetingData()
        {
            cardinalPositions.Clear();
            trackedElements.Clear();
            trackedVisuals.Clear();
        }

        // version that forwards the Grid reference correctly.</summary>
        public void SetupCable(Grid grid, BombCableVisual visual)
        {
            if (visual == null || grid == null) return;
            CableVisual = visual;

            if (activeOffsets.Count + clearedOffsets.Count < 4) return;

            List<Vector2Int> all = new List<Vector2Int>(activeOffsets);
            all.AddRange(clearedOffsets);
            CableVisual.Initialize(grid, Position, all);

            foreach (var cleared in clearedOffsets)
                CableVisual.RemoveCable(cleared);
        }

        public void ClearCable()
        {
            if (CableVisual == null) return;
            CableVisual.ClearAllLines();
            CableVisual = null;
        }
    }
}