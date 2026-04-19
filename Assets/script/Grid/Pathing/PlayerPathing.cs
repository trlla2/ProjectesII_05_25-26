using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Grid no coneix a playerpahing, 
//Desde aquí gestionem la logica de pathing, validació i cast de spells,
//així com les visuals associades (linies, highlight dels elements seleccionats, etc)

//Les dependencies més importants son SpellEffectHandler, que gestiona els efectes i visuals de les spells un cop castades,
//i el SpellPathAnimator, que gestiona les animacions dels elements anant cap a l'enemic. Es desde aquí on cridem enemyHit

[RequireComponent(typeof(LineRenderer))]
public class PlayerPathing : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Grid grid;
    [SerializeField] private Transform cursorVisual;
    [SerializeField] private SpellEffectHandler spellEffectHandler;
    [SerializeField] private AudioSource sfxLoop;

    [Header("Pathing Settings")]
    [SerializeField] private int maxPathLength = 6;
    [SerializeField] private float selectScaleAmount = 0.2f;
    [SerializeField] private float selectScaleTime = 0.2f;
    [SerializeField] private float xDistanceBetween = 1.25f;
    [SerializeField] private float castCooldown = 0.25f;

    [SerializeField] private SpellPathAnimator spellPathAnimator;


    [Header("Tutorial")]
    [SerializeField] private TutorialManager tutorialManager;
    //taking decoupuling seriously, mb a bit overboard? But it does make for a robust system so i ketp it this way :)
    private IInputHandler inputHandler;
    private IPathValidator pathValidator;
    private ISpellMatcher spellMatcher;
    private PathData pathData;
    private PathVisualizer pathVisualizer;

    private bool isPathing = false;
    private bool canPath = true;
    private bool isQTEActive = false;
    private float lastCastTime = -100000000.0f;

    public System.Action OnSpellCast;

    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        ValidateSetup();
        SubscribeToEvents();
        spellEffectHandler?.Initialize(tutorialManager);
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void InitializeComponents()
    {
        float adjacencyDistance = xDistanceBetween;

        inputHandler = new MouseInputHandler();
        pathValidator = new WorldSpacePathValidator(grid, adjacencyDistance);
        spellMatcher = new SpellMatcher();

        pathData = new PathData(maxPathLength);

        pathVisualizer = gameObject.AddComponent<PathVisualizer>();
        pathVisualizer.cursorVisual = cursorVisual;
    }

    private void ValidateSetup()
    {
        if (grid == null) { return; }

        if (grid.GridData != null)
        {
            pathVisualizer.Initialize(grid, null, pathData);
        }
    }

    private void SubscribeToEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CombatHandler.OnEndBattle += HandleBattleEnd;
        }

        Orb1QTE.OnQTEStarted += HandleQTEStarted;
        Orb1QTE.OnQTEEnded += HandleQTEEnded;

        if (grid != null)
        {
            grid.OnRerollStarted += HandleRerollStarted;
            grid.OnEnemySpellStarted += HandleEnemySpellStarted;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CombatHandler.OnEndBattle -= HandleBattleEnd;
        }

        Orb1QTE.OnQTEStarted -= HandleQTEStarted;
        Orb1QTE.OnQTEEnded -= HandleQTEEnded;

        if (grid != null)
        {
            grid.OnRerollStarted -= HandleRerollStarted;
            grid.OnEnemySpellStarted -= HandleEnemySpellStarted;
        }
    }

    private void HandleRerollStarted()
    {
        ForceEndPathing();
    }

    private void HandleEnemySpellStarted()
    {
        ForceEndPathing();
    }

    private void HandleQTEStarted()
    {
        isQTEActive = true;
        
        // Force end any active pathing
        if (isPathing)
        {
            ForceEndPathing();
        }
    }

    private void HandleQTEEnded()
    {
        isQTEActive = false;
    }

    private void Update()
    {
        if (grid.CurrentState != GridStateType.Idle)
        {
            if (isPathing)
                ForceEndPathing();
            return;
        }

        if (isQTEActive)
            return;

        if (!canPath)
            return;

        // Disable pathing during cooldown
        if (Time.time < lastCastTime + castCooldown)
        {
            grid.VisualGrid.ShowInactiveGrid(true);
            return;
        }

        HandleInput();
        UpdateVisuals();
    }

    private void FixedUpdate()
    {
        if (pathVisualizer != null)
        {
            pathVisualizer.UpdateCursorPosition(isPathing);
        }
    }

    private void HandleInput()
    {
        if (grid.CurrentState != GridStateType.Idle) return;

        if (inputHandler.IsPathingInputDown())
        {
            BeginPathing();
        }
        else if (inputHandler.IsPathingInputHeld() && isPathing)
        {
            ContinuePathing();
        }
        else if (inputHandler.IsPathingInputUp() && isPathing)
        {
            EndPathing();
        }
    }

    private void BeginPathing()
    {
        if (grid.CurrentState != GridStateType.Idle)
            return;

        isPathing = true;
        pathData.Clear();
        pathVisualizer.ShowCursor(true);
        sfxLoop.Play();
    }

    private void ContinuePathing()
    {
        ElementVisual visual = inputHandler.GetElementVisualFromInput(Camera.main);


        if (visual == null)
            return;

        if (!visual.CanSelect)
            return;

        Vector2Int gridPosition = visual.GridPosition;

        if (!CanSelectPosition(gridPosition))
            return;

        if (!pathData.Contains(gridPosition))
        {
            if (TryAddToPath(gridPosition))
            {
                pathVisualizer.HighlightElement(gridPosition, true);
                AnimateElementSelection(gridPosition);
            }
        }
        else if (pathData.Count >= 2 && gridPosition == pathData.GetSecondToLast())
        {
            TryBacktrack();
        }
    }

    private void EndPathing()
    {
        isPathing = false;

        TryCastSpell();

        ClearPath();
        pathVisualizer.ClearVisuals();
        sfxLoop.Pause();
    }



    private bool TryAddToPath(Vector2Int position)
    {
        if (pathData.Count == pathData.MaxLength)
            return false;

        if (pathData.Count > 0)
        {
            if (!pathValidator.CanAddToPath(position, pathData.Positions))
                return false;
        }

        return pathData.TryAdd(position);
    }

    private void TryBacktrack()
    {
        if (pathData.TryRemoveLast(out Vector2Int removedPosition))
        {
            pathVisualizer.HighlightElement(removedPosition, false);
        }
    }

    private void ClearPath()
    {
        Vector2Int[] positions = pathData.Positions;

        foreach (Vector2Int pos in positions)
        {
            pathVisualizer.HighlightElement(pos, false);

        }

        pathData.Clear();
    }

    private void TryCastSpell()
    {
        if (pathData.Count == 0)
            return;

        // Tutorial; reject the entire path if it doesn't match the allowed set
        if (!CanCastPath(new List<Vector2Int>(pathData.Positions)))
            return;

        List<Element> pathElements = pathData.GetElements(grid);

        List<Spell> availableSpells = GetAvailableSpells();

        Spell matchedSpell = spellMatcher.FindMatchingSpell(pathElements, availableSpells);

        if (matchedSpell != null)
        {
            lastCastTime = Time.time;
            ExecuteSpell(matchedSpell, pathElements);
            OnSpellCast?.Invoke();
        }
    }

    private void ExecuteSpell(Spell spell, List<Element> pathElements)
    {
        Vector2Int[] positions = pathData.Positions;
        List<Vector2Int> posList = new List<Vector2Int>(positions);

        // Notify blocker enemy of used positions (for block breaking)
        NotifyBlockerEnemyOfUsedPositions(posList);

        grid.InstantVisualEmpty(posList, chargeOrb: true);

        if (spellPathAnimator != null)
        {
            spellPathAnimator.OnSpellCast(positions, spell.Recipe, () =>
            {
                StartCoroutine(CompleteSpellWhenGridReady(spell, positions));
            });
        }
        else
        {
            CompleteSpellExecution(spell, positions);
        }
    }

    private void NotifyBlockerEnemyOfUsedPositions(List<Vector2Int> usedPositions)
    {
        if (GameManager.Instance == null || GameManager.Instance.CombatHandler == null)
            return;

        EnemyBehaviour enemy = GameManager.Instance.CombatHandler.GetEnemyRef();
        if (enemy == null)
            return;

        enemy.NotifyAdjacentElementsUsed(usedPositions);
    }

    private IEnumerator CompleteSpellWhenGridReady(Spell spell, Vector2Int[] positions)
    {
        while (grid.CurrentState != GridStateType.Idle)
            yield return null;

        CompleteSpellExecution(spell, positions);
    }


    private void CompleteSpellExecution(Spell spell, Vector2Int[] positions)
    {
        List<Vector2Int> positionsList = new List<Vector2Int>(positions);
        grid.DeleteElementsByPlayer(positionsList);

        HandleSpellEffects(spell);
    }

    private void HandleSpellEffects(Spell spell)
    {
        if (spellEffectHandler != null)
        {
            spellEffectHandler.HandleSpellEffects(spell);
            return;
        }

        // Fallback if handler is not assigned
        GameManager.Instance.NotifyPlayerMove();
        if (tutorialManager != null)
            tutorialManager.NotifySpellCast();
    }

    private void UpdateVisuals()
    {
        if (!isPathing && pathData.Count > 0)
        {
            pathData.Clear();
        }

        pathVisualizer.UpdateLineRenderer();
    }

    private void AnimateElementSelection(Vector2Int position)
    {
        ElementVisual visual = grid.GetElementVisualAt(position);
        if (visual != null)
        {
            StartCoroutine(ScaleElementRoutine(visual));
        }
    }

    private IEnumerator ScaleElementRoutine(ElementVisual visual)
    {
        Vector3 originalScale = visual.transform.localScale;
        Vector3 targetScale = originalScale * (1f + selectScaleAmount);

        float elapsed = 0f;
        while (elapsed < selectScaleTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / selectScaleTime;
            visual.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < selectScaleTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / selectScaleTime;
            visual.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        visual.transform.localScale = originalScale;
    }

    public void ForceEndPathing()
    {
        if (!isPathing)
            return;

        isPathing = false;
        ClearPath();
        pathVisualizer.ClearVisuals();
    }

    public void SetPathingEnabled(bool enabled)
    {
        canPath = enabled;

        if (!enabled && isPathing)
        {
            ForceEndPathing();
        }
    }

    private void HandleBattleEnd(bool isWin, int exp, int g, Spell drop)
    {   
        SetPathingEnabled(false);
    }

    private List<Spell> GetAvailableSpells()
    {
        return GameManager.Instance.PlayerStats.GetSpellBook();
    }
    //just for the tutorial
    private bool CanSelectPosition(Vector2Int pos)
    {
        if (tutorialManager != null && tutorialManager.IsTutorialActive())
        {
            return tutorialManager.IsPositionAllowed(pos);
        }
        return true;
    }

    //just for the tutorial
    private bool CanCastPath(List<Vector2Int> path)
    {
        if (tutorialManager != null && tutorialManager.IsTutorialActive())
            return tutorialManager.IsPathAllowed(path);
        return true;
    }
}