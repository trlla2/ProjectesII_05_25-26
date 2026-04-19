using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[System.Serializable]
public abstract class GridModifierEffectBase : ActivatableEffectBase, IGridModifierEffect
{
    [SerializeField] protected int selectCount = 1;

    // Set by ItemUIButton; called with a continuation after selection so the
    // UI animation plays before the grid animation fires.
    [NonSerialized] public Action<Action> BeforeAnimation;

    protected abstract ElementType TargetElementType { get; }

    public override bool CanActivate(ItemContext context)
    {
        return context.Grid != null &&
               context.Grid.CurrentState == GridStateType.Idle &&
               GetAvailablePositions(context.Grid, TargetElementType).Count >= selectCount;
    }

    public override void Activate(ItemLevel level, ItemContext context, Action onComplete)
    {
        var grid = context.Grid;

        Element targetElement = GetSpecificElement(grid, TargetElementType);
        if (targetElement == null)
        {
            onComplete?.Invoke();
            return;
        }

        grid.ChangeState(new ItemProcessingState(grid));
        grid.StartCoroutine(SelectionPhase(grid, targetElement, onComplete));
    }

    // Called once the player has confirmed all positions; subclasses drive the animation
    protected abstract void OnPositionsSelected(Grid grid, Element targetElement,
                                                 List<Vector2Int> selectedPositions, Action onComplete);

    private IEnumerator SelectionPhase(Grid grid, Element targetElement, Action onComplete)
    {
        List<Vector2Int> availablePositions = GetAvailablePositions(grid, TargetElementType);
        List<Vector2Int> selectedPositions = new List<Vector2Int>();

        grid.VisualGrid.ShowInactiveGrid(true);

        while (selectedPositions.Count < selectCount)
        {
            //same as InputHander 
            if (Input.GetMouseButtonDown(0))
            {
                Camera cam = Camera.main;
                if (cam != null)
                {
                    Vector3 worldPoint = cam.ScreenToWorldPoint(Input.mousePosition);
                    Vector2 point2D = new Vector2(worldPoint.x, worldPoint.y);
                    Collider2D hit = Physics2D.OverlapPoint(point2D);

                    if (hit != null)
                    {
                        ElementVisual visual = hit.GetComponent<ElementVisual>();
                        if (visual != null)
                        {
                            Vector2Int pos = visual.GridPosition;
                            if (availablePositions.Contains(pos) && !selectedPositions.Contains(pos))
                            {
                                selectedPositions.Add(pos);
                                visual.ShowInactive(false);
                                visual.transform.DOPunchScale(Vector3.one * 0.3f, 0.3f, 5, 0.5f);
                                HapticsManager.Instance?.GenerateHapticFeedback(HapticsManager.HapticTypes.Selection);
                            }
                        }
                    }
                }
            }

            yield return null;
        }

        yield return new WaitForSeconds(0.2f);
        grid.VisualGrid.ShowInactiveGrid(false);

        // If a UI animation was injected play it first 
        if (BeforeAnimation != null)
            BeforeAnimation(() => OnPositionsSelected(grid, targetElement, selectedPositions, onComplete));
        else
            OnPositionsSelected(grid, targetElement, selectedPositions, onComplete);
    }
}