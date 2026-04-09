using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public const int GridSize = 4;

    [Header("Configuration")]
    [SerializeField] private List<Element> availableElements;
    [SerializeField] private Element emptyElement;
    [SerializeField] private Element bombElement;    
    [SerializeField] private List<Spell> availableSpells;

    [Header("References")]
    [SerializeField] private VisualGrid visualGrid;
    [SerializeField] private SpellPathAnimator enemySpellAnimator;
    public ExplosionTrigger explosionTrigger;

    [Header("Animation Settings")]
    [SerializeField] private AnimationCurve enemyBlinkCurve;
    [SerializeField] private float enemyDeletionDuration = 1f;
    [SerializeField] private float blockDuration = 1f;

    [Header("Tutorial")]
    [SerializeField] private TutorialManager tutorialManager;

    public event System.Action OnRerollStarted;
    public event System.Action OnRerollCompleted;
    public event System.Action OnEnemySpellStarted;
    public event System.Action<GridStateType> OnStateChanged;

    public void NotifyRerollStarted() => OnRerollStarted?.Invoke();
    public void NotifyRerollCompleted() => OnRerollCompleted?.Invoke();

    public AnimationCurve EnemyTargetFillCurve => enemyBlinkCurve;
    public float EnemyTargetAnimDuration => enemyDeletionDuration;

    public SpellPathAnimator EnemySpellAnimator => enemySpellAnimator;
    private GridData gridData;
    private GridGravity gravityHandler;
    private ISpellInserter spellInserter;
    private IGridState currentState;

    public GridStateType CurrentState => currentState?.GetStateType() ?? GridStateType.Idle;
    public GridData GridData => gridData;
    public GridGravity GravityHandler => gravityHandler;
    public Element EmptyElement => emptyElement;
    public Element BombElement => bombElement;         
    public List<Element> AvailableElements => availableElements;
    public VisualGrid VisualGrid => visualGrid;

    public List<Element> SpawnableElements => availableElements.FindAll(e => GetEffectiveSpawnWeight(e) > 0f);

    private float GetEffectiveSpawnWeight(Element element)
    {
        if (GridSpawnRegistry.TryGetWeightOverride(element.Type, out float overrideWeight))
            return overrideWeight;
        return element.spawnableInGrid ? element.spawnWeight : 0f;
    }

    //Picks a random element from the list using each element's wheighs
    public Element PickWeightedRandom(List<Element> elements)
    {
        float totalWeight = 0f;
        foreach (Element e in elements)
            totalWeight += GetEffectiveSpawnWeight(e);

        if (totalWeight <= 0f)
            return elements[Random.Range(0, elements.Count)];

        float roll = Random.Range(0f, totalWeight);
        foreach (Element e in elements)
        {
            roll -= GetEffectiveSpawnWeight(e);
            if (roll <= 0f)
                return e;
        }

        return elements[elements.Count - 1];
    }

    private void Awake()
    {
        Initialize(availableElements, emptyElement);
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
        visualGrid.Initialize(this);
        RandomizeBoard(SpawnableElements);

        if (GameManager.Instance != null)
        {
            InsertSpell(GameManager.Instance.PlayerStats.GetSpellBook()[(int)Random.Range(0, GameManager.Instance.PlayerStats.GetSpellBook().Count)].Recipe);
            Debug.Log("Inserting");
        }
        else
        {
            StartCoroutine(WaitNextFrameForSpells());
        }

        ChangeState(new IdleState(this));
        visualGrid.FillEmpty(GetEmptyPositions(), 0.5f);
    }

    IEnumerator WaitNextFrameForSpells()
    {
        yield return new WaitForEndOfFrame(); 
        if (GameManager.Instance != null)
        {
            InsertSpell(GameManager.Instance.PlayerStats.GetSpellBook()[(int)Random.Range(0, GameManager.Instance.PlayerStats.GetSpellBook().Count)].Recipe);
            Debug.Log("Inserting");
        }
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

    public void InstantVisualEmpty(List<Vector2Int> positions, bool chargeOrb = false)
    {
        //Only charge orb if explicitly requested, so  called from playerpathing
        if (chargeOrb && GameManager.Instance.CombatHandler.isBossBattle)
        {
            CollectAndChargeWeakElements(positions);
        }

        foreach (Vector2Int pos in positions)
        {
            ElementVisual visual = gridData.GetVisualAt(pos);
            if (visual != null)
            {
                visual.StopHoverAnimation();
                visual.highlighter?.SetHighlight(false);

                if (visual.IsTargetedByEnemy)
                {
                    visual.SetTargetedByEnemy(false);
                    visual.ClearOverrideSprite();
                }
            }
            gridData.SetElementAt(pos, emptyElement);
            if (visualGrid.GetVisualAt(pos) != null)
            {
                visualGrid.GetVisualAt(pos).UpdateVisual(emptyElement);
            }
        }
    }

    public void DeleteElementsByPlayer(List<Vector2Int> positions)
    {
        if (positions == null || positions.Count == 0)
            return;


        foreach (var pos in positions)
        {
            gridData.SetElementAt(pos, emptyElement);
            if (visualGrid.GetVisualAt(pos) != null)
            {
                visualGrid.GetVisualAt(pos).UpdateVisual(emptyElement);
            }

            if (visualGrid != null)
            {
                ElementVisual visual = gridData.GetVisualAt(pos);
                if (visual != null)
                {
                    if (visual.IsTargetedByEnemy)
                    {
                        visual.ClearTargetVisual();
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
                    if (visual.IsTargetedByEnemy)
                    {
                        visual.ClearTargetVisual();
                        visual.SetTargetedByEnemy(false);
                    }

                    Destroy(visual.gameObject);
                    gridData.SetVisualAt(pos, null);
                }
            }
        }

        if (CurrentState == GridStateType.Idle)
        {
            TryStartRefill();
        }
        else
        {
            StartCoroutine(DeferredRefill());
        }
    }

    private IEnumerator DeferredRefill()
    {
        while (CurrentState != GridStateType.Idle)
            yield return null;

        TryStartRefill();
    }

    public void BlockElements(float duration, List<Vector2Int> positions)
    {
        if (positions == null || positions.Count == 0)
            return;

        StartCoroutine(BlockElementsRoutine(duration, positions));
    }

    public void Reroll(int scrollTimes = 5)
    {
        if (CurrentState == GridStateType.Idle || CurrentState == GridStateType.ValidateSpell || CurrentState == GridStateType.Processing)
        {
            List<Spell> spellsToUse = availableSpells;

            if (GameManager.Instance != null && GameManager.Instance.PlayerStats.GetSpellBook() != null
                && GameManager.Instance.PlayerStats.GetSpellBook().Count > 0)
            {
                spellsToUse = GameManager.Instance.PlayerStats.GetSpellBook();
            }

            ChangeState(new RerollState(this, visualGrid, SpawnableElements, spellsToUse, scrollTimes));
        }
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

    public void ProcessEnemySpell(List<Vector2Int> positions, System.Action onComplete = null, bool rerollAfter = false)
    {
        if (positions == null || positions.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        // Notify before any state change so PlayerPathing can clean up
        OnEnemySpellStarted?.Invoke();

        if (CurrentState != GridStateType.Idle)
        {
            StartCoroutine(ProcessEnemySpellWhenReady(positions, onComplete, rerollAfter));
            return;
        }

        if (GetEmptyPositions().Count > 0)
        {
            ChangeState(new RefillState(this, visualGrid,
                onRefillComplete: () =>
                {
                    ChangeState(new EnemySpellState(this, visualGrid, positions, onComplete, rerollAfter));
                }
            ));
            return;
        }

        ChangeState(new EnemySpellState(this, visualGrid, positions, onComplete, rerollAfter));
    }

    private IEnumerator ProcessEnemySpellWhenReady(List<Vector2Int> positions, System.Action onComplete, bool rerollAfter)
    {
        while (CurrentState != GridStateType.Idle)
            yield return null;

        if (GetEmptyPositions().Count > 0)
        {
            ChangeState(new RefillState(this, visualGrid,
                onRefillComplete: () =>
                {
                    ChangeState(new EnemySpellState(this, visualGrid, positions, onComplete, rerollAfter));
                }
            ));
        }
        else
        {
            ChangeState(new EnemySpellState(this, visualGrid, positions, onComplete, rerollAfter));
        }
    }



    public void FillEmptySlots(List<Element> elements, List<Vector2Int> positionsToFill = null)
    {
        if (elements == null || elements.Count == 0)
            return;

        List<Vector2Int> positions = positionsToFill ?? GetEmptyPositions();

        bool useTutorialRefill = tutorialManager != null && tutorialManager.IsTutorialActive() && tutorialManager.HasRefillOverride();

        foreach (Vector2Int pos in positions)
        {
            if (!gridData.IsPositionValid(pos))
                continue;

            Element element;
            if (useTutorialRefill)
                element = tutorialManager.GetRefillElement(pos.x, pos.y);
            else
                element = PickWeightedRandom(elements);

            if (element == null)
                element = PickWeightedRandom(elements);

            gridData.SetElementAt(pos, element);

            ElementVisual visual = gridData.GetVisualAt(pos);
            if (visual != null)
                visual.UpdateVisual(element);
            else if (visualGrid != null && visualGrid.isInitialized)
                visualGrid.EnsureVisualAt(pos, element);
        }

        if (useTutorialRefill)
            tutorialManager.ConsumeRefillOverride();
    }

    private IEnumerator BlockElementsRoutine(float duration, List<Vector2Int> positions)
    {
        foreach (var pos in positions)
        {
            ElementVisual visual = gridData.GetVisualAt(pos);
            if (visual != null)
            {
                visual.SetSelectable(false);
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

    private void CollectAndChargeWeakElements(List<Vector2Int> positions)
    {
        BossManager bossManager = GameManager.Instance.CombatHandler.bossManager;
        if (bossManager == null)
            return;

        ElementType weakElement = bossManager.WeakElement;

        List<Vector3> matchingStartPositions = new List<Vector3>();
        List<Element> matchingElements = new List<Element>();

        foreach (Vector2Int pos in positions)
        {
            if (!gridData.IsPositionValid(pos))
                continue;

            Element element = gridData.GetElementAt(pos);

            if (element != null && element != emptyElement && element.Type == weakElement)
            {
                ElementVisual visual = gridData.GetVisualAt(pos);
                if (visual != null)
                {
                    matchingStartPositions.Add(visual.GetWorldPosition());
                    matchingElements.Add(element);
                }
            }
        }
        if (matchingElements.Count > 0)
        {
            bossManager.TryChargeOrb(matchingStartPositions, matchingElements);
        }
    }

    public void ChangeState(IGridState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
        OnStateChanged?.Invoke(currentState.GetStateType());
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
                Element randomElement = PickWeightedRandom(elements);
                gridData.SetElementAt(pos, randomElement);

                ElementVisual visual = gridData.GetVisualAt(pos);
                if (visual != null)
                {
                    visual.UpdateVisual(randomElement);
                }
                else if (visualGrid != null && visualGrid.isInitialized)
                {
                    visualGrid.EnsureVisualAt(pos, randomElement);
                }
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

    public void RetargetActiveSpell()
    {
        if (GameManager.Instance == null || GameManager.Instance.CombatHandler == null)
            return;
        EnemyBehaviour enemy = GameManager.Instance.CombatHandler.GetEnemyRef();
        if (enemy == null || enemy.ActiveSpell == null)
            return;
        enemy.ActiveSpell.RetargetAfterGravity(this);
    }

    public void TryStartRefill()
    {
        if (CurrentState != GridStateType.Idle)
            return;

        if (GetEmptyPositions().Count == 0)
            return;

        ChangeState(new RefillState(this, visualGrid));
    }

    public SpellPathAnimator GetEnemySpellAnimator()
    {
        return enemySpellAnimator;
    }

    public bool IsTutorialActive()
    {
        return tutorialManager != null && tutorialManager.IsTutorialActive();
    }

    // This method can be used by the tutorial to insert specific spells
    public bool TryInsertRandomSpell(List<Spell> spells)
    {
        if (spells == null || spells.Count == 0) return false;
        Spell spell = spells[Random.Range(0, spells.Count)];
        return spellInserter.TryInsertSpell(gridData, spell.Recipe);
    }

    public void ProcessEnemySwap(List<(Vector2Int, Vector2Int)> pairs, float swapDuration, float previewDuration = 0.35f)
    {
        if (CurrentState != GridStateType.Idle) return;
        if (pairs == null || pairs.Count == 0) return;

        ChangeState(new SwapState(this, visualGrid, pairs, previewDuration, swapDuration));
    }
}