using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public enum GridStateType
{
    Initialize,
    Idle,
    Refill,
    Reroll,
    Processing,
    ValidateSpell
}

public interface IGridState
{
    void Enter();
    void Update();
    void Exit();
    GridStateType GetStateType();
}

public class IdleState : IGridState
{
    private Grid grid;

    public IdleState(Grid gridController)
    {
        grid = gridController;
    }

    public void Enter()
    {
    }

    public void Update()
    {
    }

    public void Exit()
    {
    }

    public GridStateType GetStateType()
    {
        return GridStateType.Idle;
    }
}

public class RefillState : IGridState
{
    private Grid grid;
    private VisualGrid visualGrid;
    private bool isRefilling;
    private float refillDuration = 0.15f;

    public RefillState(Grid gridController, VisualGrid visual)
    {
        grid = gridController;
        visualGrid = visual;
    }

    public void Enter()
    {
        isRefilling = true;
        PerformRefill();
    }

    public void Update()
    {
        if (!isRefilling)
        {
            grid.ChangeState(new IdleState(grid));
        }
    }

    public void Exit()
    {
        isRefilling = false;
    }

    public GridStateType GetStateType()
    {
        return GridStateType.Refill;
    }

    private void PerformRefill()
    {
        List<Vector2Int> movedPositions = grid.GravityHandler.ApplyGravity();
        grid.StartCoroutine(RefillSequence(movedPositions, refillDuration));
    }

    private IEnumerator RefillSequence(List<Vector2Int> movedPositions, float gravityDuration)
    {
        if (visualGrid != null && movedPositions != null && movedPositions.Count > 0)
        {
            yield return grid.StartCoroutine(visualGrid.AnimateGravity(movedPositions, gravityDuration));
        }

        List<Vector2Int> emptyPositions = GetEmptyPositions();

        if (emptyPositions.Count > 0)
        {
            // Fill only the allowed empty slots with new elements
            grid.FillEmptySlots(grid.AvailableElements, emptyPositions);

            // Ensure visuals exist for newly spawned elements
            foreach (var pos in emptyPositions)
            {
                Element element = grid.GridData.GetElementAt(pos);
                if (element != null && element != grid.EmptyElement)
                {
                    visualGrid.EnsureVisualAt(pos, element);
                }
            }

            // Disable selection during animation
            foreach (var pos in emptyPositions)
            {
                ElementVisual visual = grid.GridData.GetVisualAt(pos);
                if (visual != null)
                    visual.SetSelectable(false);
            }

            // Play the fill animation only for the chosen positions
            visualGrid.FillEmpty(emptyPositions, refillDuration);

            yield return new WaitForSeconds(refillDuration);
            yield return grid.StartCoroutine(CompleteRefillAfterDelay(0f));
        }
        else
        {
            isRefilling = false;
        }
    }

    private List<Vector2Int> GetEmptyPositions()
    {
        List<Vector2Int> emptyPositions = new List<Vector2Int>();
        int size = grid.GridData.GridSize;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                Element element = grid.GridData.GetElementAt(pos);

                if (element == grid.EmptyElement || element == null)
                {
                    emptyPositions.Add(pos);
                }
            }
        }

        return emptyPositions;
    }

    private IEnumerator CompleteRefillAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        for (int x = 0; x < grid.GridData.GridSize; x++)
        {
            for (int y = 0; y < grid.GridData.GridSize; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                ElementVisual visual = grid.GridData.GetVisualAt(pos);

                if (visual != null)
                {
                    visual.transform.position = visualGrid.GridToWorldPosition(pos);
                    visual.ResetSpriteTransform();
                    visual.SetWorldPosition(visualGrid.GridToWorldPosition(pos));

                    Element element = grid.GridData.GetElementAt(pos);
                    visual.Initialize(pos, element);
                    visual.SetSelectable(true);
                }
            }
        }
        isRefilling = false;

        ValidateSpellState validateState = new ValidateSpellState(grid, G.GM.PlayerStats.GetSpellBook());
        grid.ChangeState(validateState);
    }
}

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
        isAnimating = true;
        grid.StartCoroutine(PerformReroll());
    }

    public void Update()
    {
        if (!isAnimating)
        {
            grid.ChangeState(new IdleState(grid));
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
        int gridSize = grid.GridData.GridSize;

        Element[,] finalGrid = GenerateFinalGrid();

        if (availableSpells != null && availableSpells.Count > 0)
        {
            Spell randomSpell = availableSpells[Random.Range(0, availableSpells.Count)];
            TryInsertSpellIntoGrid(finalGrid, randomSpell.Recipe);
        }

        yield return grid.StartCoroutine(visualGrid.PlayRerollAnimation(scrollTimes, scrollDuration, columnDelay, availableElements, finalGrid));

        ApplyFinalGrid(finalGrid);

        FinalizeVisuals();

        isAnimating = false;
    }

    private Element[,] GenerateFinalGrid()
    {
        int size = grid.GridData.GridSize;
        Element[,] result = new Element[size, size];

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                result[x, y] = availableElements[Random.Range(0, availableElements.Count)];
            }
        }

        return result;
    }

    //Cant use the spellinsert interface since we need to reroll the whole grid and working with a temp grid
    //rather than the actual griddata, so we need to reimplement it here
    private void TryInsertSpellIntoGrid(Element[,] targetGrid, List<Element> recipe)
    {
        if (recipe == null || recipe.Count == 0)
            return;

        int size = targetGrid.GetLength(0);
        
        // Try multiple attempts to find a valid path
        int maxAttempts = 50;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector2Int startPos = new Vector2Int(
                Random.Range(0, size),
                Random.Range(0, size)
            );

            List<Vector2Int> path = GenerateRandomPathFromStart(size, recipe.Count, startPos);
            
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

    private List<Vector2Int> GenerateRandomPathFromStart(int gridSize, int length, Vector2Int start)
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
                if (next.x >= 0 && next.x < gridSize && next.y >= 0 && next.y < gridSize && !visited.Contains(next))
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
            }
        }
    }
}

public class EnemySpellState : IGridState
{
    private Grid grid;
    private VisualGrid visualGrid;
    private List<Vector2Int> positions;
    private System.Action onComplete;
    private bool isProcessing;
    private float refillDuration = 0.15f;

    private bool excludeBelowTargets = true;
    private bool rerollAfterProcess = false;

    public EnemySpellState(Grid gridController, VisualGrid visual, List<Vector2Int> positionsToProcess, 
                           System.Action onCompleteCallback, bool excludeBelowTargets = true, bool rerollAfter = false)
    {
        grid = gridController;
        visualGrid = visual;
        positions = positionsToProcess ?? new List<Vector2Int>();
        onComplete = onCompleteCallback;
        this.excludeBelowTargets = excludeBelowTargets;
        this.rerollAfterProcess = rerollAfter;
    }

    public void Enter()
    {
        isProcessing = true;
        grid.StartCoroutine(ProcessSequence());
    }

    public void Update()
    {
        if (!isProcessing)
        {
            if (rerollAfterProcess)
            {
                grid.ChangeState(new RerollState(grid, visualGrid, grid.AvailableElements,
                    G.GM.PlayerStats.GetSpellBook(), 3));
            }
            else
            {
                grid.ChangeState(new IdleState(grid));
            }
        }
    }

    public void Exit()
    {
        isProcessing = false;
    }

    public GridStateType GetStateType()
    {
        return GridStateType.Processing;
    }

    private IEnumerator ProcessSequence()
    {
        if (positions == null || positions.Count == 0)
        {
            isProcessing = false;
            onComplete?.Invoke();
            yield break;
        }

        grid.InstantVisualEmpty(positions);

        List<Vector2Int> movedPositions = grid.GravityHandler.ApplyGravity();

        if (visualGrid != null && movedPositions != null && movedPositions.Count > 0)
        {
            yield return grid.StartCoroutine(visualGrid.AnimateGravity(movedPositions, refillDuration));
        }

        List<Vector2Int> emptyPositions;

        if (excludeBelowTargets)
            emptyPositions = GetEmptyPositionsExcludingBelowTargets();
        else
            emptyPositions = GetEmptyPositions();

        if (emptyPositions.Count > 0)
        {
            grid.FillEmptySlots(grid.AvailableElements, emptyPositions);

            foreach (var pos in emptyPositions)
            {
                Element element = grid.GridData.GetElementAt(pos);
                if (element != null && element != grid.EmptyElement)
                {
                    visualGrid.EnsureVisualAt(pos, element);
                }
            }

            foreach (var pos in emptyPositions)
            {
                ElementVisual visual = grid.GridData.GetVisualAt(pos);
                if (visual != null)
                    visual.SetSelectable(false);
            }

            visualGrid.FillEmpty(emptyPositions, refillDuration);

            // Wait for fill animation to complete then finalize
            yield return new WaitForSeconds(refillDuration);
            yield return grid.StartCoroutine(CompleteRefillAfterDelay(0f));
        }
        else
        {
            // no refill needed, finish processing now
            isProcessing = false;
            onComplete?.Invoke();
        }
    }

    private List<Vector2Int> GetEmptyPositions()
    {
        List<Vector2Int> emptyPositions = new List<Vector2Int>();
        int size = grid.GridData.GridSize;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                Element element = grid.GridData.GetElementAt(pos);

                if (element == grid.EmptyElement || element == null)
                {
                    emptyPositions.Add(pos);
                }
            }
        }

        return emptyPositions;
    }

    private List<Vector2Int> GetEmptyPositionsExcludingBelowTargets()
    {
        List<Vector2Int> emptyPositions = new List<Vector2Int>();
        int size = grid.GridData.GridSize;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                Element element = grid.GridData.GetElementAt(pos);

                if (element == grid.EmptyElement || element == null)
                {
                    bool isBelowTargeted = false;
                    for (int yAbove = y + 1; yAbove < size; yAbove++)
                    {
                        Vector2Int abovePos = new Vector2Int(x, yAbove);
                        ElementVisual aboveVisual = grid.GridData.GetVisualAt(abovePos);
                        Element aboveElement = grid.GridData.GetElementAt(abovePos);

                        if (aboveVisual != null && aboveVisual.IsTargetedByEnemy
                            && aboveElement != null && aboveElement != grid.EmptyElement)
                        {
                            isBelowTargeted = true;
                            break;
                        }
                    }

                    if (!isBelowTargeted)
                        emptyPositions.Add(pos);
                }
            }
        }

        return emptyPositions;
    }

    private IEnumerator CompleteRefillAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        for (int x = 0; x < grid.GridData.GridSize; x++)
        {
            for (int y = 0; y < grid.GridData.GridSize; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                ElementVisual visual = grid.GridData.GetVisualAt(pos);

                if (visual != null)
                {
                    visual.transform.position = visualGrid.GridToWorldPosition(pos);
                    visual.ResetSpriteTransform();
                    visual.SetWorldPosition(visualGrid.GridToWorldPosition(pos));

                    Element element = grid.GridData.GetElementAt(pos);
                    visual.Initialize(pos, element);
                    visual.SetSelectable(true);
                }
            }
        }

        isProcessing = false;
        onComplete?.Invoke();
    }
}

//check if there are available spells to the player
public class ValidateSpellState : IGridState
{
    private Grid grid;
    private ISpellValidator spellValidator;
    private List<Spell> availableSpells;
    private bool isValidating;

    public ValidateSpellState(Grid gridController, List<Spell> spells)
    {
        grid = gridController;
        spellValidator = new SpellValidator();
        availableSpells = spells;
    }

    public void Enter()
    {
        isValidating = true;
        ValidateGrid();
    }

    public void Update()
    {
        if (!isValidating)
        {
            grid.ChangeState(new IdleState(grid));
        }
    }

    public void Exit()
    {
        isValidating = false;
    }

    public GridStateType GetStateType()
    {
        return GridStateType.ValidateSpell;
    }

    private void ValidateGrid()
    {
        bool hasValidSpell = spellValidator.HasValidSpell(grid.GridData, availableSpells);

        if (!hasValidSpell)
        {
            grid.Reroll();
        }

        isValidating = false;
    }
}