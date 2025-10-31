using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Slot
{
    public Element CurrentElement;
    public Vector2Int GridPosition;
    public GameObject Holder;
    public ScaleAnimator scaleAnimator;
    public bool Animated = false;
    public bool IsFalling = false;

    public Slot(Element e, Vector2Int pos)
    {
        CurrentElement = e;
        GridPosition = pos;
    }

    public void SetElement(Element e)
    {
        CurrentElement = e;
    }

    public void UpdateGridPosition(Vector2Int newPos)
    {
        GridPosition = newPos;
    }
}

public class Grid : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int columns = 4;
    [SerializeField] private int visibleRows = 4;
    [SerializeField] private int extraSpawnRows = 3; 

    [Header("Visual Settings")]
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private float slotSpacing = 1.1f;
    [SerializeField] private Vector3 gridOffset = Vector3.zero;

    [Header("Falling Animation")]
    [SerializeField] private AnimationCurve fallCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float fallDuration = 0.5f;

    [Header("Elements")]
    [SerializeField] private List<Element> availableElements;
    public List<Spell> availableSpells = new List<Spell>();

    // Grid data structure: [column, row]
    private Slot[,] grid;
    private int totalRows;
    private List<Slot> activeSlots = new List<Slot>();
    private Dictionary<Element, int> elementCounts = new Dictionary<Element, int>();

    [HideInInspector] public bool IsPlayerPathing = false;
    private bool isProcessingGravity = false;

    private void Start()
    {
        if (slotPrefab == null || availableElements.Count <= 0)
        {
            Debug.LogError("Missing required references!");
            return;
        }

        totalRows = visibleRows + extraSpawnRows;
        grid = new Slot[columns, totalRows];

        InitializeElementCounts();
        InitializeGrid();
    }

    void InitializeElementCounts()
    {
        foreach (Element element in availableElements)
        {
            elementCounts[element] = 0;
        }
    }

    void InitializeGrid()
    {
        // Fill only visible rows initially
        for (int col = 0; col < columns; col++)
        {
            for (int row = 0; row < visibleRows; row++)
            {
                SpawnSlotAt(col, row);
            }
        }
    }

    void SpawnSlotAt(int col, int row)
    {
        Element element = GetBalancedRandomElement();
        Vector2Int gridPos = new Vector2Int(col, row);

        Slot slot = new Slot(element, gridPos);
        grid[col, row] = slot;
        activeSlots.Add(slot);
        elementCounts[element]++;

        // Create visual
        Vector3 worldPos = GridToWorldPosition(col, row);
        GameObject slotObj = Instantiate(slotPrefab, worldPos, Quaternion.identity, transform);
        slotObj.name = $"Slot[{col}][{row}]";

        slot.Holder = slotObj;

        SlotHolder holder = slotObj.GetComponent<SlotHolder>();
        if (holder != null)
        {
            holder.LinkedSlot = slot;
        }

        slot.scaleAnimator = slotObj.GetComponent<ScaleAnimator>();

        UpdateSlotVisual(slot);
    }

    Element GetBalancedRandomElement()
    {
        if (availableElements.Count == 0) return null;

        // Calculate total slots and ideal distribution
        int totalSlots = activeSlots.Count;
        float idealPercentage = 1f / availableElements.Count;

        // Build weighted list
        List<Element> weightedElements = new List<Element>();

        foreach (Element element in availableElements)
        {
            int currentCount = elementCounts[element];
            float currentPercentage = totalSlots > 0 ? (float)currentCount / totalSlots : 0;

            // Calculate weight: elements below ideal percentage get higher weight
            float weight = Mathf.Max(0.1f, idealPercentage - currentPercentage + 0.1f);
            int weightMultiplier = Mathf.CeilToInt(weight * 10);

            for (int i = 0; i < weightMultiplier; i++)
            {
                weightedElements.Add(element);
            }
        }

        return weightedElements[Random.Range(0, weightedElements.Count)];
    }

    public void DestroySlots(List<Slot> slotsToDestroy)
    {
        if (isProcessingGravity) return;

        StartCoroutine(DestroyAndFillRoutine(slotsToDestroy));
    }

    IEnumerator DestroyAndFillRoutine(List<Slot> slotsToDestroy)
    {
        isProcessingGravity = true;

        // Destroy slots
        foreach (Slot slot in slotsToDestroy)
        {
            if (slot != null && slot.Holder != null)
            {
                elementCounts[slot.CurrentElement]--;
                activeSlots.Remove(slot);
                grid[slot.GridPosition.x, slot.GridPosition.y] = null;
                Destroy(slot.Holder);
            }
        }

        yield return new WaitForSeconds(0.1f);

        // Calculate how many new slots we need per column
        int[] newSlotsNeeded = new int[columns];
        for (int col = 0; col < columns; col++)
        {
            newSlotsNeeded[col] = CountEmptyInColumn(col);
        }

        // Spawn new slots in the extra rows above, matching the number needed
        for (int col = 0; col < columns; col++)
        {
            if (newSlotsNeeded[col] > 0)
            {
                // Spawn exactly the number of slots needed, starting from the top
                for (int i = 0; i < newSlotsNeeded[col]; i++)
                {
                    int spawnRow = totalRows - 1 - i; // Start from top extra row and work down
                    if (grid[col, spawnRow] == null)
                    {
                        SpawnSlotAt(col, spawnRow);
                        grid[col, spawnRow].IsFalling = true;
                    }
                }
            }
        }

        // Apply gravity until everything settles
        bool anyColumnChanged = true;

        while (anyColumnChanged)
        {
            anyColumnChanged = false;

            for (int col = 0; col < columns; col++)
            {
                if (ApplyGravityToColumn(col))
                {
                    anyColumnChanged = true;
                }
            }

            if (anyColumnChanged)
            {
                yield return StartCoroutine(AnimateFallingSlots());
                yield return new WaitForSeconds(0.05f);
            }
        }

        isProcessingGravity = false;
    }

    int CountEmptyInColumn(int col)
    {
        int count = 0;
        for (int row = 0; row < visibleRows; row++)
        {
            if (grid[col, row] == null)
            {
                count++;
            }
        }
        return count;
    }

    bool ApplyGravityToColumn(int col)
    {
        bool columnChanged = false;

        // Start from bottom and work up
        for (int row = 1; row < totalRows; row++)
        {
            if (grid[col, row] != null && grid[col, row - 1] == null)
            {
                // Move slot down one row
                Slot slot = grid[col, row];
                grid[col, row] = null;
                grid[col, row - 1] = slot;
                slot.UpdateGridPosition(new Vector2Int(col, row - 1));
                slot.IsFalling = true;
                columnChanged = true;
            }
        }

        return columnChanged;
    }

    IEnumerator AnimateFallingSlots()
    {
        List<Slot> fallingSlots = activeSlots.Where(s => s.IsFalling).ToList();

        if (fallingSlots.Count == 0) yield break;

        Dictionary<Slot, Vector3> startPositions = new Dictionary<Slot, Vector3>();
        Dictionary<Slot, Vector3> targetPositions = new Dictionary<Slot, Vector3>();

        foreach (Slot slot in fallingSlots)
        {
            startPositions[slot] = slot.Holder.transform.position;
            targetPositions[slot] = GridToWorldPosition(slot.GridPosition.x, slot.GridPosition.y);
        }

        float elapsed = 0f;

        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fallDuration);
            float curveValue = fallCurve.Evaluate(t);

            foreach (Slot slot in fallingSlots)
            {
                if (slot.Holder != null)
                {
                    slot.Holder.transform.position = Vector3.Lerp(
                        startPositions[slot],
                        targetPositions[slot],
                        curveValue
                    );
                }
            }

            yield return null;
        }

        // Snap to final positions
        foreach (Slot slot in fallingSlots)
        {
            if (slot.Holder != null)
            {
                slot.Holder.transform.position = targetPositions[slot];
            }
            slot.IsFalling = false;
        }
    }

    Vector3 GridToWorldPosition(int col, int row)
    {
        float x = col * slotSpacing - (columns - 1) * slotSpacing / 2f;
        float y = row * slotSpacing - (visibleRows - 1) * slotSpacing / 2f;
        return new Vector3(x, y, 0) + gridOffset + transform.position;
    }

    void UpdateSlotVisual(Slot slot)
    {
        if (slot.Holder != null)
        {
            SpriteRenderer renderer = slot.Holder.GetComponent<SpriteRenderer>();
            if (renderer != null && slot.CurrentElement != null)
            {
                renderer.sprite = slot.CurrentElement.Sprite;
                renderer.color = slot.CurrentElement.Color;
            }
        }
    }

    public Slot GetSlotAt(int col, int row)
    {
        if (col >= 0 && col < columns && row >= 0 && row < totalRows)
        {
            return grid[col, row];
        }
        return null;
    }

    public List<Slot> GetActiveSlots()
    {
        return activeSlots;
    }
}