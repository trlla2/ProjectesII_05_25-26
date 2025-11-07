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

public class BoardConfiguration
{
    const int SlotNumber = 4;

    public Slot[,] slots = new Slot[SlotNumber, SlotNumber];
    public Vector3 BoardWorldPosition;

    public List<GameObject> SlotObjects = new List<GameObject>();

    public void SpawnSlots(GameObject slotPrefab, float XdistanceBetween, float YdistanceBetween, Vector3 startPosition, Transform parent)
    {
        BoardWorldPosition = startPosition;

        for (int i = 0; i < SlotNumber; i++)
        {
            for (int j = 0; j < SlotNumber; j++)
            {
                Vector3 pos = startPosition + new Vector3(i * XdistanceBetween, j * YdistanceBetween, 0);
                GameObject slotObj = GameObject.Instantiate(slotPrefab, pos, Quaternion.identity, parent);
                slotObj.name = "Slot[" + i + "] [" + j + "]";

                if (slots[i, j] == null) { Debug.LogError("No valid slot"); return; }

                slotObj.GetComponent<SlotHolder>().LinkedSlot = slots[i, j];
                slots[i, j].Holder = slotObj;
                SlotObjects.Add(slotObj);
                slots[i,j].scaleAnimator = slotObj.GetComponent<ScaleAnimator>();
            }
        }
    }

    public void Initialize(List<Element> BoardElements)
    {
        for (int i = 0; i < SlotNumber; i++)
        {
            for (int j = 0; j < SlotNumber; j++)
            {
                Element randomElement = BoardElements[Random.Range(0, BoardElements.Count)];
                slots[i, j] = new Slot(randomElement, new Vector2Int(i, j));
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
                elements[i, j] = slots[i, j].CurrentElement;
                if (elements[i, j] == null) { Debug.LogError("No valid slots"); return null; }
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
                    slots[pos.x, pos.y].SetElement(spell.Recipe[i]);
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
                if (slots[i, j].Holder != null)
                {
                    SpriteRenderer renderer = slots[i, j].Holder.GetComponent<SpriteRenderer>();
                    if (renderer != null)
                    {
                        renderer.sprite = slots[i, j].CurrentElement.Sprite;
                        renderer.color = slots[i, j].CurrentElement.Color;
                    }
                }
            }
        }
    }

    public void MoveBoard(float offsetY)
    {
        foreach (GameObject slotObj in SlotObjects)
        {
            if (slotObj != null)
            {
                slotObj.transform.position += new Vector3(0, offsetY, 0);
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
    [SerializeField] private float scrollDuration = 1f;
    [SerializeField] private AnimationCurve scrollCurve;

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
        //create 2 boards
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

    IEnumerator ScrollRoutine()
    {
        while (true)
        {
            float waitTimer = 0f;
            while (waitTimer < waitTime)
            {
                waitTimer += Time.deltaTime;
                yield return null;
            }

            // Perform the scroll animation
            yield return StartCoroutine(PerformScroll());

            // Replace boards after scroll
            currentBoard.DestroyBoard();
            currentBoard = nextBoard;

            // Create new next board
            float boardHeight = YDistanceBetween * SlotNumber;
            nextBoard = CreateBoard(boardHeight);
        }
    }

    IEnumerator PerformScroll()
    {
        isScrolling = true;

        pathing.ForceEndPathing();

        float elapsed = 0f;
        float scrollDistance = -YDistanceBetween * SlotNumber;

        while (elapsed < scrollDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / scrollDuration);
            float curveValue = scrollCurve.Evaluate(t);

            // Calculate how much to move this frame
            float previousCurveValue = elapsed <= Time.deltaTime ? 0f : scrollCurve.Evaluate((elapsed - Time.deltaTime) / scrollDuration);
            float deltaMove = (curveValue - previousCurveValue) * scrollDistance;

            // Move both boards
            currentBoard.MoveBoard(deltaMove);
            nextBoard.MoveBoard(deltaMove);

            yield return null;
        }

        isScrolling = false;
    }
}