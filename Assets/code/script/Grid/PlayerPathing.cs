using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PlayerPathing : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Grid grid;
    [SerializeField] private Transform cursorVisual;

    [Header("Pathing Settings")]
    [SerializeField] private int maxPathLength = 6;
    [SerializeField] private float cursorLerpSpeed = 5f;
    [SerializeField] private float selectScaleAmount = 0.2f;
    [SerializeField] private float selectScaleTime = 0.2f;
    [SerializeField] private float xDistanceBetween = 1.25f;

    [SerializeField] private SpellPathAnimator spellPathAnimator;

    private IInputHandler inputHandler;
    private IPathValidator pathValidator;
    private ISpellMatcher spellMatcher;

    private PathData pathData;
    private PathVisualizer pathVisualizer;

    private bool isPathing = false;
    private bool canPath = true;

    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        ValidateSetup();
        SubscribeToEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void InitializeComponents()
    {
        float adjacencyDistance = xDistanceBetween * 1.47f;//Slightly more than diagonal distance sqrt(2)

        inputHandler = new MouseInputHandler();
        pathValidator = new WorldSpacePathValidator(grid, adjacencyDistance); 
        spellMatcher = new SpellMatcher();

        pathData = new PathData(maxPathLength);

        pathVisualizer = gameObject.AddComponent<PathVisualizer>();
    }

    private void ValidateSetup()
    {
        if (grid == null){ return;}

        if (grid.GridData != null)
        {
            pathVisualizer.Initialize(grid, null, pathData);
        }
    }

    private void SubscribeToEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnEndBattle += HandleBattleEnd;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnEndBattle -= HandleBattleEnd;
        }
    }

    private void Update()
    {
        if (!canPath)
            return;

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
    }

    private void ContinuePathing()
    {
        ElementVisual visual = inputHandler.GetElementVisualFromInput(Camera.main);


        if (visual == null)
            return;

        if (!visual.CanSelect)
            return;

        Vector2Int gridPosition = visual.GridPosition;

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
    }



    private bool TryAddToPath(Vector2Int position)
    {
        if (pathData.IsFull)
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

        List<Element> pathElements = pathData.GetElements(grid);

        List<Spell> availableSpells = GetAvailableSpells();

        Spell matchedSpell = spellMatcher.FindMatchingSpell(pathElements, availableSpells);

        if (matchedSpell != null)
        {
            ExecuteSpell(matchedSpell, pathElements);
        }
    }

    private void ExecuteSpell(Spell spell, List<Element> pathElements)
    {
        Debug.Log($"PlayerPathing: Casting spell - {spell.Name}");

        Vector2Int[] positions = pathData.Positions;
        List<Vector2Int> posList = new List<Vector2Int>(positions);

        if (spellPathAnimator != null)
        {
            spellPathAnimator.OnSpellCast(positions, spell.Recipe, () =>
            {
                CompleteSpellExecution(spell, positions);
            });

            grid.InstantVisualEmpty(posList);
        }
        else
        {
            CompleteSpellExecution(spell, positions);
            grid.InstantVisualEmpty(posList);
        }
    }


    private void CompleteSpellExecution(Spell spell, Vector2Int[] positions)
    {
        List<Vector2Int> positionsList = new List<Vector2Int>(positions);
        grid.DeleteElementsByPlayer(positionsList);

        HandleSpellEffects(spell);
    }

    //Can be converted into a SpellEffectHandler class in the future if more complex effects are needed
    private void HandleSpellEffects(Spell spell)
    {
        foreach(Element e in spell.Recipe)
            GameManager.Instance.AttackEnemy(e.Type);

        if (spell.Name == "HEAL") GameManager.Instance.HealPlayer(30); 
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

    private void HandleBattleEnd(bool isWin, int exp, int money)
    {
        SetPathingEnabled(false);
    }

    private List<Spell> GetAvailableSpells()
    {
        return GameManager.Instance.GetPlayerSpellBook();
    }

}