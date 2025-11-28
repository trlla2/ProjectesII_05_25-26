using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public interface IEnemySpell
{
    void Execute(Grid grid, System.Action onComplete = null);
    float GetCastDuration();
    string GetSpellName();
}

public abstract class EnemySpellBase : IEnemySpell
{
    protected float castDuration;
    protected string spellName;

    public float GetCastDuration() => castDuration;
    public string GetSpellName() => spellName;

    public abstract void Execute(Grid grid, System.Action onComplete = null);
}

public class ColumnDestroySpell : EnemySpellBase
{
    private float damagePerElement;
    private int columnIndex;

    public ColumnDestroySpell(float damage = 4f, float duration = 1.5f, int column = -1)
    {
        damagePerElement = damage;
        castDuration = duration;
        columnIndex = column;
        spellName = "Column Destroy";
    }

    public override void Execute(Grid grid, System.Action onComplete = null)
    {
        if (grid == null)
        {
            onComplete?.Invoke();
            return;
        }

        // Select a random column if not specified
        int targetColumn = columnIndex >= 0 ? columnIndex : Random.Range(0, Grid.GridSize);

        // Get all positions in the column
        List<Vector2Int> positions = GetColumnPositions(targetColumn, grid);

        if (positions.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        // Start the blinking anim (grid will mark visuals as targeted)
        grid.StartDeleteElementsByEnemies(castDuration, positions);

        // Invoke completion callback after duration and apply damage only for elements still targeted
        if (onComplete != null)
        {
            grid.StartCoroutine(InvokeAfterDelay(castDuration, grid, positions, onComplete));
        }
        else
        {
            grid.StartCoroutine(InvokeAfterDelay(castDuration, grid, positions, null));
        }
    }

    private List<Vector2Int> GetColumnPositions(int column, Grid grid)
    {
        List<Vector2Int> positions = new List<Vector2Int>();

        for (int row = 0; row < Grid.GridSize; row++)
        {
            Vector2Int pos = new Vector2Int(column, row);

            // Only add if position is valid and has a non-empty element
            if (grid.IsPositionValid(pos))
            {
                Element element = grid.GetElementAt(pos);
                if (element != null && element != grid.EmptyElement)
                {
                    positions.Add(pos);
                }
            }
        }

        return positions;
    }

    private void DamagePlayer(float damage)
    {
        // Integrate with your player stats system
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.DealDamage(damage);
        }

    }

    private System.Collections.IEnumerator InvokeAfterDelay(float delay, Grid grid, List<Vector2Int> positions, System.Action callback)
    {
        yield return new UnityEngine.WaitForSeconds(delay);

        // Count how many positions are still targeted (visual exists and still targeted)
        int remaining = 0;
        foreach (var pos in positions)
        {
            ElementVisual visual = grid.GetElementVisualAt(pos);
            if (visual != null && visual.IsTargetedByEnemy)
            {
                remaining++;
            }
        }

        if (remaining > 0)
        {
            float totalDamage = damagePerElement * remaining;
            DamagePlayer(totalDamage);
        }

        callback?.Invoke();
    }
}

public class RandomDestroySpell : EnemySpellBase
{
    private float damagePerElement;
    private int elementCount;

    public RandomDestroySpell(int count = 3, float damage = 4f, float duration = 1.5f)
    {
        elementCount = count;
        damagePerElement = damage;
        castDuration = duration;
        spellName = "Random Destroy";
    }

    public override void Execute(Grid grid, System.Action onComplete = null)
    {
        if (grid == null)
        {
            onComplete?.Invoke();
            return;
        }

        // Get random valid positions
        List<Vector2Int> positions = GetRandomPositions(elementCount, grid);

        if (positions.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        // Start the blinking deletion animation (grid will mark visuals as targeted)
        grid.StartDeleteElementsByEnemies(castDuration, positions);

        // Invoke completion callback after duration and apply damage only for elements still targeted
        if (onComplete != null)
        {
            grid.StartCoroutine(InvokeAfterDelay(castDuration, grid, positions, onComplete));
        }
        else
        {
            grid.StartCoroutine(InvokeAfterDelay(castDuration, grid, positions, null));
        }
    }

    private List<Vector2Int> GetRandomPositions(int count, Grid grid)
    {
        List<Vector2Int> allPositions = new List<Vector2Int>();
        List<Vector2Int> selectedPositions = new List<Vector2Int>();

        for (int x = 0; x < Grid.GridSize; x++)
        {
            for (int y = 0; y < Grid.GridSize; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                Element element = grid.GetElementAt(pos);

                if (element != null && element != grid.EmptyElement)
                {
                    allPositions.Add(pos);
                }
            }
        }

        int targetCount = Mathf.Min(count, allPositions.Count);
        for (int i = 0; i < targetCount; i++)
        {
            if (allPositions.Count == 0)
                break;

            int randomIndex = Random.Range(0, allPositions.Count);
            selectedPositions.Add(allPositions[randomIndex]);
            allPositions.RemoveAt(randomIndex);
        }

        return selectedPositions;
    }

    private void DamagePlayer(float damage)
    {
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.DealDamage(damage);
        }

        Debug.Log($"RandomDestroySpell: Dealt {damage} damage to player");
    }

    private IEnumerator InvokeAfterDelay(float delay, Grid grid, List<Vector2Int> positions, System.Action callback)
    {
        yield return new UnityEngine.WaitForSeconds(delay);

        // Count how many positions are still targeted (visual exists and still targeted)
        int remaining = 0;
        foreach (var pos in positions)
        {
            ElementVisual visual = grid.GetElementVisualAt(pos);
            if (visual != null && visual.IsTargetedByEnemy)
            {
                remaining++;
            }
        }

        if (remaining > 0)
        {
            float totalDamage = damagePerElement * remaining;
            DamagePlayer(totalDamage);
        }

        callback?.Invoke();
    }
}