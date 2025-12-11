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

    [Header("Spacing")]
    [SerializeField] private float xSpacing = 1.1f;
    [SerializeField] private float ySpacing = 1.1f;
    public Vector3 startPosition;

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

    public IEnumerator PlayRerollAnimation(int scrollTimes, float scrollDuration, float columnDelay, 
                                            List<Element> availableElements, Element[,] finalGrid)
    {
        yield return StartCoroutine(RerollScrollRoutine(scrollTimes, scrollDuration, columnDelay, availableElements, finalGrid));
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
        EnsureVisualAt(gridPosition, element);
    }

    public void EnsureVisualAt(Vector2Int gridPosition, Element element)
    {
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

            visual.transform.position = worldPos;
            visual.Initialize(gridPosition, element);
            visual.ResetSpriteTransform();
            visual.SetWorldPosition(worldPos);

            grid.GridData.SetVisualAt(gridPosition, visual);
        }
        else
        {
            Vector3 worldPos = GridToWorldPosition(gridPosition);
            existing.transform.position = worldPos;
            existing.Initialize(gridPosition, element);
            existing.ResetSpriteTransform();
            existing.SetWorldPosition(worldPos);
        }
    }

    public void RemoveVisualAt(Vector2Int gridPosition)
    {
        if (!isInitialized || grid == null || grid.GridData == null)
            return;

        ElementVisual visual = grid.GridData.GetVisualAt(gridPosition);
        if (visual != null)
        {
            Destroy(visual.gameObject);
            grid.GridData.SetVisualAt(gridPosition, null);
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
                visual.Initialize(pos, grid.GridData.GetElementAt(pos));
                visual.ResetSpriteTransform();
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

    public IEnumerator AnimateGravity(List<Vector2Int> movedPositions, float duration)
    {
        yield return StartCoroutine(GravityRoutine(movedPositions, duration));
    }

    private IEnumerator GravityRoutine(List<Vector2Int> movedPositions, float duration)
    {
        if (!isInitialized || grid == null || grid.GridData == null || movedPositions == null || movedPositions.Count == 0)
            yield break;

        var items = new List<(ElementVisual visual, Vector3 start, Vector3 target, Vector2Int targetGridPos)>(movedPositions.Count);

        foreach (var pos in movedPositions)
        {
            if (!grid.GridData.IsPositionValid(pos))
                continue;

            ElementVisual visual = grid.GridData.GetVisualAt(pos);
            if (visual == null)
                continue;

            Vector3 startWorld = visual.GetWorldPosition();
            Vector3 targetWorld = GridToWorldPosition(pos);

            items.Add((visual, startWorld, targetWorld, pos));
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = gravityRefillCurve.Evaluate(Mathf.Clamp01(elapsed / duration));

            foreach (var item in items)
            {
                if (item.visual != null)
                    item.visual.SetWorldPosition(Vector3.Lerp(item.start, item.target, t));
            }

            yield return null;
        }

        foreach (var item in items)
        {
            if (item.visual == null)
                continue;

            item.visual.SetWorldPosition(item.target);
            item.visual.transform.position = item.target;

            Element element = grid.GridData.GetElementAt(item.targetGridPos);
            item.visual.Initialize(item.targetGridPos, element);

            item.visual.ResetSpriteTransform();
            item.visual.SetWorldPosition(item.target);
        }
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
                    bool isBelowTargeted = false;
                    for (int yAbove = y + 1; yAbove < grid.GridData.GridSize; yAbove++)
                    {
                        Vector2Int abovePos = new Vector2Int(x, yAbove);
                        ElementVisual aboveVisual = grid.GridData.GetVisualAt(abovePos);
                        if (aboveVisual != null && aboveVisual.IsTargetedByEnemy)
                        {
                            isBelowTargeted = true;
                            break;
                        }
                    }

                    if (!isBelowTargeted)
                    {
                        emptyPositions.Add(pos);
                    }
                }
            }
        }

        if (emptyPositions.Count > 0)
            yield return FillEmptyRoutine(emptyPositions, duration);
    }

    private IEnumerator RerollScrollRoutine(int scrollTimes, float scrollDuration, float columnDelay,
                                             List<Element> availableElements, Element[,] finalGrid)
    {
        int gridSize = grid.GridData.GridSize;
        float gridHeight = gridSize * ySpacing;

        float upperThreshold = startPosition.y + (gridSize - 1) * ySpacing;
        float lowerThreshold = startPosition.y;

        List<List<(GameObject obj, SpriteRenderer renderer, GameObject background)>> columnClones = new List<List<(GameObject, SpriteRenderer, GameObject)>>();

        for (int col = 0; col < gridSize; col++)
        {
            List<(GameObject, SpriteRenderer, GameObject)> clones = new List<(GameObject, SpriteRenderer, GameObject)>();

            for (int cloneIndex = 0; cloneIndex < scrollTimes; cloneIndex++)
            {
                for (int row = 0; row < gridSize; row++)
                {
                    Element element;
                    if (cloneIndex == scrollTimes - 1)
                    {
                        element = finalGrid[col, row];
                    }
                    else
                    {
                        element = availableElements[Random.Range(0, availableElements.Count)];
                    }

                    Vector3 spawnPos = startPosition + new Vector3(
                        col * xSpacing,
                        (gridSize + row + (cloneIndex * gridSize)) * ySpacing,
                        0f
                    );

                    GameObject cloneObj = Instantiate(elementVisualPrefab, spawnPos, Quaternion.identity, transform);
                    SpriteRenderer sr = cloneObj.GetComponentInChildren<SpriteRenderer>();
                    GameObject cloneBackground = cloneObj.transform.Find("Background")?.gameObject;
                    if (sr != null && element != null)
                    {
                        sr.sprite = element.Sprite;
                        sr.color = element.Color;
                        sr.enabled = false;
                        if (cloneBackground != null)
                            cloneBackground.SetActive(false);
                    }

                    clones.Add((cloneObj, sr, cloneBackground));
                }
            }

            columnClones.Add(clones);
        }

        float totalScrollDistance = scrollTimes * gridHeight;

        for (int col = 0; col < gridSize; col++)
        {
            StartCoroutine(ScrollColumnDown(col, columnClones[col], totalScrollDistance, scrollDuration, finalGrid, upperThreshold, lowerThreshold));
            yield return new WaitForSeconds(columnDelay);
        }

        yield return new WaitForSeconds(scrollDuration);

        foreach (var cloneList in columnClones)
        {
            foreach (var clone in cloneList)
            {
                if (clone.obj != null)
                    Destroy(clone.obj);
            }
        }
    }

    private IEnumerator ScrollColumnDown(int column, List<(GameObject obj, SpriteRenderer renderer, GameObject background)> clones, 
                                          float totalDistance, float duration, Element[,] finalGrid, float upperThreshold, float lowerThreshold)
    {
        int gridSize = grid.GridData.GridSize;

        List<(ElementVisual visual, SpriteRenderer renderer, GameObject background)> originalVisuals = new List<(ElementVisual, SpriteRenderer, GameObject)>();
        List<Vector3> originalStartPositions = new List<Vector3>();

        for (int row = 0; row < gridSize; row++)
        {
            ElementVisual visual = grid.GridData.GetVisualAt(new Vector2Int(column, row));
            if (visual != null)
            {
                SpriteRenderer sr = visual.GetComponentInChildren<SpriteRenderer>();
                GameObject bg = visual.transform.Find("Background")?.gameObject;
                originalVisuals.Add((visual, sr, bg));
                originalStartPositions.Add(visual.GetWorldPosition());
            }
        }

        List<Vector3> cloneStartPositions = new List<Vector3>();
        foreach (var clone in clones)
        {
            cloneStartPositions.Add(clone.obj.transform.position);
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = scrollCurve.Evaluate(Mathf.Clamp01(elapsed / duration));
            float offset = t * totalDistance;

            for (int i = 0; i < originalVisuals.Count; i++)
            {
                if (originalVisuals[i].visual != null)
                {
                    Vector3 newPos = originalStartPositions[i] - new Vector3(0, offset, 0);
                    originalVisuals[i].visual.SetWorldPosition(newPos);

                    bool isVisible = newPos.y <= upperThreshold && newPos.y >= lowerThreshold;

                    if (originalVisuals[i].renderer != null)
                    {
                        originalVisuals[i].renderer.enabled = isVisible;
                    }

                    if (originalVisuals[i].background != null)
                    {
                        originalVisuals[i].background.SetActive(isVisible);
                    }
                }
            }

            for (int i = 0; i < clones.Count; i++)
            {
                if (clones[i].obj != null)
                {
                    Vector3 newPos = cloneStartPositions[i] - new Vector3(0, offset, 0);
                    clones[i].obj.transform.position = newPos;

                    bool isVisible = newPos.y <= upperThreshold && newPos.y >= lowerThreshold;

                    if (clones[i].renderer != null)
                    {
                        clones[i].renderer.enabled = isVisible;
                    }

                    if (clones[i].background != null)
                    {
                        clones[i].background.SetActive(isVisible);
                    }
                }
            }

            yield return null;
        }

        for (int row = 0; row < gridSize; row++)
        {
            Vector2Int pos = new Vector2Int(column, row);
            ElementVisual visual = grid.GridData.GetVisualAt(pos);
            Element element = finalGrid[column, row];

            if (visual != null)
            {
                Vector3 finalPos = GridToWorldPosition(pos);
                visual.transform.position = finalPos;
                visual.Initialize(pos, element);
                visual.UpdateVisual(element);
                visual.ResetSpriteTransform();
                visual.SetWorldPosition(finalPos);

                SpriteRenderer sr = visual.GetComponentInChildren<SpriteRenderer>();
                if (sr != null)
                    sr.enabled = true;

                GameObject bg = visual.transform.Find("Background")?.gameObject;
                if (bg != null)
                    bg.SetActive(true);
            }
        }
    }
}