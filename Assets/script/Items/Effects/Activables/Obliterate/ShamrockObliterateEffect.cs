using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class ShamrockObliterateEffect : ActivatableEffectBase
{
    [SerializeField] private float delayBetweenElements = 0.15f;

    public override bool CanActivate(ItemContext context)
    {
        return context.Grid != null &&
               context.Grid.CurrentState == GridStateType.Idle &&
               GetShamrockPositions(context.Grid).Count > 0;
    }

    public override void Activate(ItemLevel level, ItemContext context, Action onComplete)
    {
        var grid = context.Grid;    
        var positions = GetShamrockPositions(grid);
                
        if (positions.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        List<Element> shamrockElements = new List<Element>();
        foreach (var pos in positions)
            shamrockElements.Add(grid.GetElementAt(pos));

        grid.ChangeState(new ItemProcessingState(grid));

        List<GameObject> xMarkers = new List<GameObject>();
        Sequence selectSequence = DOTween.Sequence();

        for (int i = 0; i < positions.Count; i++)
        {
            Vector2Int targetGridPos = positions[i];
            Vector3 targetWorldPos = grid.VisualGrid.GridToWorldPosition(targetGridPos);

            selectSequence.AppendCallback(() =>
            {
                xMarkers.Add(SpawnXMarker(targetWorldPos));
                ElementVisual visual = grid.GridData.GetVisualAt(targetGridPos);
                visual?.transform.DOPunchScale(Vector3.one * 0.4f, 0.3f, 5, 0.5f);
            });

            if (i < positions.Count - 1)
                selectSequence.AppendInterval(delayBetweenElements);
        }

        selectSequence.AppendInterval(0.2f);

        selectSequence.OnComplete(() =>
        {
            foreach (var marker in xMarkers)
                GameObject.Destroy(marker);

            AttackWithElements(context, positions, shamrockElements, onComplete);
        });
    }

    private List<Vector2Int> GetShamrockPositions(Grid grid)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        for (int x = 0; x < grid.GridSize; x++)
        {
            for (int y = 0; y < grid.GridSize; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                Element element = grid.GetElementAt(pos);
                ElementVisual visual = grid.GetElementVisualAt(pos);

                if (element != null &&
                    (element.Type == ElementType.Green || element.Type == ElementType.Melon)&&
                    visual != null &&
                    !visual.IsBlocked)
                {
                    result.Add(pos);
                }
            }
        }
        return result;
    }
}