using System.Collections.Generic;
using UnityEngine;

public class BlockerSpell : IEnemySpell
{
    private int blockCount;
    private float damagePercent;
    private float attackTimer;
    private float damagePerElement;

    private List<Vector2Int> blockedPositions = new List<Vector2Int>();
    private Dictionary<Vector2Int, int> blockProgress = new Dictionary<Vector2Int, int>();//Track progress for each block position

    private SingleElementTimedSpell activeAttackSpell;
    private Grid cachedGrid;
    private EnemyBehaviour cachedEnemy;

    private bool isActive;
    private int requiredToBreak = 3;

    public int RemainingMoves => activeAttackSpell?.RemainingMoves ?? 0;
    public bool ShouldRerollAfterAttack => false; // We handle our own reroll logic
    public bool IsActive => isActive;
    public List<Vector2Int> BlockedPositions => blockedPositions;

    public BlockerSpell(int blockCount = 2, float damagePercent = 0.25f, float attackTimer = 6f, float damagePerElement = 4f)
    {
        this.blockCount = blockCount;
        this.damagePercent = damagePercent;
        this.attackTimer = attackTimer;
        this.damagePerElement = damagePerElement;
    }

    public string GetSpellName() => "Blocker";

    public void Execute(Grid grid)
    {
        if (grid == null) return;

        cachedGrid = grid;
        isActive = true;

        if (GameManager.Instance != null && GameManager.Instance.CombatHandler != null)
        {
            cachedEnemy = GameManager.Instance.CombatHandler.GetEnemyRef();
        }

        PlaceBlocks(grid, blockCount);

        StartAttackSpell(grid);
    }

    public void Tick(Grid grid, System.Action onComplete = null)
    {
        // The timed attack spell handles its own ticking

        activeAttackSpell?.Tick(grid, onComplete);
    }

    public void Cleanup(Grid grid)
    {
        isActive = false;

        foreach (var pos in blockedPositions)
        {
            if (!grid.IsPositionValid(pos)) continue;

            ElementVisual visual = grid.GetElementVisualAt(pos);
            if (visual != null && visual.IsBlocked)
            {
                visual.SetBlocked(false);
            }
        }

        blockedPositions.Clear();
        blockProgress.Clear();

        // Cleanup attack spell
        activeAttackSpell?.Cleanup(grid);
        activeAttackSpell = null;
    }

    public void RetargetAfterGravity(Grid grid)
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
            {
                invalidBlocks.Add(pos);
            }
        }

        foreach (var pos in invalidBlocks)
        {
            blockedPositions.Remove(pos);
            blockProgress.Remove(pos);
        }

        activeAttackSpell?.RetargetAfterGravity(grid);
    }

    public bool OnAdjacentElementsUsed(Grid grid, List<Vector2Int> usedPositions)
    {
        if (!isActive || grid == null) return false;

        bool anyBlockBroken = false;
        List<Vector2Int> brokenBlocks = new List<Vector2Int>();

        foreach (var blockPos in blockedPositions)
        {
            int adjacentCount = CountAdjacentPositions(blockPos, usedPositions);
            
            if (adjacentCount > 0)
            {
                //Accumulate progress
                if (!blockProgress.ContainsKey(blockPos))
                    blockProgress[blockPos] = 0;

                blockProgress[blockPos] += adjacentCount;

                //Update visual
                ElementVisual blockVisual = grid.GetElementVisualAt(blockPos);
                if (blockVisual != null)
                {
                    blockVisual.UpdateBlockProgress(blockProgress[blockPos], requiredToBreak);
                }

                //Check if block is broken
                if (blockProgress[blockPos] >= requiredToBreak)
                {
                    brokenBlocks.Add(blockPos);
                    anyBlockBroken = true;
                }
            }
        }

        foreach (var brokenPos in brokenBlocks)
        {
            BreakBlock(grid, brokenPos);
        }

        return anyBlockBroken;
    }

    private void BreakBlock(Grid grid, Vector2Int position)
    {
        ElementVisual visual = grid.GetElementVisualAt(position);
        if (visual != null)
        {
            visual.SetBlocked(false);
        }

        blockedPositions.Remove(position);
        blockProgress.Remove(position);

        if (cachedEnemy != null)
        {
            cachedEnemy.dealBossDamage(damagePercent);
            Debug.Log($"Block broken! Dealt {damagePercent * 100}% damage to enemy.");
        }

        OnBlockBroken(grid);
    }

    private void OnBlockBroken(Grid grid)
    {
        if (activeAttackSpell != null)
        {
            activeAttackSpell.Cleanup(grid);
            activeAttackSpell = null;
        }

        foreach (var pos in blockedPositions)
        {
            if (blockProgress.ContainsKey(pos))
                blockProgress[pos] = 0;

            ElementVisual visual = grid.GetElementVisualAt(pos);
            if (visual != null)
                visual.ResetBlockProgress();
        }

        grid.Reroll(3);

        //We need to wait for reroll to complete, then place new blocks
        //this will be handled by the EnemyBehaviour's RerollAndRetarget
    }

    public void OnRerollComplete(Grid grid)
    {
        PlaceBlocks(grid, 2);

        StartAttackSpell(grid);
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
        blockProgress[position] = 0;

        ElementVisual visual = grid.GetElementVisualAt(position);
        if (visual != null)
        {
            visual.SetBlocked(true);
        }
    }

    private List<Vector2Int> GetValidBlockPositions(Grid grid)
    {
        List<Vector2Int> valid = new List<Vector2Int>();

        for (int x = 0; x < Grid.GridSize; x++)
        {
            for (int y = 0; y < Grid.GridSize; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);

                if (!grid.IsPositionValid(pos))
                    continue;

                // Don't place on existing blocks
                if (blockedPositions.Contains(pos))
                    continue;

                Element element = grid.GetElementAt(pos);
                if (element == null || element == grid.EmptyElement)
                    continue;

                //Don't place on bomb elements
                if (element.Type == ElementType.Bomb)
                    continue;

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


    private int CountAdjacentPositions(Vector2Int blockPos, List<Vector2Int> positions)
    {
        int count = 0;
        Vector2Int[] directions = new Vector2Int[]
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        foreach (var pos in positions)
        {
            foreach (var dir in directions)
            {
                if (blockPos + dir == pos)
                {
                    count++;
                    break;
                }
            }
        }

        return count;
    }

    public bool IsAdjacentToBlock(Vector2Int position)
    {
        Vector2Int[] directions = new Vector2Int[]
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        foreach (var blockPos in blockedPositions)
        {
            foreach (var dir in directions)
            {
                if (blockPos + dir == position)
                    return true;
            }
        }

        return false;
    }

    public HashSet<Vector2Int> GetBlockedPositionsSet()
    {
        return new HashSet<Vector2Int>(blockedPositions);
    }
}