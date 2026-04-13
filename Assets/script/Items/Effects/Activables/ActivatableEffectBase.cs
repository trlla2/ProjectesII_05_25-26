using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[System.Serializable]
public abstract class ActivatableEffectBase : IActivatableEffect
{
    [SerializeField] protected float animationDuration = 0.6f;
    [SerializeField] protected float curveStrength = 1.5f;
    [SerializeField] public Sprite xSprite;

    protected Vector3 uiItemPosition;

    public void SetUIPosition(Vector3 position) => uiItemPosition = position;

    public virtual void Equip(PlayerStats target, ItemLevel level) { }
    public virtual void UnEquip(PlayerStats target, ItemLevel level) { }

    public abstract bool CanActivate(ItemContext context);
    public abstract void Activate(ItemLevel level, ItemContext context, Action onComplete);

    // Returns positions that are non-empty, non-bomb, non-blocked, and not the excluded element type
    protected List<Vector2Int> GetAvailablePositions(Grid grid, ElementType excludedType)
    {
        List<Vector2Int> available = new List<Vector2Int>();
        for (int x = 0; x < Grid.GridSize; x++)
        {
            for (int y = 0; y < Grid.GridSize; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                Element element = grid.GetElementAt(pos);
                ElementVisual visual = grid.GetElementVisualAt(pos);

                if (element != null &&
                    element.Type != ElementType.EMPTY &&
                    element.Type != ElementType.Bomb &&
                    visual != null && !visual.IsBlocked &&
                    element.Type != excludedType)
                {
                    available.Add(pos);
                }
            }
        }
        return available;
    }

    protected List<Vector2Int> GetRandomPositions(Grid grid, int count, ElementType excludedType)
    {
        List<Vector2Int> available = GetAvailablePositions(grid, excludedType);
        List<Vector2Int> selected = new List<Vector2Int>();

        while (selected.Count < count && available.Count > 0)
        {
            int idx = UnityEngine.Random.Range(0, available.Count);
            selected.Add(available[idx]);
            available.RemoveAt(idx);
        }
        return selected;
    }

    protected GameObject SpawnXMarker(Vector3 worldPos)
    {
        GameObject xObj = new GameObject("ItemEffect_XMarker");
        SpriteRenderer sr = xObj.AddComponent<SpriteRenderer>();
        sr.sprite = xSprite;
        sr.color = Color.white;
        sr.sortingOrder = 101;
        xObj.transform.position = worldPos;
        xObj.transform.localScale = Vector3.zero;
        xObj.transform.DOScale(Vector3.one, 0.1f).SetEase(Ease.OutBack);
        return xObj;
    }

    protected Element GetSpecificElement(Grid grid, ElementType type)
    {
        foreach (var element in grid.AvailableElements)
        {
            if (element.Type == type)
                return element;
        }
        return grid.AvailableElements.Count > 0 ? grid.AvailableElements[0] : null;
    }

    protected void AnimateElementToGrid(Grid grid, Element element, Vector3 startPos,
        Vector3 targetPos, Vector2Int gridPos, int curveDirection, GameObject xMarker = null)
    {
        GameObject tempVisual = new GameObject("ItemEffect_Element");
        SpriteRenderer sr = tempVisual.AddComponent<SpriteRenderer>();
        sr.sprite = element.Sprite;
        sr.color = element.Color;
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
            grid.SetElementAt(gridPos, element);
            ElementVisual visual = grid.GridData.GetVisualAt(gridPos);
            visual?.UpdateVisual(element);

            CameraShake cameraShake = Camera.main.GetComponent<CameraShake>();
            cameraShake?.Shake(0.1f, 0.08f);

            HapticsManager.Instance?.GenerateHapticFeedback(HapticsManager.HapticTypes.LightImpact);

            GameObject.Destroy(xMarker);
            GameObject.Destroy(tempVisual);
        });
    }

    protected void AttackWithElements(ItemContext context, List<Vector2Int> positions, List<Element> elements, Action onComplete, bool tickEnemyProgress = false)
    {
        Grid grid = context.Grid;

        NotifyBlockerEnemyOfUsedPositions(positions);

        grid.InstantVisualEmpty(positions);

        // Animate clones along the path
        context.SpellAnimator.OnSpellCast(positions.ToArray(), elements, () =>
        {
            // Destroy ElementVisual objects and queue refill
            grid.DeleteElementsByPlayer(positions);

            if (tickEnemyProgress || GameManager.Instance.CombatHandler.GetEnemyRef().ActiveSpell is BombSpell)
                GameManager.Instance.NotifyPlayerMove();

            grid.ChangeState(new IdleState(grid));
            onComplete?.Invoke();
        });
    }
    
    //need to reeimplement it here from playerpathing
    protected void NotifyBlockerEnemyOfUsedPositions(List<Vector2Int> usedPositions)
    {
        if (GameManager.Instance == null || GameManager.Instance.CombatHandler == null)
            return;

        EnemyBehaviour enemy = GameManager.Instance.CombatHandler.GetEnemyRef();
        if (enemy == null)
            return;

        enemy.NotifyAdjacentElementsUsed(usedPositions);
    }
}