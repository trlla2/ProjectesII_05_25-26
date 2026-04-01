using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class MoonInstanceEffect : GridModifierEffectBase
{
    [SerializeField] private ElementType moonElementType = ElementType.Blue;
    [SerializeField] private float delayBetweenElements = 0.2f;

    protected override ElementType TargetElementType => moonElementType;

    protected override void OnPositionsSelected(Grid grid, Element targetElement,
                                                 List<Vector2Int> selectedPositions, Action onComplete)
    {
        Vector3 startPos = uiItemPosition != Vector3.zero ? uiItemPosition : grid.VisualGrid.transform.position + Vector3.up * 3f;

        startPos.y -= 5f;
        startPos.x += UnityEngine.Random.Range(-1f, 1f);

        List<GameObject> xMarkers = new List<GameObject>();
        Sequence selectSequence = DOTween.Sequence();

        for (int i = 0; i < selectedPositions.Count; i++)
        {
            int index = i;
            Vector2Int targetGridPos = selectedPositions[i];
            Vector3 targetWorldPos = grid.VisualGrid.GridToWorldPosition(targetGridPos);

            selectSequence.AppendCallback(() =>
            {
                xMarkers.Add(SpawnXMarker(targetWorldPos));
                ElementVisual vis = grid.GridData.GetVisualAt(targetGridPos);
                vis?.transform.DOPunchScale(Vector3.one * 0.3f, 0.3f, 5, 0.5f);
            });

            if (i < selectedPositions.Count - 1)
                selectSequence.AppendInterval(0.1f);
        }

        selectSequence.AppendInterval(0.1f);

        selectSequence.OnComplete(() =>
        {
            Sequence fireSequence = DOTween.Sequence();

            for (int i = 0; i < selectedPositions.Count; i++)
            {
                int index = i;
                Vector2Int targetGridPos = selectedPositions[i];
                Vector3 targetWorldPos = grid.VisualGrid.GridToWorldPosition(targetGridPos);
                int curveDir = index == 0 ? -1 : (index == selectedPositions.Count - 1 ? 1 : 0);

                fireSequence.AppendCallback(() =>
                {
                    GameObject xMarker = index < xMarkers.Count ? xMarkers[index] : null;
                    AnimateElementToGrid(grid, targetElement, startPos, targetWorldPos, targetGridPos, curveDir, xMarker);
                });

                if (i < selectedPositions.Count - 1)
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