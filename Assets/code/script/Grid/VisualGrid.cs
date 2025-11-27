using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class VisualGrid : MonoBehaviour
{
    [Header("Animation Curves")]
    [SerializeField] private AnimationCurve gravityRefillCurve;
    [SerializeField] private AnimationCurve swapCurve;
    [SerializeField] private AnimationCurve scrollCurve;
    [SerializeField] private AnimationCurve fillEmptyCurve;

    [Header("Prefab Holder")]
    [SerializeField] private GameObject elementVisualPrefab;

    private Grid grid;
    public bool isInitialized = false;
    private float xSpacing = 1.1f;
    private float ySpacing = 1.1f;
    private Vector3 startPosition;
    private void Awake()
    {
        startPosition = transform.position;
    }

    public void Initialize(Grid gridController)
    {
        grid = gridController;
        isInitialized = true;

        CreateAllVisuals();
    }

    public void PlayStartGravityAnimation(float duration)
    {
        StartCoroutine(StartGravityRoutine(duration));
    }

    public void FillEmpty(List<Vector2Int> positions, float duration)
    {
        StartCoroutine(FillEmptyRoutine(positions, duration));
    }

    public void Swap(Vector2Int posA, Vector2Int posB, float duration)
    {
        StartCoroutine(SwapRoutine(posA, posB, duration));
    }

    public void Refill(float duration)
    {
        StartCoroutine(RefillRoutine(duration));
    }

    public void PlayScrollAnimation(float scrollDuration, float columnDelay)
    {
        StartCoroutine(ScrollRoutine(scrollDuration, columnDelay));
    }

    public Vector3 GridToWorldPosition(Vector2Int gridPosition)
    {
        return startPosition + new Vector3(gridPosition.x * xSpacing, gridPosition.y * ySpacing, 0f);
    }

    public ElementVisual GetVisualAt(Vector2Int position)
    {
        if (grid == null || grid.GridData == null)
            return null;

        return grid.GridData.GetVisualAt(position);
    }

    private void CreateAllVisuals()
    {
        for (int x = 0; x < grid.GridData.GridSize; x++)
        {
            for (int y = 0; y < grid.GridData.GridSize; y++)
            {
                Vector2Int gridPos = new Vector2Int(x, y);
                Element element = grid.GridData.GetElementAt(gridPos);
                CreateElementVisual(gridPos, element);
            }
        }
    }

    private void CreateElementVisual(Vector2Int gridPosition, Element element)
    {
        // Delegate to EnsureVisualAt to centralize behavior.
        EnsureVisualAt(gridPosition, element);
    }

    public void EnsureVisualAt(Vector2Int gridPosition, Element element)
    {
        // VisualGrid must be initialized and grid data must exist.
        if (!isInitialized || grid == null || grid.GridData == null)
            return;

        if (element == null)
            return;

        ElementVisual existing = grid.GridData.GetVisualAt(gridPosition);
        if (existing == null)
        {
            if (elementVisualPrefab == null)
                return;

            GameObject visualObj = Instantiate(elementVisualPrefab, transform);
            ElementVisual visual = visualObj.GetComponent<ElementVisual>();
            if (visual == null)
                return;

            Vector3 worldPos = GridToWorldPosition(gridPosition);

            // Anchor the visual GameObject to the fixed grid world position.
            visual.transform.position = worldPos;

            // Initialize and make sure the sprite renderers are positioned relative to the anchor.
            visual.Initialize(gridPosition, element);
            visual.ResetSpriteTransform();
            visual.SetWorldPosition(worldPos); // ensure sprite is centered on anchor

            grid.GridData.SetVisualAt(gridPosition, visual);
        }
        else
        {
            // Reuse existing visual: set anchor and update the displayed element.
            Vector3 worldPos = GridToWorldPosition(gridPosition);
            existing.transform.position = worldPos;
            existing.Initialize(gridPosition, element);
            existing.ResetSpriteTransform();
            existing.SetWorldPosition(worldPos);
        }
    }

    
    public void UpdateAnchorsForPositions(List<Vector2Int> positions)
    {
        if (!isInitialized || grid == null || grid.GridData == null)
            return;

        if (positions == null || positions.Count == 0)
            return;

        foreach (var pos in positions)
        {
            if (!grid.GridData.IsPositionValid(pos))
                continue;

            ElementVisual visual = grid.GridData.GetVisualAt(pos);
            if (visual != null)
            {
                visual.transform.position = GridToWorldPosition(pos);
                // Make sure visual knows its grid position and sprites reset
                visual.Initialize(pos, grid.GridData.GetElementAt(pos));
                visual.ResetSpriteTransform();
                // Keep sprite renderers centered on anchor
                visual.SetWorldPosition(GridToWorldPosition(pos));
            }
        }
    }

    private void DestroyElementVisual(Vector2Int gridPosition)
    {
        ElementVisual visual = grid.GridData.GetVisualAt(gridPosition);
        if (visual != null)
        {
            Destroy(visual.gameObject);
            grid.GridData.SetVisualAt(gridPosition, null);
        }
    }

    private IEnumerator StartGravityRoutine(float duration)
    {
        Dictionary<ElementVisual, Vector3> startPositions = new Dictionary<ElementVisual, Vector3>();
        Dictionary<ElementVisual, Vector3> targetPositions = new Dictionary<ElementVisual, Vector3>();

        for (int x = 0; x < grid.GridData.GridSize; x++)
        {
            for (int y = 0; y < grid.GridData.GridSize; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                ElementVisual visual = grid.GridData.GetVisualAt(pos);

                if (visual != null)
                {
                    Vector3 targetPos = GridToWorldPosition(pos);
                    Vector3 startPos = targetPos + new Vector3(0, (grid.GridData.GridSize + 1) * ySpacing, 0);

                    startPositions[visual] = startPos;
                    targetPositions[visual] = targetPos;

                    // Move only the sprite renderers to the start position; the anchor remains fixed.
                    visual.SetWorldPosition(startPos);
                }
            }
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = gravityRefillCurve.Evaluate(elapsed / duration);

            foreach (var kvp in startPositions)
            {
                ElementVisual visual = kvp.Key;
                if (visual != null)
                {
                    Vector3 newPos = Vector3.Lerp(startPositions[visual], targetPositions[visual], t);
                    visual.SetWorldPosition(newPos);
                }
            }

            yield return null;
        }

        //snap final positions (sprite renderers)
        foreach (var kvp in targetPositions)
        {
            if (kvp.Key != null)
                kvp.Key.SetWorldPosition(kvp.Value);
        }
    }

    private IEnumerator FillEmptyRoutine(List<Vector2Int> positions, float duration)
    {
        Dictionary<ElementVisual, float> startScales = new Dictionary<ElementVisual, float>();

        foreach (var pos in positions)
        {
            ElementVisual visual = grid.GridData.GetVisualAt(pos);
            if (visual != null)
            {
                startScales[visual] = 0f;
                visual.SetSpriteScale(Vector3.zero);
            }
            else
            {
                // If a visual doesn't exist for a newly spawned element, create it so scaling animation works.
                Element element = grid.GridData.GetElementAt(pos);
                if (element != null)
                {
                    EnsureVisualAt(pos, element);
                    ElementVisual newVisual = grid.GridData.GetVisualAt(pos);
                    if (newVisual != null)
                    {
                        startScales[newVisual] = 0f;
                        newVisual.SetSpriteScale(Vector3.zero);
                    }
                }
            }
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = fillEmptyCurve.Evaluate(elapsed / duration);

            foreach (var kvp in startScales)
            {
                if (kvp.Key != null)
                    kvp.Key.SetSpriteScale(Vector3.one * t);
            }

            yield return null;
        }

        //Snap to final scale
        foreach (var kvp in startScales)
        {
            if (kvp.Key != null)
                kvp.Key.SetSpriteScale(Vector3.one);
        }
    }

    private IEnumerator SwapRoutine(Vector2Int posA, Vector2Int posB, float duration)
    {
        ElementVisual visualA = grid.GridData.GetVisualAt(posA);
        ElementVisual visualB = grid.GridData.GetVisualAt(posB);

        if (visualA == null || visualB == null)
            yield break;

        // Work with sprite positions so anchors remain fixed.
        Vector3 startPosA = visualA.GetWorldPosition();
        Vector3 startPosB = visualB.GetWorldPosition();
        Vector3 targetPosA = GridToWorldPosition(posB);
        Vector3 targetPosB = GridToWorldPosition(posA);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = swapCurve.Evaluate(elapsed / duration);

            visualA.SetWorldPosition(Vector3.Lerp(startPosA, targetPosA, t));
            visualB.SetWorldPosition(Vector3.Lerp(startPosB, targetPosB, t));

            yield return null;
        }

        visualA.SetWorldPosition(targetPosA);
        visualB.SetWorldPosition(targetPosB);
    }

    private IEnumerator RefillRoutine(float duration)
    {
        List<Vector2Int> emptyPositions = new List<Vector2Int>();

        for (int x = 0; x < grid.GridData.GridSize; x++)
        {
            for (int y = 0; y < grid.GridData.GridSize; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                Element element = grid.GridData.GetElementAt(pos);

                if (element == grid.EmptyElement || element == null)
                {
                    emptyPositions.Add(pos);
                }
            }
        }

        yield return FillEmptyRoutine(emptyPositions, duration);
    }

    private IEnumerator ScrollRoutine(float scrollDuration, float columnDelay)
    {
        for (int col = 0; col < grid.GridData.GridSize; col++)
        {
            StartCoroutine(ScrollColumnRoutine(col, scrollDuration));
            yield return new WaitForSeconds(columnDelay);
        }
    }

    private IEnumerator ScrollColumnRoutine(int column, float duration)
    {
        List<ElementVisual> columnVisuals = new List<ElementVisual>();

        for (int row = 0; row < grid.GridData.GridSize; row++)
        {
            ElementVisual visual = grid.GridData.GetVisualAt(new Vector2Int(column, row));
            if (visual != null)
                columnVisuals.Add(visual);
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = scrollCurve.Evaluate(elapsed / duration);
            float scrollAmount = Mathf.Sin(t * Mathf.PI * 4) * 0.5f;

            foreach (var visual in columnVisuals)
            {
                if (visual != null)
                {
                    Vector3 basePos = GridToWorldPosition(visual.GridPosition);
                    // Move only sprite renderers, leaving the anchor fixed.
                    visual.SetWorldPosition(basePos + new Vector3(0, scrollAmount, 0));
                }
            }

            yield return null;
        }

        foreach (var visual in columnVisuals)
        {
            if (visual != null)
            {
                visual.SetWorldPosition(GridToWorldPosition(visual.GridPosition));
            }
        }
    }
}