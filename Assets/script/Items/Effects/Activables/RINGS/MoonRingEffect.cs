using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class MoonRingEffect : ActivatableEffectBase
{
    [SerializeField] private ElementType blueElementType = ElementType.Blue;
    [SerializeField] private int elementCount = 3;
    [SerializeField] private float delayBetweenElements = 0.2f;

    public override bool CanActivate(ItemContext context)
    {
        return context.Grid != null &&
               context.Grid.CurrentState == GridStateType.Idle &&
               GetAvailablePositions(context.Grid, blueElementType).Count >= elementCount;
    }

    public override void Activate(ItemLevel level, ItemContext context, Action onComplete)
    {
        var grid = context.Grid;
        var positions = GetRandomPositions(grid, elementCount, blueElementType);

        if (positions.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        grid.ChangeState(new ItemProcessingState(grid));

        Element blueElement = GetSpecificElement(grid, blueElementType);
        if (blueElement == null)
        {
            grid.ChangeState(new IdleState(grid));
            onComplete?.Invoke();
            return;
        }

        Vector3 startPos = uiItemPosition != Vector3.zero ? uiItemPosition : grid.VisualGrid.transform.position + Vector3.up * 3f;
        startPos.y -= 5f;
        startPos.x += UnityEngine.Random.Range(-1f, 1f);

        List<GameObject> xMarkers = new List<GameObject>();
        Sequence selectSequence = DOTween.Sequence();

        for (int i = 0; i < positions.Count; i++)
        {
            int index = i;
            Vector2Int targetGridPos = positions[i];
            Vector3 targetWorldPos = grid.VisualGrid.GridToWorldPosition(targetGridPos);

            selectSequence.AppendCallback(() =>
            {
                xMarkers.Add(SpawnXMarker(targetWorldPos));
                ElementVisual visual = grid.GridData.GetVisualAt(targetGridPos);
                visual?.transform.DOPunchScale(Vector3.one * 0.3f, 0.3f, 5, 0.5f);
            });

            if (i < positions.Count - 1)
                selectSequence.AppendInterval(0.1f);
        }

        selectSequence.AppendInterval(0.1f);

        selectSequence.OnComplete(() =>
        {
            Sequence fireSequence = DOTween.Sequence();

            for (int i = 0; i < positions.Count; i++)
            {
                int index = i;
                Vector2Int targetGridPos = positions[i];
                Vector3 targetWorldPos = grid.VisualGrid.GridToWorldPosition(targetGridPos);
                int curveDir = index == 0 ? -1 : (index == positions.Count - 1 ? 1 : 0);

                fireSequence.AppendCallback(() =>
                {
                    GameObject xMarker = index < xMarkers.Count ? xMarkers[index] : null;
                    AnimateElementToGrid(grid, blueElement, startPos, targetWorldPos, targetGridPos, curveDir, xMarker);
                });

                if (i < positions.Count - 1)
                    fireSequence.AppendInterval(delayBetweenElements);
            }

            fireSequence.AppendInterval(animationDuration);
            fireSequence.OnComplete(() =>
            {
                grid.ChangeState(new IdleState(grid));
                onComplete?.Invoke();
            });
        });
    }
}