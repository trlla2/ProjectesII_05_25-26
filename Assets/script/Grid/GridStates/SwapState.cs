using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapState : IGridState
{
    private Grid grid;
    private VisualGrid visualGrid;
    private List<(Vector2Int, Vector2Int)> swapPairs;
    private float swapDuration;
    private bool isSwapping;

    public SwapState(Grid gridController, VisualGrid visual, List<(Vector2Int, Vector2Int)> pairs, float swapDuration)
    {
        grid = gridController;
        visualGrid = visual;
        swapPairs = pairs;
        this.swapDuration = swapDuration;
    }

    public void Enter()
    {
        isSwapping = true;
        grid.StartCoroutine(PerformSwap());
    }

    public void Update()
    {
        if (!isSwapping)
            grid.ChangeState(new ValidateSpellState(grid, GameManager.Instance.PlayerStats.GetSpellBook()));
    }

    public void Exit()
    {
        isSwapping = false;
    }

    public GridStateType GetStateType() => GridStateType.Swap;

    private IEnumerator PerformSwap()
    {
        foreach (var pair in swapPairs)
            grid.GridData.SwapElements(pair.Item1, pair.Item2);

        yield return grid.StartCoroutine(visualGrid.AnimateEnemySwap(swapPairs, swapDuration));

        isSwapping = false;
    }
}
