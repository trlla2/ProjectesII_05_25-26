using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


//same as other rrings
[System.Serializable]
public class MoonRingEffect : IActivatableEffect
{
    [SerializeField] private ElementType blueElementType = ElementType.Blue;
    [SerializeField] private int elementCount = 3;
    [SerializeField] private float delayBetweenElements = 0.2f;
    [SerializeField] private float animationDuration = 0.6f;
    [SerializeField] private float curveStrength = 1.5f;
    [SerializeField] public Sprite xSprite;

    private Vector3 uiItemPosition;

    public void SetUIPosition(Vector3 position) => uiItemPosition = position;

    public void Equip(PlayerStats target, ItemLevel level) { }
    public void UnEquip(PlayerStats target, ItemLevel level) { }

    public bool CanActivate(ItemContext context)
    {
        return context.Grid != null &&
               context.Grid.CurrentState == GridStateType.Idle &&
               GetAvailablePositions(context.Grid).Count >= elementCount;
    }

    public void Activate(ItemLevel level, ItemContext context, Action onComplete)
    {
        var grid = context.Grid;
        var positions = GetRandomPositions(grid, elementCount);

        if (positions.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        grid.ChangeState(new ItemProcessingState(grid));

        Element blueElement = GetBlueElement(grid);
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

    private GameObject SpawnXMarker(Vector3 worldPos)
    {
        GameObject xObj = new GameObject("MoonRing_XMarker");
        SpriteRenderer sr = xObj.AddComponent<SpriteRenderer>();
        sr.sprite = xSprite;
        sr.color = Color.white;
        sr.sortingOrder = 101;
        xObj.transform.position = worldPos;
        xObj.transform.localScale = Vector3.zero;
        xObj.transform.DOScale(Vector3.one, 0.1f).SetEase(Ease.OutBack);
        return xObj;
    }

    private void AnimateElementToGrid(Grid grid, Element blueElement, Vector3 startPos, Vector3 targetPos, Vector2Int gridPos, int curveDirection, GameObject xMarker = null)
    {
        GameObject tempVisual = new GameObject("MoonRing_Element");
        SpriteRenderer sr = tempVisual.AddComponent<SpriteRenderer>();
        sr.sprite = blueElement.Sprite;
        sr.color = blueElement.Color;
        sr.sortingOrder = 100;
        tempVisual.transform.position = startPos;
        tempVisual.transform.localScale = Vector3.one * 0.2f;

        Tween moveTween;
        if (curveDirection == 0)
        {
            moveTween = tempVisual.transform.DOMove(targetPos, animationDuration).SetEase(Ease.OutQuad);
        }
        else
        {
            Vector3 midPoint = (startPos + targetPos) / 2f;
            midPoint.x += curveDirection * curveStrength;

            Vector3[] path = { startPos, midPoint, targetPos };
            moveTween = tempVisual.transform.DOPath(path, animationDuration, PathType.CatmullRom).SetEase(Ease.OutQuad);
        }

        tempVisual.transform.DOScale(Vector3.one, animationDuration).SetEase(Ease.OutBack);

        moveTween.OnComplete(() =>
        {
            grid.SetElementAt(gridPos, blueElement);
            ElementVisual visual = grid.GridData.GetVisualAt(gridPos);
            visual?.UpdateVisual(blueElement);

            CameraShake cameraShake = Camera.main.GetComponent<CameraShake>();
            cameraShake?.Shake(0.1f, 0.08f);

            HapticsManager.Instance?.GenerateHapticFeedback(HapticsManager.HapticTypes.LightImpact);

            GameObject.Destroy(xMarker);
            GameObject.Destroy(tempVisual);
        });
    }

    private List<Vector2Int> GetAvailablePositions(Grid grid)
    {
        List<Vector2Int> available = new List<Vector2Int>();
        for (int x = 0; x < Grid.GridSize; x++)
        {
            for (int y = 0; y < Grid.GridSize; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                Element element = grid.GetElementAt(pos);
                if (element != null && element.Type != ElementType.EMPTY && element.Type != ElementType.Bomb && !grid.GetElementVisualAt(pos).IsBlocked && element.Type != ElementType.Blue)
                    available.Add(pos);
            }
        }
        return available;
    }

    private List<Vector2Int> GetRandomPositions(Grid grid, int count)
    {
        List<Vector2Int> available = GetAvailablePositions(grid);
        List<Vector2Int> selected = new List<Vector2Int>();

        while (selected.Count < count && available.Count > 0)
        {
            int idx = UnityEngine.Random.Range(0, available.Count);
            selected.Add(available[idx]);
            available.RemoveAt(idx);
        }
        return selected;
    }

    private Element GetBlueElement(Grid grid)
    {
        foreach (var element in grid.AvailableElements)
        {
            if (element.Type == blueElementType)
                return element;
        }
        return grid.AvailableElements.Count > 0 ? grid.AvailableElements[0] : null;
    }
}