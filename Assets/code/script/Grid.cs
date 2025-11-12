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
            yield return StartCoroutine(PerformScroll(1));
        }
    }

    public IEnumerator PerformScroll(int times)
    {
        isScrolling = true;
        pathing.ForceEndPathing();

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
            for (int col = 0; col < SlotNumber; col++)
            {
                int columnIndex = col;
                yield return new WaitForSeconds(columnScrollDelay);
                StartCoroutine(ScrollColumnAllBoards(columnIndex, allBoards));
            }
            if (scrollCount >= times - 1)
            {
                yield return new WaitForSeconds(scrollDuration);
            }
        }

        for (int i = 0; i < times; i++)
        {
            allBoards[i].DestroyBoard();
        }

        currentBoard = allBoards[times];

        Vector3 actualCurrentPos = new Vector3(
            -XDistanceBetween * (SlotNumber - 1) / 2f,
            -YDistanceBetween * (SlotNumber - 1) / 2f,
            0
        ) + transform.position;

        currentBoard.BoardWorldPosition = actualCurrentPos;

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

            foreach (BoardConfiguration board in boards)
            {
                board.MoveColumn(columnIndex, deltaMove);
            }

            yield return null;
        }
    }

    public void DeleteElements(List<Slot> slotsToDelete)
    {
        if (slotsToDelete == null || slotsToDelete.Count == 0 || emptyElement == null)
            return;

        List<int> affectedColumns = new List<int>();

        foreach (Slot slot in slotsToDelete)
        {
            Vector2Int pos = slot.GridPosition;
            if (pos.x >= 0 && pos.x < SlotNumber && pos.y >= 0 && pos.y < SlotNumber)
            {
                currentBoard.columns[pos.x].RemoveElement(pos.y, emptyElement);
                affectedColumns.Add(pos.x);
            }
        }

        currentBoard.UpdateVisuals();
        nextBoard.UpdateVisuals();

        foreach (int columnIndex in affectedColumns)
        {
            StartCoroutine(ApplyGravityToColumn(columnIndex));
        }
    }

    IEnumerator ApplyGravityToColumn(int columnIndex)
    {
        if (columnIndex < 0 || columnIndex >= SlotNumber) yield break;

        //Merge both board columns into a single array
        Slot[] mergedSlots = new Slot[SlotNumber * 2];

        for (int i = 0; i < SlotNumber; i++)
        {
            mergedSlots[i] = currentBoard.columns[columnIndex].slots[i];
        }

        for (int i = 0; i < SlotNumber; i++)
        {
            mergedSlots[SlotNumber + i] = nextBoard.columns[columnIndex].slots[i];
        }

        bool changesMade;
        do
        {
            changesMade = false;

            for (int i = 0; i < mergedSlots.Length - 1; i++)
            {
                Slot currentSlot = mergedSlots[i];
                Slot aboveSlot = mergedSlots[i + 1];

                // If current slot is empty 
                if (currentSlot.CurrentElement != null &&
                    currentSlot.CurrentElement.Type == ElementType.EMPTY &&
                    aboveSlot.CurrentElement != null)
                {

                    Element fallingElement = aboveSlot.CurrentElement;

                    yield return StartCoroutine(AnimateSlotFall(aboveSlot, currentSlot, fallingElement));

                    currentSlot.SetElement(fallingElement);
                    aboveSlot.SetElement(emptyElement);

                    changesMade = true;
                }
            }

        } while (changesMade);

        //fill empty slots
        for (int i = 0; i < mergedSlots.Length; i++)
        {
            if (mergedSlots[i].CurrentElement != null &&
                mergedSlots[i].CurrentElement.Type == ElementType.EMPTY)
            {
                Element randomElement = availableelements[Random.Range(0, availableelements.Count)];
                mergedSlots[i].SetElement(randomElement);
            }
        }

        currentBoard.UpdateVisuals();
        nextBoard.UpdateVisuals();
    }

    IEnumerator AnimateSlotFall(Slot fromSlot, Slot toSlot, Element fallingElement)
    {
        if (fromSlot.Holder == null || toSlot.Holder == null) yield break;

        Vector3 startPos = fromSlot.Holder.transform.position;
        Vector3 endPos = toSlot.Holder.transform.position;

        SpriteRenderer toRenderer = toSlot.Holder.GetComponent<SpriteRenderer>();
        if (toRenderer != null)
        {
            toRenderer.enabled = false;
        }

        SpriteRenderer fromRenderer = fromSlot.Holder.GetComponent<SpriteRenderer>();
        if (fromRenderer != null)
        {
            fromRenderer.sprite = fallingElement.Sprite;
            fromRenderer.color = fallingElement.Color;
            fromRenderer.enabled = true;
        }

        float elapsed = 0f;
        float duration = gravityDuration * currentFallSpeedMultiplier;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float curveValue = gravityCurve.Evaluate(t);

            if (fromSlot.Holder != null)
            {
                fromSlot.Holder.transform.position = Vector3.Lerp(startPos, endPos, curveValue);
            }

            yield return null;
        }



        if (fromSlot.Holder != null)
        {
            fromSlot.Holder.transform.position = startPos;
        }
        if (toSlot.Holder != null)
        {
            toSlot.Holder.transform.position = endPos;
        }

        if (toRenderer != null)
        {
            toRenderer.sprite = fallingElement.Sprite;
            toRenderer.color = fallingElement.Color;
            toRenderer.enabled = true;
        }

        if (fromRenderer != null)
        {
            if (emptyElement != null)
            {
                fromRenderer.sprite = emptyElement.Sprite;
                fromRenderer.color = emptyElement.Color;
            }
            else
            {
                fromRenderer.enabled = false;
            }
        }
    }
    public float GetCurrentScrollTimer() { return waitTimer; }
    public float GetScrollTime() { return waitTime; }
}