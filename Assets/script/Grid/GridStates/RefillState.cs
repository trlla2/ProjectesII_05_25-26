using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RefillState : IGridState
{
    private Grid grid;
    private VisualGrid visualGrid;
    private bool isRefilling;
    private float refillDuration = 0.15f;

    private System.Action onRefillComplete;

    public RefillState(Grid gridController, VisualGrid visual, System.Action onRefillComplete = null)
    {
        grid = gridController;
        visualGrid = visual;
        this.onRefillComplete = onRefillComplete;
    }

    public void Enter()
    {
        visualGrid.RemoveAllHighlights();
        isRefilling = true;
        PerformRefill();
    }

    public void Update()
    {
        if (!isRefilling)
        {
            if (onRefillComplete != null)
            {
                onRefillComplete.Invoke();
            }
            else
            {
                grid.ChangeState(new ValidateSpellState(grid, GameManager.Instance.PlayerStats.GetSpellBook()));
            }
        }
    }

    public void Exit()
    {
        isRefilling = false;
    }

    public GridStateType GetStateType() => GridStateType.Refill;

    private void PerformRefill()
    {
        //pass through the slots that were emptied by gravity
        List<Vector2Int> movedPositions = grid.GravityHandler.ApplyGravity();
        grid.StartCoroutine(RefillSequence(movedPositions, refillDuration));
    }

    private IEnumerator RefillSequence(List<Vector2Int> movedPositions, float gravityDuration)
    {
        if (visualGrid != null && movedPositions != null && movedPositions.Count > 0)
        {
            yield return grid.StartCoroutine(visualGrid.AnimateGravity(movedPositions, gravityDuration));
        }

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

            //finalize visuals
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
}
