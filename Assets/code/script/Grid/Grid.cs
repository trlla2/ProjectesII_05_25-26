using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEditor.PlayerSettings;

public class Grid : MonoBehaviour
{
    public const int GridSize = 4;

    [Header("Configuration")]
    [SerializeField] private List<Element> availableElements;
    [SerializeField] private Element emptyElement;
    [SerializeField] private List<Spell> availableSpells;

    [Header("References")]
    [SerializeField] private VisualGrid visualGrid;
    [SerializeField] private SpellPathAnimator enemySpellAnimator;
    
    [Header("Animation Settings")]
    [SerializeField] private AnimationCurve enemyBlinkCurve;
    [SerializeField] private float enemyDeletionDuration = 1f;
    [SerializeField] private float blockDuration = 1f;

    private GridData gridData;
    private GridGravity gravityHandler;
    private ISpellInserter spellInserter;
    private IGridState currentState;


    public GridStateType CurrentState => currentState?.GetStateType() ?? GridStateType.Idle;
    public GridData GridData => gridData;
    public GridGravity GravityHandler => gravityHandler;
    public Element EmptyElement => emptyElement;
    public List<Element> AvailableElements => availableElements;

    private void Awake()
    {
        Initialize(availableElements, emptyElement);
        visualGrid.Initialize(this);
    }

    private void Update()
    {
        currentState?.Update();
    }

    public void Initialize(List<Element> elements, Element empty)
    {
        availableElements = elements;
        emptyElement = empty;

        InitializeComponents();
        RandomizeBoard(availableElements);

        ChangeState(new IdleState(this));
        visualGrid.FillEmpty(GetEmptyPositions(), 0.5f);
    }


    public void InsertSpell(List<Element> spellRecipe)
    {
        if (spellRecipe == null || spellRecipe.Count == 0)
            return;

        bool success = spellInserter.TryInsertSpell(gridData, spellRecipe);
    }

    public void SwapElements(Vector2Int posA, Vector2Int posB)
    {
        if (CurrentState != GridStateType.Idle)
            return;

        gridData.SwapElements(posA, posB);
        visualGrid.Swap(posA, posB, 0.3f);
    }
    
    public void InstantVisualEmpty(List<Vector2Int> positions)
    {
        foreach (Vector2Int pos in positions)
        {
            ElementVisual visual = gridData.GetVisualAt(pos);
            if (visual.IsTargetedByEnemy)
            {
                visual.StopBlinkingAnimation();
                visual.SetTargetedByEnemy(false);
            }
                gridData.SetElementAt(pos, emptyElement);
            visualGrid.GetVisualAt(pos).UpdateVisual(emptyElement);
        }
    }

    public void DeleteElementsByPlayer(List<Vector2Int> positions)
    {
        if (positions == null || positions.Count == 0)
            return;

        foreach (var pos in positions)
        {
            // mark logical element as empty immediately
            gridData.SetElementAt(pos, emptyElement);
            visualGrid.GetVisualAt(pos).UpdateVisual(emptyElement);

            // remove visual immediately so it's gone before gravity runs
            if (visualGrid != null)
            {
                ElementVisual visual = gridData.GetVisualAt(pos);
                if (visual != null)
                {
                    // If this visual was targeted by the enemy, stop blinking and clear target
                    if (visual.IsTargetedByEnemy)
                    {
                        print("HERE");
                        visual.StopBlinkingAnimation();
                        visual.SetTargetedByEnemy(false);
                    }

                    visualGrid.RemoveVisualAt(pos);
                }
            }
            else
            {
                ElementVisual visual = gridData.GetVisualAt(pos);
                if (visual != null)
                {
                    //If this visual was targeted by the enemy, stop blinking and 
                    if (visual.IsTargetedByEnemy)
                    {
                        visual.StopBlinkingAnimation();
                        visual.SetTargetedByEnemy(false);
                    }

                    Destroy(visual.gameObject);
                    gridData.SetVisualAt(pos, null);
                }
            }
        }

        ChangeState(new RefillState(this, visualGrid));
    }

    public void StartDeleteElementsByEnemies(float duration, List<Vector2Int> positions)
    {
        if (positions == null || positions.Count == 0)
            return;

        StartCoroutine(EnemyDeletionRoutine(duration, positions));
    }

    public void BlockElements(float duration, List<Vector2Int> positions)
    {
        if (positions == null || positions.Count == 0)
            return;

        StartCoroutine(BlockElementsRoutine(duration, positions));
    }

    public void LeverPull(List<Element> elements)
    {
        if (CurrentState != GridStateType.Idle)
            return;

        ChangeState(new LeverPullState(this, visualGrid, elements, PlayerStats.Instance.GetSpellBook()));
    }

    public Element GetElementAt(Vector2Int position)
    {
        return gridData.GetElementAt(position);
    }

    public void SetElementAt(Vector2Int position, Element element)
    {
        gridData.SetElementAt(position, element);
    }

    public Element[,] GetAllElements()
    {
        return gridData.GetAllElements();
    }

    public bool IsPositionValid(Vector2Int position)
    {
        return gridData.IsPositionValid(position);
    }

    public ElementVisual GetElementVisualAt(Vector2Int position)
    {
        return gridData.GetVisualAt(position);
    }

    public void SetElementVisualAt(Vector2Int position, ElementVisual visual)
    {
        gridData.SetVisualAt(position, visual);
    }


    public void ChangeState(IGridState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }

    private void InitializeComponents()
    {
        gridData = new GridData(GridSize);
        gravityHandler = new GridGravity(gridData, emptyElement);
        spellInserter = new SpellInserter();
    }

    public void RandomizeBoard(List<Element> elements)
    {
        if (elements == null || elements.Count == 0)
            return;

        for (int x = 0; x < GridSize; x++)
        {
            for (int y = 0; y < GridSize; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                Element randomElement = elements[Random.Range(0, elements.Count)];
                gridData.SetElementAt(pos, randomElement);

                ElementVisual visual = gridData.GetVisualAt(pos);
                if (visual != null)
                {
                    visual.UpdateVisual(randomElement);
                }
                else if (visualGrid != null && visualGrid.isInitialized)
                {
                    // ensure a visual exists for newly populated slot (only if VisualGrid is initialized)
                    visualGrid.EnsureVisualAt(pos, randomElement);
                }
            }
        }
    }

    public void FillEmptySlots(List<Element> elements)
    {
        if (elements == null || elements.Count == 0)
            return;

        List<Vector2Int> emptyPositions = GetEmptyPositions();

        foreach (var pos in emptyPositions)
        {
            Element randomElement = elements[Random.Range(0, elements.Count)];
            gridData.SetElementAt(pos, randomElement);

            ElementVisual visual = gridData.GetVisualAt(pos);
            if (visual != null)
            {
                visual.UpdateVisual(randomElement);
            }
            else if (visualGrid != null && visualGrid.isInitialized)
            {
                // create visual for newly spawned element and anchor it (only if VisualGrid is initialized)
                visualGrid.EnsureVisualAt(pos, randomElement);
            }
        }
    }
    private List<Vector2Int> GetEmptyPositions()
    {
        List<Vector2Int> emptyPositions = new List<Vector2Int>();

        for (int x = 0; x < GridSize; x++)
        {
            for (int y = 0; y < GridSize; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                Element element = gridData.GetElementAt(pos);

                if (element == emptyElement || element == null)
                {
                    emptyPositions.Add(pos);
                }
            }
        }

        return emptyPositions;
    }

    private IEnumerator EnemyDeletionRoutine(float duration, List<Vector2Int> positions)
    {
        foreach (var pos in positions)
        {
            ElementVisual visual = gridData.GetVisualAt(pos);
            if (visual != null)
            {
                visual.SetTargetedByEnemy(true);

                Element element = gridData.GetElementAt(pos);
                if (element != null)
                {
                    visual.StartBlinkingAnimation(duration, enemyBlinkCurve);
                }
            }
        }

        yield return new WaitForSeconds(duration);

        // Filter out positions where elements were already destroyed by player
        List<Vector2Int> validPositions = new List<Vector2Int>();
        List<Element> spellElements = new List<Element>();

        foreach (var pos in positions)
        {
            Element elem = gridData.GetElementAt(pos);
            ElementVisual visual = gridData.GetVisualAt(pos);

            // Only include positions that still have non-empty elements
            if (elem != null && elem != emptyElement && visual != null)
            {
                validPositions.Add(pos);
                spellElements.Add(elem);
            }
            else
            {
                // If element was already destroyed, clean up any remaining visual state
                if (visual != null && visual.IsTargetedByEnemy)
                {
                    visual.StopBlinkingAnimation();
                    visual.SetTargetedByEnemy(false);
                }
            }
        }

        // Only proceed with deletion if there are valid positions remaining
        if (validPositions.Count > 0)
        {
            foreach (var pos in validPositions)
            {
                ElementVisual visual = gridData.GetVisualAt(pos);
                if (visual != null)
                {
                    visual.StopBlinkingAnimation();
                    visual.SetTargetedByEnemy(false);
                    visual.PlayDeletionEffect();

                    if (visualGrid != null)
                        visualGrid.RemoveVisualAt(pos);
                    else
                    {
                        Destroy(visual.gameObject);
                        gridData.SetVisualAt(pos, null);
                    }
                }

                gridData.SetElementAt(pos, emptyElement);
            }

            // Pass only valid positions and elements to the animator
            if (enemySpellAnimator != null && validPositions.Count > 0)
            {
                enemySpellAnimator.OnSpellCast(validPositions.ToArray(), spellElements, () =>
                {
                    ChangeState(new RefillState(this, visualGrid));
                });
            }
            else
            {
                ChangeState(new RefillState(this, visualGrid));
            }
        }
        else
        {
            Debug.Log("All targeted elements were destroyed by player before enemy spell could cast");
            //maybe do something special here?
            ChangeState(new RefillState(this, visualGrid));
        }
    }

    private IEnumerator BlockElementsRoutine(float duration, List<Vector2Int> positions)
    {
        foreach (var pos in positions)
        {
            ElementVisual visual = gridData.GetVisualAt(pos);
            if (visual != null)
            {
                visual.SetSelectable(false);
                //future: add block visual effect
            }
        }

        yield return new WaitForSeconds(duration);

        foreach (var pos in positions)
        {
            ElementVisual visual = gridData.GetVisualAt(pos);
            if (visual != null)
            {
                visual.SetSelectable(true);
            }
        }
    }
}