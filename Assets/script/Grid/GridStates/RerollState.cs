using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RerollState : IGridState
{
    private Grid grid;
    private VisualGrid visualGrid;
    private List<Element> availableElements;
    private List<Spell> availableSpells;
    private bool isAnimating;

    private float scrollDuration = 1.5f;
    private float columnDelay = 0.25f;
    private int scrollTimes = 5;

    public RerollState(Grid gridController, VisualGrid visual,
                       List<Element> elements, List<Spell> spells, int times = 3)
    {
        grid = gridController;
        visualGrid = visual;
        availableElements = elements;
        availableSpells = spells;
        scrollTimes = Mathf.Max(1, times);
    }

    public void Enter()
    {
        visualGrid.RemoveAllHighlights();
        isAnimating = true;
        grid.StartCoroutine(PerformReroll());
    }

    public void Update()
    {
        if (!isAnimating)
        {
            grid.ChangeState(new ValidateSpellState(grid, GameManager.Instance.PlayerStats.GetSpellBook()));
        }
    }

    public void Exit()
    {
        isAnimating = false;
    }

    public GridStateType GetStateType()
    {
        return GridStateType.Reroll;
    }

    private IEnumerator PerformReroll()
    {
        grid.NotifyRerollStarted();

        Element[,] finalGrid = GenerateFinalGrid();

        if (availableSpells != null && availableSpells.Count > 0)
        {
            Spell randomSpell = availableSpells[Random.Range(0, availableSpells.Count)];
            TryInsertSpellIntoGrid(finalGrid, randomSpell.Recipe);
        }

        yield return grid.StartCoroutine(
            visualGrid.PlayRerollAnimation(scrollTimes, scrollDuration, columnDelay, availableElements, finalGrid)
        );

        ApplyFinalGrid(finalGrid);
        FinalizeVisuals();

        grid.NotifyRerollCompleted();

        isAnimating = false;
    }

    private Element[,] GenerateFinalGrid()
    {
        int size = grid.GridData.GridSize;
        Element[,] result = new Element[size, size];
        Element bombEl = grid.BombElement;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                Element current = grid.GridData.GetElementAt(pos);

                // Preserve the bomb element so the active BombSpell stays valid
                if (bombEl != null && current == bombEl)
                    result[x, y] = bombEl;
                else
                    result[x, y] = grid.PickWeightedRandom(availableElements);
            }
        }

        return result;
    }

    // Cant use the spellinsert interface since we need to reroll the whole grid and working with a temp grid
    // rather than the actual griddata, so we need to reimplement it here
    private void TryInsertSpellIntoGrid(Element[,] targetGrid, List<Element> recipe)
    {
        if (recipe == null || recipe.Count == 0)
            return;

        int size = targetGrid.GetLength(0);

        // Collect bomb positions so spell paths never overwrite them
        HashSet<Vector2Int> bombPositions = new HashSet<Vector2Int>();
        Element bombEl = grid.BombElement;
        if (bombEl != null)
        {
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    if (targetGrid[x, y] == bombEl)
                        bombPositions.Add(new Vector2Int(x, y));
        }

        int maxAttempts = 50;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector2Int startPos = new Vector2Int(
                Random.Range(0, size),
                Random.Range(0, size)
            );

            if (bombPositions.Contains(startPos))
                continue;

            List<Vector2Int> path = GenerateRandomPathFromStart(size, recipe.Count, startPos, bombPositions);

            if (path != null && path.Count == recipe.Count)
            {
                for (int i = 0; i < recipe.Count; i++)
                {
                    Vector2Int pos = path[i];
                    targetGrid[pos.x, pos.y] = recipe[i];
                }
                return;
            }
        }
    }

    private List<Vector2Int> GenerateRandomPathFromStart(int gridSize, int length, Vector2Int start, HashSet<Vector2Int> excluded = null)
    {
        if (length > gridSize * gridSize)
            return null;

        List<Vector2Int> path = new List<Vector2Int> { start };
        HashSet<Vector2Int> visited = new HashSet<Vector2Int> { start };

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (path.Count < length)
        {
            Vector2Int current = path[path.Count - 1];
            List<Vector2Int> neighbors = new List<Vector2Int>();

            foreach (var dir in directions)
            {
                Vector2Int next = current + dir;
                if (next.x >= 0 && next.x < gridSize &&
                    next.y >= 0 && next.y < gridSize &&
                    !visited.Contains(next) &&
                    (excluded == null || !excluded.Contains(next)))
                {
                    neighbors.Add(next);
                }
            }

            if (neighbors.Count == 0)
                return null;

            Vector2Int chosen = neighbors[Random.Range(0, neighbors.Count)];
            path.Add(chosen);
            visited.Add(chosen);
        }

        return path;
    }

    private void ApplyFinalGrid(Element[,] finalGrid)
    {
        int size = grid.GridData.GridSize;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                grid.GridData.SetElementAt(new Vector2Int(x, y), finalGrid[x, y]);
            }
        }
    }

    private void FinalizeVisuals()
    {
        int size = grid.GridData.GridSize;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                ElementVisual visual = grid.GridData.GetVisualAt(pos);
                Element element = grid.GridData.GetElementAt(pos);

                if (visual != null)
                {
                    visual.transform.position = visualGrid.GridToWorldPosition(pos);
                    visual.Initialize(pos, element);
                    visual.ResetSpriteTransform();
                    visual.SetWorldPosition(visualGrid.GridToWorldPosition(pos));
                    visual.SetSelectable(true);
                }
                else if (element != null)
                {
                    visualGrid.EnsureVisualAt(pos, element);
                }
            }
        }
    }
}
