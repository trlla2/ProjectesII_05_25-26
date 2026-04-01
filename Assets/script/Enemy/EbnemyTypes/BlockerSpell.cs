using System.Collections.Generic;
using UnityEngine;

//weird enemy so it cant directly inherit from spell base, it contains a separate timed spell inside it tracked separately tho
public class BlockerSpell : IEnemySpell
{
    private int blockCount;
    private float damagePercent;
    private float attackTimer;
    private float damagePerElement;

    private List<Vector2Int> blockedPositions = new List<Vector2Int>();
    private Dictionary<Vector2Int, float> blockProgress = new Dictionary<Vector2Int, float>();

    private SingleElementTimedSpell activeAttackSpell;
    private EnemyBehaviour cachedEnemy;
    private Grid cachedGrid;

    private bool isActive;
    private const int requiredAdjacentCount = 4;
    private const float requiredToBreak = 1.0f;
    private const float progressPerAdjacent = 1f / requiredAdjacentCount;

    public int RemainingMoves => activeAttackSpell?.RemainingMoves == null ? 0 : activeAttackSpell.RemainingMoves;
    public bool ShouldRerollAfterAttack => false;
    public bool IsActive => isActive;
    public bool IsAttackTimerExpired => activeAttackSpell?.IsTimerExpired ?? false;
    public bool HasActiveAttackSpell => activeAttackSpell != null;

    public BlockerSpell(int blockCount = 2, float damagePercent = 0.25f, float attackTimer = 6f, float damagePerElement = 4f)
    {
        this.blockCount = blockCount;
        this.damagePercent = damagePercent;
        this.attackTimer = attackTimer;
        this.damagePerElement = damagePerElement;
    }

    public override string GetSpellName() => "Blocker";

    public void Execute(Grid grid)
    {
        if (grid == null) return;

        isActive = true;
        cachedGrid = grid;

        if (GameManager.Instance != null && GameManager.Instance.CombatHandler != null)
            cachedEnemy = GameManager.Instance.CombatHandler.GetEnemyRef();

        grid.OnRerollCompleted += HandleRerollCompleted;

        PlaceBlocks(grid, blockCount);
        StartAttackSpell(grid);
    }

    public override void Tick(Grid grid, System.Action onComplete = null)
    {
        // The timed attack spell handles its own ticking
        activeAttackSpell?.Tick(grid, onComplete);
    }

    public override void Cleanup(Grid grid)
    {
        isActive = false;

        if (cachedGrid != null)
        {
            cachedGrid.OnRerollCompleted -= HandleRerollCompleted;
            cachedGrid = null;
        }

        foreach (var pos in blockedPositions)
        {
            if (!grid.IsPositionValid(pos)) continue;

            ElementVisual visual = grid.GetElementVisualAt(pos);
            if (visual != null && visual.IsBlocked)
                visual.SetBlocked(false);
        }

        blockedPositions.Clear();
        blockProgress.Clear();

        // Cleanup attack spell
        activeAttackSpell?.Cleanup(grid);
        activeAttackSpell = null;
    }

    public override void RetargetAfterGravity(Grid grid)
    {
        if (grid == null) return;

        // Blocks don't move with gravity, but we need to verify they're still valid
        List<Vector2Int> invalidBlocks = new List<Vector2Int>();

        foreach (var pos in blockedPositions)
        {
            if (!grid.IsPositionValid(pos))
            {
                invalidBlocks.Add(pos);
                continue;
            }

            ElementVisual visual = grid.GetElementVisualAt(pos);
            if (visual == null || !visual.IsBlocked)
                invalidBlocks.Add(pos);
        }

        foreach (var pos in invalidBlocks)
        {
            blockedPositions.Remove(pos);
            blockProgress.Remove(pos);
        }

        activeAttackSpell?.RetargetAfterGravity(grid);
        
        // Check if the player cleared the targeted element during their move
        CheckAndRetargetIfCleared(grid);
    }

    public bool OnAdjacentElementsUsed(Grid grid, List<Vector2Int> usedPositions)
    {
        if (!isActive || grid == null) return false;

        // Check if player used the targeted element
        CheckIfTargetUsedByPlayer(grid, usedPositions);

        List<Vector2Int> brokenBlocks = new List<Vector2Int>();
        HashSet<Vector2Int> usedSet = new HashSet<Vector2Int>(usedPositions);

        foreach (var blockPos in blockedPositions)
        {
            int adjacentCount = CountAdjacentPositions(blockPos, usedSet);
            if (adjacentCount <= 0)
                continue;

            if (!blockProgress.ContainsKey(blockPos))
                blockProgress[blockPos] = 0f;

            blockProgress[blockPos] += adjacentCount * progressPerAdjacent;
            blockProgress[blockPos] = Mathf.Clamp01(blockProgress[blockPos]);

            ElementVisual blockVisual = grid.GetElementVisualAt(blockPos);
            if (blockVisual != null)
                blockVisual.UpdateBlockProgress(blockProgress[blockPos] / requiredToBreak);

            if (blockProgress[blockPos] >= requiredToBreak)
                brokenBlocks.Add(blockPos);
        }

        if (brokenBlocks.Count > 0)
        {
            // Animation happens but we don't complete the break yet
            // The callback from AnimateBlockadeBreaking will handle CompleteBreakBlock
            AnimateBlockadeBreaking(grid, brokenBlocks, () =>
            {
                // After animation completes, finish breaking the blocks
                foreach (var brokenPos in brokenBlocks)
                    CompleteBreakBlock(grid, brokenPos);
            });
        }

        // Return true immediately if all blocks will be broken
        return blockedPositions.Count - brokenBlocks.Count == 0;
    }

    // Check if player used any of the targeted positions
    private void CheckIfTargetUsedByPlayer(Grid grid, List<Vector2Int> usedPositions)
    {
        if (activeAttackSpell == null || activeAttackSpell.IsEffectApplied)
            return;

        // Check if any of the used positions match the targeted positions
        foreach (var usedPos in usedPositions)
        {
            if (activeAttackSpell.TargetedPositions.Contains(usedPos))
            {
                // Player used the targeted element - immediately retarget
                RestartAttackSpell(grid);
                return;
            }
        }
    }

    // Helper method to check and retarget after gravity
    private void CheckAndRetargetIfCleared(Grid grid)
    {
        if (activeAttackSpell == null || activeAttackSpell.IsEffectApplied)
            return;

        var positions = new List<Vector2Int>(activeAttackSpell.TargetedPositions);

        foreach (var pos in positions)
        {
            ElementVisual visual = grid.GetElementVisualAt(pos);
            if (visual == null || !visual.IsTargetedByEnemy)
            {
                bool allCleared = activeAttackSpell.OnTargetCleared(grid, pos);
                if (allCleared)
                {
                    RestartAttackSpell(grid);
                    return;
                }
            }
        }

        activeAttackSpell.ReapplyTargetingVisuals(grid);
    }

    // Called by EnemyBehaviour when all blocks are broken to trigger the reroll cycle.
    // Now accepts a completion callback to signal when reroll should start
    public void OnAllBlocksBroken(Grid grid, System.Action onAnimationComplete)
    {
        if (activeAttackSpell != null)
        {
            activeAttackSpell.Cleanup(grid);
            activeAttackSpell = null;
        }

        // Invoke callback immediately - the animation has already completed via AnimateBlockadeBreaking
        onAnimationComplete?.Invoke();
    }

    // Covers both ValidateSpellState rerolls and OnAllBlocksBroken rerolls.
    private void HandleRerollCompleted()
    {
        if (cachedGrid == null || !isActive) return;

        foreach (var pos in blockedPositions)
        {
            if (!cachedGrid.IsPositionValid(pos)) continue;

            ElementVisual visual = cachedGrid.GetElementVisualAt(pos);
            if (visual == null) continue;

            if (!visual.IsBlocked)
                visual.SetBlocked(true);

            float progress = blockProgress.ContainsKey(pos) ? blockProgress[pos] : 0f;
            visual.UpdateBlockProgress((progress / requiredToBreak));
        }
    }

    private void AnimateBlockadeBreaking(Grid grid, List<Vector2Int> brokenPositions, System.Action onComplete)
    {
        if (brokenPositions == null || brokenPositions.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        //Get the enemy SpellPathAnimator to reuse the same animation system
        if (cachedEnemy == null)
        {
            Debug.LogWarning("No cached enemy reference for animation");
            onComplete?.Invoke();
            return;
        }

        SpellPathAnimator animator = cachedEnemy.GetComponentInChildren<SpellPathAnimator>();
        if (animator == null)
        {
            Debug.LogWarning("No SpellPathAnimator found on enemy");
            onComplete?.Invoke();
            return;
        }

        // Gather elements from broken block positions
        List<Element> elements = new List<Element>();
        foreach (var pos in brokenPositions)
        {
            Element element = grid.GetElementAt(pos);
            if (element != null && element != grid.EmptyElement)
                elements.Add(element);
        }

        if (elements.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        // Convert List to array for animator
        Vector2Int[] pathArray = brokenPositions.ToArray();

        // Animate the blockade visuals flying to the enemy
        animator.OnSpellCast(pathArray, elements, onComplete);
    }

    private void CompleteBreakBlock(Grid grid, Vector2Int position)
    {
        ElementVisual visual = grid.GetElementVisualAt(position);
        if (visual != null)
            visual.SetBlocked(false);

        blockedPositions.Remove(position);
        blockProgress.Remove(position);

        if (cachedEnemy != null)
        {
            cachedEnemy.dealBossDamage(damagePercent);
        }
    }

    public void OnRerollComplete(Grid grid)
    {
        // Clear all surviving blocks before placing a fresh set
        foreach (var pos in blockedPositions)
        {
            if (!grid.IsPositionValid(pos)) continue;
            ElementVisual visual = grid.GetElementVisualAt(pos);
            if (visual != null && visual.IsBlocked)
                visual.SetBlocked(false);
        }
        blockedPositions.Clear();
        blockProgress.Clear();

        PlaceBlocks(grid, blockCount);
        StartAttackSpell(grid);
    }
    public void ProcessExpiredAttack(Grid grid, System.Action onComplete)
    {
        activeAttackSpell?.Tick(grid, onComplete);
    }


    public void RestartAttackSpell(Grid grid)
    {
        activeAttackSpell?.Cleanup(grid);
        activeAttackSpell = null;
        StartAttackSpell(grid);
    }

    public void CheckForClearedAttackTarget(Grid grid)
    {
        if (activeAttackSpell == null || activeAttackSpell.IsEffectApplied) return;

        var positions = new List<Vector2Int>(activeAttackSpell.TargetedPositions);

        foreach (var pos in positions)
        {
            ElementVisual visual = grid.GetElementVisualAt(pos);
            if (visual == null || !visual.IsTargetedByEnemy)
            {
                bool allCleared = activeAttackSpell.OnTargetCleared(grid, pos);
                if (allCleared)
                {
                    RestartAttackSpell(grid);
                    return;
                }
            }
        }

        activeAttackSpell.ReapplyTargetingVisuals(grid);
    }

    public void PauseAttackTimer()
    {
        activeAttackSpell?.PauseTimer();
    }

    public void ResumeAttackTimer()
    {
        activeAttackSpell?.ResumeTimer();
    }

    private void PlaceBlocks(Grid grid, int count)
    {
        List<Vector2Int> candidates = GetValidBlockPositions(grid);

        for (int i = 0; i < count && candidates.Count > 0; i++)
        {
            int index = Random.Range(0, candidates.Count);
            Vector2Int pos = candidates[index];
            candidates.RemoveAt(index);

            PlaceBlockAt(grid, pos);
        }
    }

    private void PlaceBlockAt(Grid grid, Vector2Int position)
    {
        blockedPositions.Add(position);
        blockProgress[position] = 0f;

        ElementVisual visual = grid.GetElementVisualAt(position);
        if (visual != null)
            visual.SetBlocked(true);
    }

    private List<Vector2Int> GetValidBlockPositions(Grid grid)
    {
        List<Vector2Int> valid = new List<Vector2Int>();

        for (int x = 0; x < Grid.GridSize; x++)
        {
            for (int y = 0; y < Grid.GridSize; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);

                if (!grid.IsPositionValid(pos)) continue;
                if (blockedPositions.Contains(pos)) continue;

                Element element = grid.GetElementAt(pos);
                if (element == null || element == grid.EmptyElement) continue;
                if (element.Type == ElementType.Bomb) continue;

                valid.Add(pos);
            }
        }

        return valid;
    }

    private void StartAttackSpell(Grid grid)
    {
        activeAttackSpell = new SingleElementTimedSpell(attackTimer, damagePerElement);
        activeAttackSpell.Execute(grid);
    }

    private int CountAdjacentPositions(Vector2Int blockPos, HashSet<Vector2Int> positions)
    {
        int count = 0;
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (var dir in directions)
            if (positions.Contains(blockPos + dir))
                count++;

        return count;
    }

    public HashSet<Vector2Int> GetBlockedPositionsSet()
    {
        return new HashSet<Vector2Int>(blockedPositions);
    }
}