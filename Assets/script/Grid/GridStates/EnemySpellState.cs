using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpellState : IGridState
{
    private Grid grid;
    private VisualGrid visualGrid;
    private List<Vector2Int> positions;
    private System.Action onComplete;
    private bool isProcessing;
    private float refillDuration = 0.15f;

    private bool rerollAfterProcess = false;

    public EnemySpellState(Grid gridController, VisualGrid visual, List<Vector2Int> positionsToProcess,
                           System.Action onCompleteCallback, bool rerollAfter = false)
    {
        grid = gridController;
        visualGrid = visual;
        positions = positionsToProcess ?? new List<Vector2Int>();
        onComplete = onCompleteCallback;
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
                grid.ChangeState(new RerollState(grid, visualGrid, grid.SpawnableElements,
                    GameManager.Instance.PlayerStats.GetSpellBook(), 3));
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

        // Update bomb (and any other persistent spell) state after gravity has settled,
        // mirroring what RefillState does so cleared offsets are recorded before Tick runs.
        grid.RetargetActiveSpell();

        List<Vector2Int> emptyPositions = GetEmptyPositions();

        if (emptyPositions.Count > 0)
        {
            grid.FillEmptySlots(grid.SpawnableElements, emptyPositions);

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
