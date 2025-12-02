using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Unity.VisualScripting.FullSerializer;
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
        //Nothing to update in idle state
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
        //Apply gravity and get positions that moved (logical swap already updates gridData visuals)
        List<Vector2Int> movedPositions = grid.GravityHandler.ApplyGravity();

        // Run the refill sequence as a coroutine so we can wait for gravity animation to finish
        grid.StartCoroutine(RefillSequence(movedPositions, refillDuration));
    }

    private IEnumerator RefillSequence(List<Vector2Int> movedPositions, float gravityDuration)
    {
        // Animate gravity and wait until it finishes
        if (visualGrid != null && movedPositions != null && movedPositions.Count > 0)
        {
            yield return grid.StartCoroutine(visualGrid.AnimateGravity(movedPositions, gravityDuration));
        }

        // After gravity animation completes, find empty slots and spawn new elements
        List<Vector2Int> emptyPositions = GetEmptyPositions();

        if (emptyPositions.Count > 0)
        {
            //Fill empty slots with new elements
            grid.FillEmptySlots(grid.AvailableElements);

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

            // Play the refill (scale-in) animation and then finalize
            visualGrid.Refill(refillDuration);

            // Wait for fill animation to complete then finalize (CompleteRefillAfterDelay will set isRefilling=false)
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

        for (int x = 0; x < grid.GridData.GridSize; x++)
        {
            for (int y = 0; y < grid.GridData.GridSize; y++)
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
    private readonly Grid grid;
    private readonly VisualGrid visualGrid;
    private readonly List<Element> availableElements;
    private readonly List<Spell> availableSpells;
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