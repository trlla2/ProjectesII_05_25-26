using System;
using System.Collections.Generic;
using UnityEngine;

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
        for (int x = 0; x < grid.GridSize; x++)
        {
            for (int y = 0; y < grid.GridSize; y++)
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

    protected Element GetSpecificElement(Grid grid, ElementType type)
    {
        foreach (var element in grid.AvailableElements)
        {
            if (element.Type == type)
                return element;
        }
        return grid.AvailableElements.Count > 0 ? grid.AvailableElements[0] : null;
    }

    // Thin wrappers kept for backward compatibility with derived effects.
    // All visual/audio/haptic work lives in ItemEffectFeedback.
    protected GameObject SpawnXMarker(Vector3 worldPos)
    {
        return ItemEffectFeedback.SpawnXMarker(worldPos, xSprite);
    }

    protected void AnimateElementToGrid(Grid grid, Element element, Vector3 startPos,
        Vector3 targetPos, Vector2Int gridPos, int curveDirection, GameObject xMarker = null)
    {
        ItemEffectFeedback.AnimateClonedElement(element, startPos, targetPos, curveDirection,
            animationDuration, curveStrength, () =>
        {
            // Gameplay state mutation (heavy lifting) stays here
            grid.SetElementAt(gridPos, element);
            ElementVisual visual = grid.GridData.GetVisualAt(gridPos);
            visual?.UpdateVisual(element);

            // Feedback delegated to the presentation layer
            ItemEffectFeedback.PlayElementPlacedFeedback();

            if (xMarker != null)
                GameObject.Destroy(xMarker);
        });
    }

    protected void AttackWithElements(ItemContext context, List<Vector2Int> positions, List<Element> elements, Action onComplete, bool tickEnemyProgress = false, bool chargeOrb = false)
    {
        Grid grid = context.Grid;

        NotifyBlockerEnemyOfUsedPositions(positions);

        if (chargeOrb && GameManager.Instance.CombatHandler.isBossBattle)
            grid.CollectAndChargeWeakElements(positions);

        grid.InstantVisualEmpty(positions);

        context.SpellAnimator.OnSpellCast(positions.ToArray(), elements, () =>
        {
            grid.DeleteElementsByPlayer(positions);
            IEnemySpell enemySpell = GameManager.Instance.CombatHandler.GetEnemyRef().ActiveSpell;
            if (tickEnemyProgress || enemySpell is BombSpell || enemySpell is DoubleBomberSpell || enemySpell is HiddenDestroySpell || enemySpell is BlockerSpell)
                GameManager.Instance.NotifyPlayerMove();

            grid.ChangeState(new IdleState(grid));
            onComplete?.Invoke();
        });
    }

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
