using System.Collections.Generic;
using System.Collections;
using UnityEngine;
public enum GridStateType
{
    Initialize,
    Idle,
    Refill,
    LeverPull,
    Processing
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
        // Animate gravity and wait until it finishes
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

        //After animation completes, ensure ALL visuals are properly 
        for (int x = 0; x < grid.GridData.GridSize; x++)
        {
            for (int y = 0; y < grid.GridData.GridSize; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                ElementVisual visual = grid.GridData.GetVisualAt(pos);

                if (visual != null)
                {
                    //Reset anchor
                    visual.transform.position = visualGrid.GridToWorldPosition(pos);

                    //Reset sprite transform
                    visual.ResetSpriteTransform();
                    visual.SetWorldPosition(visualGrid.GridToWorldPosition(pos));

                    //Re-initialize to ensure grid position
                    Element element = grid.GridData.GetElementAt(pos);
                    visual.Initialize(pos, element);

                    //Re-enable selection
                    visual.SetSelectable(true);
                }
            }
        }

        isRefilling = false;
    }
}

public class LeverPullState : IGridState
{
    private  Grid grid;
    private  VisualGrid visualGrid;
    private  List<Element> availableElements;
    private  List<Spell> availableSpells;
    private bool isAnimating;
    private float scrollDuration = 1.5f;
    private float columnDelay = 0.1f;

    public LeverPullState(Grid gridController, VisualGrid visual,
                          List<Element> elements, List<Spell> spells)
    {
        grid = gridController;
        visualGrid = visual;
        availableElements = elements;
        availableSpells = spells;
    }

    public void Enter()
    {
        isAnimating = true;
        PerformLeverPull();
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
        return GridStateType.LeverPull;
    }

    private void PerformLeverPull()
    {
        grid.RandomizeBoard(availableElements);

        //Try to insert a random spell
        if (availableSpells != null && availableSpells.Count > 0)
        {
            Spell randomSpell = availableSpells[Random.Range(0, availableSpells.Count)];
            grid.InsertSpell(randomSpell.Recipe);
        }

        visualGrid.PlayScrollAnimation(scrollDuration, columnDelay);

        float totalDuration = scrollDuration + (columnDelay * grid.GridData.GridSize);
        grid.StartCoroutine(CompleteAfterDelay(totalDuration));
    }

    private IEnumerator CompleteAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isAnimating = false;
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

    public EnemySpellState(Grid gridController, VisualGrid visual, List<Vector2Int> positionsToProcess, System.Action onCompleteCallback, bool excludeBelowTargets = true)
    {
        grid = gridController;
        visualGrid = visual;
        positions = positionsToProcess ?? new List<Vector2Int>();
        onComplete = onCompleteCallback;
        this.excludeBelowTargets = excludeBelowTargets;
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
            grid.ChangeState(new IdleState(grid));
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

        // Apply gravity
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

            //Ensure visuals exist for newly spawned elements
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

                        // Consider the above tile as a blocker only if:
                        // here's a visual
                        //that visual is currently targeted by the enemy
                        //the logical element at that position is non-empty
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

        //After animation completes, ensure ALL visuals are properly 
        for (int x = 0; x < grid.GridData.GridSize; x++)
        {
            for (int y = 0; y < grid.GridData.GridSize; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                ElementVisual visual = grid.GridData.GetVisualAt(pos);

                if (visual != null)
                {
                    //Reset anchor
                    visual.transform.position = visualGrid.GridToWorldPosition(pos);

                    //Reset sprite transform
                    visual.ResetSpriteTransform();
                    visual.SetWorldPosition(visualGrid.GridToWorldPosition(pos));

                    //Re-initialize to ensure grid position
                    Element element = grid.GridData.GetElementAt(pos);
                    visual.Initialize(pos, element);

                    //Re-enable selection
                    visual.SetSelectable(true);
                }
            }
        }

        isProcessing = false;
        onComplete?.Invoke();
    }
}