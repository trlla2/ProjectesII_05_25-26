using System.Collections.Generic;
using UnityEngine;

public class Slot
{
    public Element CurrentElement;
    public Vector2Int GridPosition;
    public GameObject Holder;

    public Slot(Element e, Vector2Int pos)
    {
        CurrentElement = e;
        GridPosition = pos;
    }

    public void SetElement(Element e)
    {
        CurrentElement = e;
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
                //the gameobject keyword is necessay bc we are outside monobehabiour context
                GameObject slotObj = GameObject.Instantiate(slotPrefab,pos, Quaternion.identity, parent);
                slotObj.name = "Slot["+i+"] ["+j+"]";

                if (slots[i, j] == null) { Debug.LogError("No valid slot"); return; }

                slotObj.GetComponent<SlotHolder>().LinkedSlot = slots[i, j];
                slots[i, j].Holder = slotObj;
                SlotObjects.Add(slotObj);
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
                if (elements[i, j] == null){Debug.LogError("No valid slots");return null;}
            }
        }
        return elements;
    }

    public void InsertSpell(Spell spell)
    {
        if (spell == null || spell.Recipe == null || spell.Recipe.Count == 0){ Debug.LogWarning("Invalid spell for insertion");return;}

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

        //failsafe 
        int maxAttempts = 100;
        int attempts = 0;
        bool spellInserted = false;

        //random spell insert
        while (!spellInserted && attempts < maxAttempts)
        {
            attempts++;

            int startX = Random.Range(0, SlotNumber);
            int startY = Random.Range(0, SlotNumber);

            Vector2Int dir = directions[Random.Range(0, directions.Length)];
            Vector2Int previousDir = new Vector2Int(-100, -100);


            bool canFit = true;
            List<Vector2Int> positions = new List<Vector2Int>();

            //for the spell length
            for (int i = 0; i < spell.Recipe.Count; i++)
            {
                //choose rand neighbour slot in dir
                int x = startX + dir.x * i;
                int y = startY + dir.y * i;

                if (x < 0 || x >= SlotNumber || y < 0 || y >= SlotNumber)
                {
                    canFit = false;
                    break;
                }

                //add to positions if valid pos
                positions.Add(new Vector2Int(x, y));
                previousDir = dir;

                //choose a diferent dir than the contrary one
                dir = directions[Random.Range(0, directions.Length)];
                while (dir != -previousDir)
                {
                    dir = directions[Random.Range(0, directions.Length)];
                }
            }

            if (canFit)
            {
                // Insert the spell
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

    public void MoveBoard(float speed)
    {
        BoardWorldPosition.y += speed;

        foreach (GameObject slotObj in SlotObjects)
        {
            if (slotObj != null)
            {
                slotObj.transform.position += new Vector3(0, speed, 0);
            }
        }
    }

    public void DestroyBoard()
    {
        foreach (GameObject slotObj in SlotObjects)
        {
            if (slotObj != null)
            {
                //GameObject flag necesary 4 some reason...
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

    [SerializeField] private int maxBoards = 3;
    [SerializeField] private float boardYOffset = 8f;
    [SerializeField] private float yDisappearThreshold = -10f;

    public float FallSpeed = 2f;

    [SerializeField] private List<Element> availableelements;
    public List<Spell> availableSpells = new List<Spell>();

    private List<BoardConfiguration> currentBoards = new List<BoardConfiguration>();

    [HideInInspector] public float currentFallSpeedMultiplier = 1f;
    private bool DrawingPath = false;

    public bool IsPlayerPathing
    {
        get { return DrawingPath; }
        set
        {
            DrawingPath = value;
            currentFallSpeedMultiplier = value ? 0.2f : 1f;
        }
    }

    private void Start()
    {
        if (slotPrefab == null || availableelements.Count <= 0 || availableSpells.Count <= 0) return;

        InitializeBoards();
    }

    void InitializeBoards()
    {
        float BoardHeight = YDistanceBetween * (SlotNumber - 1);
        for (int i = 0; i < maxBoards; i++)
        {
            SpawnNewBoard(i * BoardHeight);
        }
    }

    void SpawnNewBoard(float yPosition)
    {
        BoardConfiguration newBoard = new BoardConfiguration();

        newBoard.Initialize(availableelements);

        Spell randomSpell = availableSpells[Random.Range(0, availableSpells.Count)];
        newBoard.InsertSpell(randomSpell);

        Vector3 startPos = new Vector3(-XDistanceBetween * (SlotNumber - 1) / 2f, yPosition + -YDistanceBetween * (SlotNumber - 1) / 2f,0);
        startPos += transform.position;//Debug 
        newBoard.SpawnSlots(slotPrefab, XDistanceBetween, YDistanceBetween, startPos, transform);

        newBoard.UpdateVisuals();

        currentBoards.Add(newBoard);
    }

    private void Update()
    {
        GameLoop();
    }

    void GameLoop()
    {
        float moveAmount = -FallSpeed * currentFallSpeedMultiplier * Time.deltaTime;

        for (int i = currentBoards.Count - 1; i >= 0; i--)
        {
            currentBoards[i].MoveBoard(moveAmount);

            //Check if moved below the threshhold and delete
            if (currentBoards[i].BoardWorldPosition.y < yDisappearThreshold)
            {
                currentBoards[i].DestroyBoard();
                currentBoards.RemoveAt(i);
            }
        }

        //find current highest board and spawn above it
        if (currentBoards.Count < maxBoards)
        {
            float highestY = 0;
            for (int i = 0; i < currentBoards.Count; i++)
            {
                if (highestY < currentBoards[i].BoardWorldPosition.y) highestY = currentBoards[i].BoardWorldPosition.y;
            }

            SpawnNewBoard(highestY + boardYOffset);
        }
    }
}