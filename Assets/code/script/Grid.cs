using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slot
{
    public Element CurrentElement;
    public Vector2Int GridPosition;
    public GameObject Holder;

    public ScaleAnimator scaleAnimator;
    public bool Animated = false;

    public Slot(Element e, Vector2Int pos)
    {
        CurrentElement = e;
        GridPosition = pos;
    }

    public void SetElement(Element e)
    {
        CurrentElement = e;
    }

    public void DestroySlot()
    {
        GameObject.Destroy(Holder);
    }
}

public class Column
{
    public Slot[] slots;
    public int columnIndex;
    private int slotCount;

    public Column(int columnIdx, int size)
    {
        columnIndex = columnIdx;
        slotCount = size;
        slots = new Slot[size];
    }

    public void Initialize(List<Element> availableElements)
    {
        for (int i = 0; i < slotCount; i++)
        {
            Element randomElement = availableElements[Random.Range(0, availableElements.Count)];
            slots[i] = new Slot(randomElement, new Vector2Int(columnIndex, i));
        }
    }

    public void RemoveElement(int slotIndex, Element emptyElement)
    {
        if (slotIndex >= 0 && slotIndex < slotCount)
        {
            slots[slotIndex].SetElement(emptyElement);
        }
    }

    public List<int> GetEmptySlotIndices()
    {
        List<int> emptyIndices = new List<int>();
        for (int i = 0; i < slotCount; i++)
        {
            if (slots[i].CurrentElement != null && slots[i].CurrentElement.Type == ElementType.EMPTY)
            {
                emptyIndices.Add(i);
            }
        }
        return emptyIndices;
    }
}

public class BoardConfiguration
{
    const int SlotNumber = 4;

    public Column[] columns = new Column[SlotNumber];
    public Vector3 BoardWorldPosition;

    public List<GameObject> SlotObjects = new List<GameObject>();

    public void Initialize(List<Element> BoardElements)
    {
        for (int i = 0; i < SlotNumber; i++)
        {
            columns[i] = new Column(i, SlotNumber);
            columns[i].Initialize(BoardElements);
        }
    }

    public void SpawnSlots(GameObject slotPrefab, float XdistanceBetween, float YdistanceBetween, Vector3 startPosition, Transform parent)
    {
        BoardWorldPosition = startPosition;

        for (int i = 0; i < SlotNumber; i++)
        {
            for (int j = 0; j < SlotNumber; j++)
            {
                Vector3 pos = startPosition + new Vector3(i * XdistanceBetween, j * YdistanceBetween, 0);
                GameObject slotObj = GameObject.Instantiate(slotPrefab, pos, Quaternion.identity, parent);
                slotObj.name = "Slot[" + i + "][" + j + "]";

                if (columns[i].slots[j] == null) { Debug.LogError("No valid slot"); return; }

                slotObj.GetComponent<SlotHolder>().LinkedSlot = columns[i].slots[j];
                columns[i].slots[j].Holder = slotObj;
                SlotObjects.Add(slotObj);
                columns[i].slots[j].scaleAnimator = slotObj.GetComponent<ScaleAnimator>();
            }
        }
    }

    public Element[,] GetElements()
    {
        Element[,] elements = new Element[SlotNumber, SlotNumber];

        for (int i = 0; i < SlotNumber; i++)
        {
            for (int j = 0; j < SlotNumber; j++)
            {
                elements[i, j] = columns[i].slots[j].CurrentElement;
            }
        }
        return elements;
    }

    public void InsertSpell(Spell spell)
    {
        if (spell == null || spell.Recipe == null || spell.Recipe.Count == 0) { Debug.LogWarning("Invalid spell for insertion"); return; }

        Vector2Int[] directions = new Vector2Int[] {
            new Vector2Int(0, 1),
            new Vector2Int(1, 1),
            new Vector2Int(1, 0),
            new Vector2Int(1, -1),
            new Vector2Int(0, -1),
            new Vector2Int(-1, -1),
            new Vector2Int(-1, 0),
            new Vector2Int(-1, 1)
        };

        int maxAttempts = 100;
        int attempts = 0;
        bool spellInserted = false;

        while (!spellInserted && attempts < maxAttempts)
        {
            attempts++;

            int startX = Random.Range(0, SlotNumber);
            int startY = Random.Range(0, SlotNumber);

            Vector2Int dir = directions[Random.Range(0, directions.Length)];
            Vector2Int previousDir = new Vector2Int(-100, -100);

            bool canFit = true;
            List<Vector2Int> positions = new List<Vector2Int>();

            for (int i = 0; i < spell.Recipe.Count; i++)
            {
                int x = startX + dir.x * i;
                int y = startY + dir.y * i;

                if (x < 0 || x >= SlotNumber || y < 0 || y >= SlotNumber)
                {
                    canFit = false;
                    break;
                }

                positions.Add(new Vector2Int(x, y));
                previousDir = dir;

                dir = directions[Random.Range(0, directions.Length)];
                while (dir != -previousDir)
                {
                    dir = directions[Random.Range(0, directions.Length)];
                }
            }

            if (canFit)
            {
                for (int i = 0; i < spell.Recipe.Count; i++)
                {
                    Vector2Int pos = positions[i];
                    columns[pos.x].slots[pos.y].SetElement(spell.Recipe[i]);
                }
                spellInserted = true;
            }
        }

        if (!spellInserted)
        {
            Debug.LogError("CANT INSERT SPELL!!");
        }
    }

    public void UpdateVisuals()
    {
        for (int i = 0; i < SlotNumber; i++)
        {
            for (int j = 0; j < SlotNumber; j++)
            {
                if (columns[i].slots[j].Holder != null && columns[i].slots[j].CurrentElement != null)
                {
                    SpriteRenderer renderer = columns[i].slots[j].Holder.GetComponent<SpriteRenderer>();
                    if (renderer != null)
                    {
                        renderer.sprite = columns[i].slots[j].CurrentElement.Sprite;
                        renderer.color = columns[i].slots[j].CurrentElement.Color;
                    }
                }
            }
        }
    }

    public void MoveColumn(int columnIndex, float offsetY)
    {
        for (int j = 0; j < SlotNumber; j++)
        {
            if (columns[columnIndex].slots[j].Holder != null)
            {
                columns[columnIndex].slots[j].Holder.transform.position += new Vector3(0, offsetY, 0);
            }
        }
    }

    public void DestroyBoard()
    {
        foreach (GameObject slotObj in SlotObjects)
        {
            if (slotObj != null)
            {
                GameObject.Destroy(slotObj);
            }
        }
        SlotObjects.Clear();
    }
}

public class Grid : MonoBehaviour
{
    const int SlotNumber = 4;

    [Header("Generation")]
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private float XDistanceBetween = 1.1f;
    [SerializeField] private float YDistanceBetween = 1.1f;

    [Header("Scroll Settings")]
    [SerializeField] private float waitTime = 4f;
    private float waitTimer = 0;
    [SerializeField] private float scrollDuration = 1f;
    [SerializeField] private float columnScrollDelay = 0.1f;
    [SerializeField] private AnimationCurve scrollCurve;

    [Header("Gravity Settings")]
    [SerializeField] private AnimationCurve gravityCurve;
    [SerializeField] private float gravityDuration = 0.5f;

    [Header("Elements")]
    [SerializeField] private Element emptyElement;
    [SerializeField] private List<Element> availableelements;
    public List<Spell> availableSpells = new List<Spell>();

    private BoardConfiguration currentBoard;
    private BoardConfiguration nextBoard;

    [SerializeField] private PlayerPathing pathing;

    [HideInInspector] public float currentFallSpeedMultiplier = 1f;
    [HideInInspector] public bool isScrolling = false;
    [HideInInspector] public bool IsPlayerPathing;

    private void Start()
    {
        if (slotPrefab == null || availableelements.Count <= 0 || availableSpells.Count <= 0) return;

        InitializeBoards();
        StartCoroutine(ScrollRoutine());
    }

    void InitializeBoards()
    {
        currentBoard = CreateBoard(0f);
        float boardHeight = YDistanceBetween * SlotNumber;
        nextBoard = CreateBoard(boardHeight);
    }

    BoardConfiguration CreateBoard(float yPosition)
    {
        BoardConfiguration newBoard = new BoardConfiguration();
        newBoard.Initialize(availableelements);

        Spell randomSpell = availableSpells[Random.Range(0, availableSpells.Count)];
        newBoard.InsertSpell(randomSpell);

        Vector3 startPos = new Vector3(
            -XDistanceBetween * (SlotNumber - 1) / 2f,
            yPosition + -YDistanceBetween * (SlotNumber - 1) / 2f,
            0
        );
        startPos += transform.position;

        newBoard.SpawnSlots(slotPrefab, XDistanceBetween, YDistanceBetween, startPos, transform);
        newBoard.UpdateVisuals();

        return newBoard;
    }

    public void ResetWaitTimer()
    {
        waitTimer = 0f;
    }

    IEnumerator ScrollRoutine()
    {
        while (true)
        {
            waitTimer = 0f;
            while (waitTimer < waitTime)
            {
                waitTimer += Time.deltaTime;
                yield return null;
            }

            // Default to 1 scroll
            yield return StartCoroutine(PerformScroll(3));
        }
    }

    public IEnumerator PerformScroll(int times)
    {
        isScrolling = true;
        pathing.ForceEndPathing();

        // Create all necessary boards before scrolling
        List<BoardConfiguration> allBoards = new List<BoardConfiguration>();
        allBoards.Add(currentBoard);
        allBoards.Add(nextBoard);

        for (int i = 2; i <= times; i++)
        {
            float Height = YDistanceBetween * SlotNumber * i;
            BoardConfiguration newBoard = CreateBoard(Height);
            allBoards.Add(newBoard);
        }

        for (int scrollCount = 0; scrollCount < times; scrollCount++)
        {
            // Scroll each column with delay
            for (int col = 0; col < SlotNumber; col++)
            {
                int columnIndex = col;
                yield return new WaitForSeconds(columnScrollDelay);
                StartCoroutine(ScrollColumnAllBoards(columnIndex, allBoards));
            }

            // Wait for all columns to finish scrolling 
            // If more than 1 time we won't wait the full duration
            if (scrollCount >= times - 1)
            {
                yield return new WaitForSeconds(scrollDuration);
            }
        }

        // Now destroy old boards
        for (int i = 0; i < times; i++)
        {
            allBoards[i].DestroyBoard();
        }

        // Assign the correct board as current
        currentBoard = allBoards[times];

        // Update BoardWorldPosition to match actual position after scrolls
        // The board has scrolled down 'times' times, so it's now at position 0
        Vector3 actualCurrentPos = new Vector3(
            -XDistanceBetween * (SlotNumber - 1) / 2f,
            -YDistanceBetween * (SlotNumber - 1) / 2f,
            0
        ) + transform.position;

        currentBoard.BoardWorldPosition = actualCurrentPos;

        // Create new next board at correct position
        float boardHeight = YDistanceBetween * SlotNumber;
        nextBoard = CreateBoard(boardHeight);

        isScrolling = false;
    }

    IEnumerator ScrollColumnAllBoards(int columnIndex, List<BoardConfiguration> boards)
    {
        float elapsed = 0f;
        float scrollDistance = -YDistanceBetween * SlotNumber;

        while (elapsed < scrollDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / scrollDuration);
            float curveValue = scrollCurve.Evaluate(t);

            float previousCurveValue = elapsed <= Time.deltaTime ? 0f : scrollCurve.Evaluate((elapsed - Time.deltaTime) / scrollDuration);
            float deltaMove = (curveValue - previousCurveValue) * scrollDistance;

            // Move the column in ALL boards
            foreach (BoardConfiguration board in boards)
            {
                board.MoveColumn(columnIndex, deltaMove);
            }

            yield return null;
        }
    }
    public void DeleteElement(int columnIndex, int slotIndex)
    {
        if (columnIndex >= 0 && columnIndex < SlotNumber && emptyElement != null)
        {
            currentBoard.columns[columnIndex].RemoveElement(slotIndex, emptyElement);
            StartCoroutine(ApplyGravityToColumn(columnIndex));
        }
    }

    IEnumerator ApplyGravityToColumn(int columnIndex)
    {
        bool hasMoved = true;

        // Keep applying gravity until no more moves can be made
        while (hasMoved)
        {
            hasMoved = false;

            // Check both boards as one continuous column (current board is bottom, next board is top)
            // First handle movement within current board (bottom to top)
            for (int i = 0; i < SlotNumber - 1; i++)
            {
                Slot currentSlot = currentBoard.columns[columnIndex].slots[i];
                Slot aboveSlot = currentBoard.columns[columnIndex].slots[i + 1];

                // If current slot is empty and slot above has an element
                if (currentSlot.CurrentElement != null && 
                    currentSlot.CurrentElement.Type == ElementType.EMPTY &&
                    aboveSlot.CurrentElement != null &&
                    aboveSlot.CurrentElement.Type != ElementType.EMPTY)
                {
                    // Animate the fall from i+1 to i
                    yield return StartCoroutine(AnimateSlotFall(currentBoard, columnIndex, i + 1, i));

                    // Swap elements
                    currentSlot.SetElement(aboveSlot.CurrentElement);
                    aboveSlot.SetElement(emptyElement);

                    currentBoard.UpdateVisuals();
                    hasMoved = true;
                    // Continue to check for more moves - don't break here, but restart the whole gravity check
                    break;
                }
            }

            // If we moved something in current board, continue to next iteration to check again from bottom
            if (hasMoved) continue;

            // If we didn't move anything in current board, check the transition from next board to current board
            Slot topCurrentSlot = currentBoard.columns[columnIndex].slots[SlotNumber - 1];
            Slot bottomNextSlot = nextBoard.columns[columnIndex].slots[0];

            if (topCurrentSlot.CurrentElement != null &&
                topCurrentSlot.CurrentElement.Type == ElementType.EMPTY &&
                bottomNextSlot.CurrentElement != null &&
                bottomNextSlot.CurrentElement.Type != ElementType.EMPTY)
            {
                // Animate from next board bottom to current board top
                yield return StartCoroutine(AnimateSlotFallBetweenBoards(columnIndex));

                // Swap elements
                topCurrentSlot.SetElement(bottomNextSlot.CurrentElement);
                bottomNextSlot.SetElement(emptyElement);

                currentBoard.UpdateVisuals();
                nextBoard.UpdateVisuals();
                hasMoved = true;
                continue; // Check from bottom again
            }

            // If we still didn't move anything, check within next board
            for (int i = 0; i < SlotNumber - 1; i++)
            {
                Slot currentSlot = nextBoard.columns[columnIndex].slots[i];
                Slot aboveSlot = nextBoard.columns[columnIndex].slots[i + 1];

                if (currentSlot.CurrentElement != null && 
                    currentSlot.CurrentElement.Type == ElementType.EMPTY &&
                    aboveSlot.CurrentElement != null &&
                    aboveSlot.CurrentElement.Type != ElementType.EMPTY)
                {
                    // Animate the fall from i+1 to i
                    yield return StartCoroutine(AnimateSlotFall(nextBoard, columnIndex, i + 1, i));

                    // Swap elements
                    currentSlot.SetElement(aboveSlot.CurrentElement);
                    aboveSlot.SetElement(emptyElement);

                    nextBoard.UpdateVisuals();
                    hasMoved = true;
                    break; // Restart from checking current board
                }
            }
        }

        // Spawn new elements at top of next board for any remaining empty slots
        for (int i = SlotNumber - 1; i >= 0; i--)
        {
            Slot slot = nextBoard.columns[columnIndex].slots[i];
            if (slot.CurrentElement != null && slot.CurrentElement.Type == ElementType.EMPTY)
            {
                Element newElement = availableelements[Random.Range(0, availableelements.Count)];
                slot.SetElement(newElement);
            }
        }

        currentBoard.UpdateVisuals();
        nextBoard.UpdateVisuals();
    }

    IEnumerator AnimateSlotFall(BoardConfiguration board, int columnIndex, int fromSlot, int toSlot)
    {
        GameObject holder = board.columns[columnIndex].slots[fromSlot].Holder;
        Vector3 startPos = holder.transform.position;
        Vector3 endPos = board.columns[columnIndex].slots[toSlot].Holder.transform.position;

        float elapsed = 0f;
        while (elapsed < gravityDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / gravityDuration);
            float curveValue = gravityCurve.Evaluate(t);

            holder.transform.position = Vector3.Lerp(startPos, endPos, curveValue);
            yield return null;
        }

        holder.transform.position = endPos;
    }

    IEnumerator AnimateSlotFallBetweenBoards(int columnIndex)
    {
        GameObject holder = nextBoard.columns[columnIndex].slots[0].Holder;
        Vector3 startPos = holder.transform.position;
        Vector3 endPos = currentBoard.columns[columnIndex].slots[SlotNumber - 1].Holder.transform.position;

        float elapsed = 0f;
        while (elapsed < gravityDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / gravityDuration);
            float curveValue = gravityCurve.Evaluate(t);

            holder.transform.position = Vector3.Lerp(startPos, endPos, curveValue);
            yield return null;
        }

        holder.transform.position = endPos;
    }

    public float GetCurrentScrollTimer() { return waitTimer; }
    public float GetScrollTime() { return waitTime; }
}